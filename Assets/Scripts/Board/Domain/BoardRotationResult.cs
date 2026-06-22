// ======================================================
// RotationResult.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-26
// 概要     : 盤面回転処理の結果データ
// ======================================================

using System.Collections.Generic;

namespace BoardSystem.Domain
{
    /// <summary>
    /// 回転処理結果
    /// </summary>
    public readonly struct BoardRotationResult
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 回転による移動情報
        /// </summary>
        public readonly IReadOnlyList<BoardMoveResult> RotateMoves;

        /// <summary>
        /// 再配置による移動情報
        /// </summary>
        public readonly IReadOnlyList<BoardMoveResult> RepositionMoves;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 回転結果を初期化
        /// </summary>
        /// <param name="rotateMoves">回転移動</param>
        /// <param name="repositionMoves">再配置移動</param>
        public BoardRotationResult(
            IReadOnlyList<BoardMoveResult> rotateMoves,
            IReadOnlyList<BoardMoveResult> repositionMoves)
        {
            RotateMoves = rotateMoves;
            RepositionMoves = repositionMoves;
        }
    }
}