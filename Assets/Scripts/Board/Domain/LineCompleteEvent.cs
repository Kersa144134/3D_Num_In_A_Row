// ======================================================
// LineCompleteEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-17
// 更新日時 : 2026-04-03
// 概要     : ライン成立イベントデータ
// ======================================================

using System.Collections.Generic;

namespace BoardSystem.Domain
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

        /// <summary>成立ラインごとの座標リスト</summary>
        public readonly IReadOnlyList<BoardIndex>[] LinePositions;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="player">プレイヤー番号</param>
        /// <param name="linePositions">成立ラインの座標配列</param>
        public LineCompleteEvent(
            in int player,
            in IReadOnlyList<BoardIndex>[] linePositions)
        {
            Player = player;
            LinePositions = linePositions;
        }
    }
}