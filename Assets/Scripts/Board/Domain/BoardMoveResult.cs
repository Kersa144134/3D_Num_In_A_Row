// ======================================================
// BoardMoveResult.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-22
// 更新日時 : 2026-06-22
// 概要     : 盤面移動結果データ
// ======================================================

namespace BoardSystem.Domain
{
    /// <summary>
    /// 盤面移動結果
    /// </summary>
    public readonly struct BoardMoveResult
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 移動元座標
        /// </summary>
        public readonly BoardIndex From;

        /// <summary>
        /// 移動先座標
        /// </summary>
        public readonly BoardIndex To;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="from">移動元座標</param>
        /// <param name="to">移動先座標</param>
        public BoardMoveResult(
            in BoardIndex from,
            in BoardIndex to)
        {
            From = from;
            To = to;
        }
    }
}