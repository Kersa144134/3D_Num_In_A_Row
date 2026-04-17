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

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在のフェーズ状態</summary>
        private IPhaseState _currentState;

        /// <summary>現在のフェーズ種別</summary>
        private PhaseType _currentPhaseType;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>フェーズ変更通知用 Subject</summary>
        private readonly Subject<PhaseChangeEvent> _onPhaseChanged = new Subject<PhaseChangeEvent>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseChangeEvent> OnPhaseChanged => _onPhaseChanged;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public PhaseMachine(
            PhaseType initialPhase,
            PhaseTransitionConfig config)
        {
            _transitionConfig = config;
            
            // --------------------------------------------------
            // フェーズ生成
            // --------------------------------------------------
            _stateRepository = new PhaseStateRepository(_transitionConfig);

            // --------------------------------------------------
            // 初期フェーズ設定
            // --------------------------------------------------
            _currentPhaseType = initialPhase;

            _currentState = _stateRepository.GetPhaseState(initialPhase);

            // 開始処理
            _currentState.OnEnter();

            // --------------------------------------------------
            // イベント設定
            // --------------------------------------------------
            // 初期通知
            _onPhaseChanged.OnNext(
                new PhaseChangeEvent(
                    PhaseType.None,
                    _currentPhaseType
                )
            );

            _transitionRule = new PhaseTransitionRule(_stateRepository, _transitionConfig);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ進行の更新処理を行う
        /// </summary>
        /// <param name="unscaledDeltaTime">経過時間</param>
        public void Update(float unscaledDeltaTime)
        {
            // 状態更新
            _currentState.OnUpdate(unscaledDeltaTime);

            // 遷移判定
            PhaseType nextPhase = _transitionRule.Resolve(_currentState, unscaledDeltaTime);

            // 遷移なし
            if (nextPhase == _currentPhaseType)
            {
                return;
            }

            // --------------------------------------------------
            // フェーズ遷移
            // --------------------------------------------------
            // 終了処理
            _currentState.OnExit();

            // 通知
            _onPhaseChanged.OnNext(
                new PhaseChangeEvent(
                    _currentPhaseType,
                    nextPhase
                )
            );

            // 新しい State 取得
            _currentPhaseType = nextPhase;

            _currentState = _stateRepository.GetPhaseState(nextPhase);

            // 開始処理
            _currentState.OnEnter();
        }

        /// <summary>
        /// PlayPhaseState を取得する
        /// </summary>
        public PlayPhaseState GetPlayState()
        {
            return _stateRepository.GetPhaseState(PhaseType.Play) as PlayPhaseState;
        }
    }
}