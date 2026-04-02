// ======================================================
// LineCompleteEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-17
// 更新日時 : 2026-04-02
// 概要     : ライン成立イベントデータ
// ======================================================

using System.Collections.Generic;

namespace BoardSystem.Data
{
    /// <summary>
    /// ライン成立イベントデータ
    /// </summary>
    public readonly struct LineCompleteEvent
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>プレイヤー番号</summary>
        public readonly int Player;

        /// <summary>成立ラインごとのセル座標リスト</summary>
        public readonly IReadOnlyList<(int x, int y, int z)>[] LinePositions;

        /// <summary>成立ライン数</summary>
        public int LineCount
        {
            get { return LinePositions.Length; }
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LineCompleteEvent(int player, IReadOnlyList<(int x, int y, int z)>[] linePositions)
        {
            Player = player;
            LinePositions = linePositions;
        }
    }
}