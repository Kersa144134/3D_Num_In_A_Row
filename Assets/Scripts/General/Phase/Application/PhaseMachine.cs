// ======================================================
// PhaseMachine.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-16
// 概要     : フェーズ状態の実行および遷移制御を行うステートマシン
// ======================================================

using System;
using System.Collections.Generic;
using UniRx;
using PhaseSystem.Domain;
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
        private readonly PhaseStateRepository _stateRepository = new PhaseStateRepository();

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

        /// <summary>プレイヤー総数</summary>
        private readonly int _playerCount;

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

        /// <summary>現在プレイヤーインデックス取得用 Subject</summary>
        private readonly ReactiveProperty<int> _currentPlayerIndex;

        /// <summary>現在プレイヤーインデックスストリーム</summary>
        public IReadOnlyReactiveProperty<int> CurrentPlayerIndex => _currentPlayerIndex;

        /// <summary>Play フェーズへ遷移した回数</summary>
        public IReadOnlyReactiveProperty<int> PlayEnterCount => _transitionRule.PlayEnterCount;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public PhaseMachine(
            in PhaseTransitionConfig transitionConfig,
            in UpdatableContexts updatableContexts)
        {
            _transitionConfig = transitionConfig;
            _updatableContexts = updatableContexts;

            // --------------------------------------------------
            // 初期フェーズ設定
            // --------------------------------------------------
            _currentState = _stateRepository.GetPhaseState(PhaseType.None);

            // 開始処理
            _currentState.OnEnterState();

            // --------------------------------------------------
            // フェーズ遷移設定
            // --------------------------------------------------
            _transitionRule = new PhaseTransitionRule(_stateRepository, _transitionConfig);

            _maxLimitTime = _transitionConfig.PerPlayerLimitTime;

            // --------------------------------------------------
            // 初期プレイヤー設定
            // --------------------------------------------------
            _playerCount = _transitionConfig.PlayerCount;

            // 初期プレイヤーをランダムに設定
            int initialPlayerIndex = UnityEngine.Random.Range(1, _playerCount + 1);
            _currentPlayerIndex = new ReactiveProperty<int>(initialPlayerIndex);
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
            PhaseType nextPhaseType = _transitionRule.ResolveTimeTransition(_currentState);

            // 現在フェーズと同一ならフェーズ遷移なし
            if (nextPhaseType == _currentPhaseType.Value)
            {
                return;
            }

            // フェーズ遷移
            ChangePhase(nextPhaseType);
        }

        /// <summary>
        /// 外部からのフェーズ遷移リクエストを処理する
        /// </summary>
        /// <param name="nextPhaseType">要求された遷移先フェーズ</param>
        public void RequestChangePhase(in PhaseType nextPhaseType)
        {
            // 現在フェーズと要求フェーズから実際の遷移先を決定
            PhaseType resolvedPhaseType =
                _transitionRule.ResolveRequestedPhase(
                    _currentPhaseType.Value,
                    nextPhaseType);

            // 現在フェーズと同一ならフェーズ遷移なし
            if (resolvedPhaseType == _currentPhaseType.Value)
            {
                return;
            }
            
            // フェーズ遷移
            ChangePhase(resolvedPhaseType);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// フェーズ切替を行う
        /// </summary>
        private void ChangePhase(in PhaseType nextPhaseType)
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
            _updatableManagement.RebuildUpdatables(ResolvePhaseUpdatables());

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

            if (nextPhaseType is PhaseType.ChangePlayer)
            {
                NextPlayer();
            }
        }

        /// <summary>
        /// 現在のフェーズ定義に基づき、実行対象となる Updatable 配列を解決する
        /// </summary>
        /// <returns>実行対象となる Updatable 配列</returns>
        private IUpdatable[] ResolvePhaseUpdatables()
        {
            if (!(_currentState is IPhaseUpdatableDefinition definition))
            {
                return Array.Empty<IUpdatable>();
            }

            // フェーズが要求する Updatable 種別を取得
            UpdatableType[] types = definition.GetUpdatableTypes();

            List<IUpdatable> result = new List<IUpdatable>();

            for (int i = 0; i < types.Length; i++)
            {
                UpdatableType type = types[i];

                // コンテキストから対応する Updatable 取得
                IUpdatable updatable = _updatableContexts.Get(type);

                if (updatable == null)
                {
                    throw new InvalidOperationException($"Updatableが見つかりません。Type: {type}");
                }

                result.Add(updatable);
            }

            return result.ToArray();
        }

        /// <summary>
        /// 次のプレイヤーへ遷移
        /// </summary>
        private void NextPlayer()
        {
            // --------------------------------------------------
            // 1 ベースの循環処理
            // --------------------------------------------------
            // 0 ベースに変換
            int zeroBasedIndex = _currentPlayerIndex.Value - 1;

            // 循環処理
            zeroBasedIndex = (zeroBasedIndex + 1) % _playerCount;

            // 1 ベースに変換
            _currentPlayerIndex.Value = zeroBasedIndex + 1;
        }
    }
}