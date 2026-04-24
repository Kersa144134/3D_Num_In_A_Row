// ======================================================
// BoardRepositionResult.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : 駒再配置処理結果データ
// ======================================================

using System.Collections.Generic;

namespace BoardSystem.Domain
{
    /// <summary>
    /// 駒再配置結果
    /// </summary>
    public readonly struct BoardRepositionResult
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 移動情報
        /// </summary>
        public readonly IReadOnlyList<(BoardIndex from, BoardIndex to)> Moves;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardRepositionResult(
            IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            Moves = moves;
        }
    }
}