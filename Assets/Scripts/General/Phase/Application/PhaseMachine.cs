// ======================================================
// PhaseMachine.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-16
// 概要     : フェーズ状態の実行および遷移制御を行うステートマシン
// ======================================================

using System;
using UniRx;
using PhaseSystem.Domain;

namespace PhaseSystem.Application
{
    /// <summary>
    /// フェーズ遷移と状態更新を制御するステートマシン
    /// </summary>
    public sealed class PhaseMachine
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ設定データ</summary>
        private readonly PhaseTransitionConfig _config;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在アクティブなフェーズ状態</summary>
        private IPhaseState _currentState;

        /// <summary>フェーズ遷移ルール</summary>
        private IPhaseTransitionRule _transitionRule;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在のフェーズ状態</summary>
        public IPhaseState CurrentState => _currentState;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>フェーズ変更通知</summary>
        private readonly Subject<PhaseChangeEvent> _onPhaseChanged;

        /// <summary>フェーズ変更通知</summary>
        public IObservable<PhaseChangeEvent> OnPhaseChanged => _onPhaseChanged;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// PhaseMachine を生成
        /// </summary>
        public PhaseMachine(
            IPhaseState initialState,
            PhaseTransitionConfig config)
        {
            // 初期状態設定
            _currentState = initialState;

            // 遷移ルール保持
            //_transitionRule = new IPhaseTransitionRule();

            // 設定保持
            _config = config;

            // イベント生成
            _onPhaseChanged = new Subject<PhaseChangeEvent>();

            // 初期状態開始
            _currentState.OnEnter();

            // 初期通知
            _onPhaseChanged.OnNext(
                new PhaseChangeEvent(null, _currentState)
            );
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        public void Update(float deltaTime)
        {
            // 現在状態更新
            _currentState.OnUpdate(deltaTime);

            // 遷移判定
            IPhaseState nextState =
                _transitionRule.Resolve(_currentState, deltaTime);

            if (nextState == null)
            {
                return;
            }

            // 前状態保持
            IPhaseState prevState = _currentState;

            // 終了処理
            _currentState.OnExit();

            // 状態更新
            _currentState = nextState;

            // 開始処理
            _currentState.OnEnter();

            // 通知
            _onPhaseChanged.OnNext(
                new PhaseChangeEvent(prevState, _currentState)
            );
        }
    }
}