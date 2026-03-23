// ======================================================
// ColumnDropService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-03-23
// 概要     : 列への駒落下処理サービス
// ======================================================

using BoardSystem.Data;

namespace BoardSystem.Service
{
    /// <summary>
    /// 列落下処理サービス
    /// </summary>
    public sealed class ColumnDropService
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定列に駒を落下可能か判定
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <returns>落下可能なら true</returns>
        public bool CanDrop(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            int boardSize = board.GetSize();

            for (int y = boardSize - 1; y >= 0; y--)
            {
                if (board.Get(columnX, y, columnZ) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 駒を落下させる
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <param name="player">プレイヤー番号</param>
        /// <returns>落下した Y 座標（落下不可の場合は -1）</returns>
        public int Drop(
            in BoardState board,
            in int columnX,
            in int columnZ,
            in int player)
        {
            int boardSize = board.GetSize();

            for (int y = 0; y < boardSize; y++)
            {
                if (board.Get(columnX, y, columnZ) == 0)
                {
                    board.Set(columnX, y, columnZ, player);
                    return y;
                }
            }

            return -1;
        }
    }
}