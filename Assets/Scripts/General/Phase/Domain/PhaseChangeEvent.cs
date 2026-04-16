// ======================================================
// PhaseChangeEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-16
// 概要     : フェーズ遷移時に発行されるイベントデータ
// ======================================================

namespace PhaseSystem.Domain
{
    /// <summary>
    /// フェーズ変更イベント
    /// </summary>
    public readonly struct PhaseChangeEvent
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>遷移前のフェーズ状態</summary>
        private readonly IPhaseState _previousState;

        /// <summary>遷移後のフェーズ状態</summary>
        private readonly IPhaseState _currentState;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>遷移前のフェーズ状態を取得</summary>
        public IPhaseState PreviousState => _previousState;

        /// <summary>遷移後のフェーズ状態を取得</summary>
        public IPhaseState CurrentState => _currentState;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// フェーズ変更イベント生成
        /// </summary>
        /// <param name="previousState">遷移前の状態</param>
        /// <param name="currentState">遷移後の状態</param>
        public PhaseChangeEvent(
            IPhaseState previousState,
            IPhaseState currentState)
        {
            _previousState = previousState;
            _currentState = currentState;
        }
    }
}