// ======================================================
// StartButtonEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-02
// 更新日時 : 2026-04-02
// 概要     : スタートボタン押下イベントデータ
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// スタートボタン押下イベントデータ
    /// </summary>
    public readonly struct StartButtonEvent
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>押下されたフェーズ</summary>
        public readonly PhaseType Phase;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// StartButtonEvent の生成
        /// </summary>
        /// <param name="phase">押下時のフェーズ</param>
        public StartButtonEvent(in PhaseType phase)
        {
            Phase = phase;
        }
    }
}