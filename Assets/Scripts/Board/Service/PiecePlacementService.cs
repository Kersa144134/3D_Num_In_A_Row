// ======================================================
// PiecePlacementService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-02
// 概要     : 列への駒配置ロジックサービス
// ======================================================

using BoardSystem.Data;
using System.Collections.Generic;

namespace BoardSystem.Service
{
    /// <summary>
    /// 列への駒配置処理サービス
    /// </summary>
    public sealed class PiecePlacementService
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
            // 盤面サイズ取得
            int boardSize = board.GetSize();

            // 上から順に空マスをチェック
            for (int y = boardSize - 1; y >= 0; y--)
            {
                // インデックス生成
                BoardIndex index = new BoardIndex(columnX, y, columnZ);

                // 空マスがあれば配置可能
                if (board.Get(index) == 0)
                {
                    return true;
                }
            }

            // 空きが無い場合は配置不可
            return false;
        }

        /// <summary>
        /// 指定列に駒を配置する
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <param name="player">プレイヤー番号</param>
        /// <returns>配置された列 Y インデックス</returns>
        public int Place(
            in BoardState board,
            in int columnX,
            in int columnZ,
            in int player)
        {
            // 盤面サイズ取得
            int boardSize = board.GetSize();

            // 下から空マスを探索して配置
            for (int y = 0; y < boardSize; y++)
            {
                // 指定座標のインデックス生成
                BoardIndex index = new BoardIndex(columnX, y, columnZ);

                // 空マスなら配置
                if (board.Get(index) == 0)
                {
                    // プレイヤー番号を書き込む
                    board.Set(index, player);

                    // 配置したインデックスを返す
                    return index.Y;
                }
            }

            // 配置不可
            return -1;
        }

        /// <summary>
        /// 指定列の駒を下に詰める
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <returns>移動情報リスト（移動が無い場合は空）</returns>
        public List<(BoardIndex from, BoardIndex to)> Reposition(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            // --------------------------------------------------
            // 盤面サイズ取得
            // --------------------------------------------------
            int boardSize = board.GetSize();

            // --------------------------------------------------
            // 移動情報を保持するリスト
            // --------------------------------------------------
            List<(BoardIndex from, BoardIndex to)> moves =
                new List<(BoardIndex, BoardIndex)>();

            // --------------------------------------------------
            // 書き込み先ポインタ
            // --------------------------------------------------
            int writeY = 0;

            // --------------------------------------------------
            // 下から順に走査
            // --------------------------------------------------
            for (int readY = 0; readY < boardSize; readY++)
            {
                // --------------------------------------------------
                // 読み取りインデックス生成
                // --------------------------------------------------
                BoardIndex fromIndex =
                    new BoardIndex(columnX, readY, columnZ);

                // --------------------------------------------------
                // 値取得
                // --------------------------------------------------
                int value = board.Get(fromIndex);

                // --------------------------------------------------
                // 空マスはスキップ
                // --------------------------------------------------
                if (value == 0)
                {
                    continue;
                }

                // --------------------------------------------------
                // 正しい位置ならスキップ
                // --------------------------------------------------
                if (readY == writeY)
                {
                    writeY++;
                    continue;
                }

                // --------------------------------------------------
                // 書き込み先インデックス生成
                // --------------------------------------------------
                BoardIndex toIndex =
                    new BoardIndex(columnX, writeY, columnZ);

                // --------------------------------------------------
                // 下に詰める
                // --------------------------------------------------
                board.Set(toIndex, value);

                // --------------------------------------------------
                // 元の位置をクリア
                // --------------------------------------------------
                board.Set(fromIndex, 0);

                // --------------------------------------------------
                // 移動情報を記録
                // --------------------------------------------------
                moves.Add((fromIndex, toIndex));

                // --------------------------------------------------
                // 次の書き込み位置へ
                // --------------------------------------------------
                writeY++;
            }

            // --------------------------------------------------
            // 移動情報を返却
            // --------------------------------------------------
            return moves;
        }
    }
}