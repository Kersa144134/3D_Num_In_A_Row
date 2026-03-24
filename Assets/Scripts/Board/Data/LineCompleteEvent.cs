// ======================================================
// LineCompleteEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-17
// 更新日時 : 2026-03-17
// 概要     : ライン成立イベントデータ
// ======================================================

namespace BoardSystem.Data
{
    /// <summary>
    /// ライン成立イベントデータ
    /// </summary>
    public readonly struct LineCompleteEvent
    {
        /// <summary>プレイヤー番号</summary>
        public readonly int Player;

        /// <summary>成立ライン長配列</summary>
        public readonly int[] Lengths;

        /// <summary>成立ライン数</summary>
        public int LineCount
        {
            get { return Lengths.Length; }
        }

        public LineCompleteEvent(int player, int[] lengths)
        {
            Player = player;
            Lengths = lengths;
        }
    }
}