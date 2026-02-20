// ======================================================
// ColumnDropService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
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
        /// <param name="x">列X座標</param>
        /// <param name="z">列Z座標</param>
        /// <returns>落下可能ならtrue</returns>
        public bool CanDrop(BoardState board, int x, int z)
        {
            int boardSize = board.GetSize();

            for (int y = boardSize - 1; y >= 0; y--)
            {
                if (board.Get(x, y, z) == 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 駒を落下させる
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="x">列X座標</param>
        /// <param name="z">列Z座標</param>
        /// <param name="player">プレイヤー番号</param>
        /// <returns>落下したY座標（落下不可の場合は-1）</returns>
        public int Drop(BoardState board, int x, int z, int player)
        {
            int boardSize = board.GetSize();

            for (int y = 0; y < boardSize; y++)
            {
                if (board.Get(x, y, z) == 0)
                {
                    board.Set(x, y, z, player);
                    return y;
                }
            }

            return -1;
        }
    }
}