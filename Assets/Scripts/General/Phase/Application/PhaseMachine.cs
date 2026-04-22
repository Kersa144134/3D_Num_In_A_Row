// ======================================================
// PhaseMachine.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-16
// 概要     : フェーズ状態の実行および遷移制御を行うステートマシン
// ======================================================

using PhaseSystem.Domain;
using System;
using System.Collections.Generic;
using UniRx;
using UpdateSystem.Application;
using UpdateSystem.Domain;

namespace PhaseSystem.Application
{
    public sealed class PhaseMachine
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ状態リポジトリ</summary>
        private readonly PhaseStateRepository _stateRepository;

        /// <summary>遷移設定データ</summary>
        private readonly PhaseTransitionConfig _transitionConfig;

        /// <summary>遷移ルール</summary>
        private readonly PhaseTransitionRule _transitionRule;

        /// <summary>Update を管理するサービス</summary>
        private readonly UpdatableManagement _updatableManagement = new UpdatableManagement();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在のフェーズ状態</summary>
        private IPhaseState _currentState;

        /// <summary>現在のフェーズ状態</summary>
        private UpdatableContexts _updatableContexts;

        /// <summary>現在のフェーズの制限時間</summary>
        private float _maxLimitTime;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>現在のフェーズ</summary>
        private readonly ReactiveProperty<PhaseType> _currentPhaseType =
            new ReactiveProperty<PhaseType>(PhaseType.None);

        /// <summary>現在のフェーズ</summary>
        public IReadOnlyReactiveProperty<PhaseType> CurrentPhaseType => _currentPhaseType;

        /// <summary>フェーズ経過時間</summary>
        private readonly ReactiveProperty<float> _limitTime =
            new ReactiveProperty<float>(0.0f);

        /// <summary>フェーズ経過時間</summary>
        public IReadOnlyReactiveProperty<float> LimitTime => _limitTime;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public PhaseMachine(
            in PhaseType initialPhase,
            in PhaseTransitionConfig transitionConfig,
            in UpdatableContexts updatableContexts)
        {
            _transitionConfig = transitionConfig;
            _updatableContexts = updatableContexts;

            // --------------------------------------------------
            // フェーズ生成
            // --------------------------------------------------
            _stateRepository = new PhaseStateRepository(_transitionConfig);

            // --------------------------------------------------
            // 初期フェーズ設定
            // --------------------------------------------------
            _currentPhaseType.Value = initialPhase;
            _currentState = _stateRepository.GetPhaseState(initialPhase);

            // 開始処理
            _currentState.OnEnterState();

            // --------------------------------------------------
            // フェーズ遷移設定
            // --------------------------------------------------
            _transitionRule = new PhaseTransitionRule(_stateRepository, _transitionConfig);

            _maxLimitTime = _transitionConfig.PerPlayerLimitTime;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ進行の更新処理を行う
        /// </summary>
        /// <param name="unscaledDeltaTime">経過時間</param>
        public void OnUpdate(in float unscaledDeltaTime)
        {
            // Updatable 更新
            _updatableManagement.ExecuteUpdate(unscaledDeltaTime);

            // 状態更新
            _currentState.OnUpdateState(unscaledDeltaTime);

            // Play中のみ制限時間更新
            if (_currentState is PlayPhaseState playState)
            {
                _limitTime.Value = _maxLimitTime - playState.PlayElapsedTime;
            }
        }

        /// <summary>
        /// フェーズ進行の更新処理を行う
        /// </summary>
        /// <param name="unscaledDeltaTime">経過時間</param>
        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // Updatable 更新
            _updatableManagement.ExecuteLateUpdate(unscaledDeltaTime);
            
            // 状態更新
            _currentState.OnLateUpdateState(unscaledDeltaTime);

            // 遷移判定
            PhaseType nextPhase = _transitionRule.Resolve(_currentState);

            if (nextPhase == _currentPhaseType.Value)
            {
                return;
            }

            // フェーズ遷移
            ChangePhase(nextPhase);
        }

        /// <summary>
        /// フェーズ切替を行う
        /// </summary>
        public void ChangePhase(in PhaseType nextPhaseType)
        {
            // 遷移前フェーズを保持
            PhaseType previousPhaseType = _currentPhaseType.Value;

            // 終了処理
            _updatableManagement.ExecutePhaseExit(previousPhaseType);
            _currentState.OnExitState();

            // 新しい State 取得
            _currentPhaseType.Value = nextPhaseType;
            _currentState = _stateRepository.GetPhaseState(nextPhaseType);

            // Updatables 再構築
            _updatableManagement.RebuildUpdatables(Rebuild());

            // 開始処理
            _updatableManagement.ExecutePhaseEnter(nextPhaseType);
            if (_currentState is IPhaseEnterHandler handler)
            {
                handler.OnEnterState(previousPhaseType);
            }
            else
            {
                _currentState.OnEnterState();
            }
        }

        /// <summary>
        /// PlayPhaseState を取得する
        /// </summary>
        public PlayPhaseState GetPlayState()
        {
            return _stateRepository.GetPhaseState(PhaseType.Play) as PlayPhaseState;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        private IUpdatable[] Rebuild()
        {
            // --------------------------------------------------
            // フェーズ定義チェック
            // --------------------------------------------------
            if (!(_currentState is IPhaseUpdatableDefinition definition))
            {
                // フェーズが未定義の場合は空配列を返す
                return Array.Empty<IUpdatable>();
            }

            // --------------------------------------------------
            // フェーズが要求するUpdatable種別を取得
            // --------------------------------------------------
            UpdatableType[] types = definition.GetUpdatableTypes();

            // --------------------------------------------------
            // 解決結果リスト
            // --------------------------------------------------
            List<IUpdatable> result = new List<IUpdatable>();

            // --------------------------------------------------
            // enum → 実体解決
            // --------------------------------------------------
            for (int i = 0; i < types.Length; i++)
            {
                UpdatableType type = types[i];

                // コンテキストから対応するUpdatableを取得
                IUpdatable updatable = _updatableContexts.Get(type);

                // 結果リストへ追加
                result.Add(updatable);
            }

            // --------------------------------------------------
            // 配列化して返却
            // --------------------------------------------------
            return result.ToArray();
        }
    }
}