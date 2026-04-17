// ======================================================
// PhaseChangeEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-17
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
        // プロパティ
        // ======================================================

        /// <summary>遷移前のフェーズ種別</summary>
        public PhaseType PreviousPhaseType { get; }

        /// <summary>遷移後のフェーズ種別</summary>
        public PhaseType NextPhaseType { get; }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// フェーズ変更イベント生成
        /// </summary>
        /// <param name="previousPhaseType">遷移前のフェーズ</param>
        /// <param name="nextPhaseType">遷移後のフェーズ</param>
        public PhaseChangeEvent(
            in PhaseType previousPhaseType,
            in PhaseType nextPhaseType)
        {
            PreviousPhaseType = previousPhaseType;
            NextPhaseType = nextPhaseType;
        }
    }
}