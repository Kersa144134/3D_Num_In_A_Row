// ======================================================
// ColumnPlacementService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-02
// 概要     : 列への駒配置ロジックサービス
// ======================================================

using BoardSystem.Data;

namespace BoardSystem.Service
{
    /// <summary>
    /// 列への駒配置処理サービス
    /// </summary>
    public sealed class ColumnPlacementService
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定列に駒を配置可能か判定
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <returns>配置可能なら true</returns>
        public bool CanPlace(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            int boardSize = board.GetSize();

            // 上から順に空マスをチェック
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
        /// 指定列に駒を配置する
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <param name="player">プレイヤー番号</param>
        /// <returns>配置された Y 座標（配置不可の場合は -1）</returns>
        public int Place(
            in BoardState board,
            in int columnX,
            in int columnZ,
            in int player)
        {
            int boardSize = board.GetSize();

            // 下から空マスを探索して配置
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