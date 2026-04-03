// ======================================================
// PiecePlacementService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-03
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
        // 定数
        // ======================================================

        /// <summary>空マスを示す値</summary>
        private const int EMPTY = 0;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 再配置時の移動情報バッファ
        /// </summary>
        private readonly List<(BoardIndex from, BoardIndex to)> _repositionBuffer =
            new List<(BoardIndex, BoardIndex)>(32);

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
            return FindEmptyY(board, columnX, columnZ) != -1;
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
            int y = FindEmptyY(
                board,
                columnX,
                columnZ
            );

            // 配置不可
            if (y == -1)
            {
                return -1;
            }

            // インデックス生成
            BoardIndex index = new BoardIndex(columnX, y, columnZ);

            // プレイヤー番号を書き込み
            board.Set(index, player);

            return y;
        }

        /// <summary>
        /// 指定列の駒を下に詰める
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <returns>駒単位の移動情報リスト（読み取り専用）</returns>
        public IReadOnlyList<(BoardIndex from, BoardIndex to)> Reposition(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            // 列専用の移動リスト
            List<(BoardIndex from, BoardIndex to)> moves = new List<(BoardIndex, BoardIndex)>();

            // 盤面サイズ取得
            int boardSize = board.GetSize();

            // 書き込みポインタ
            int writeY = 0;

            // 下から探索
            for (int readY = 0; readY < boardSize; readY++)
            {
                // 移動元インデックス
                BoardIndex fromIndex = new BoardIndex(columnX, readY, columnZ);
                int value = board.Get(fromIndex);

                if (value == EMPTY)
                {
                    continue;
                }

                // 正しい位置にある場合は書き込みポインタを進める
                if (readY == writeY)
                {
                    writeY++;
                    continue;
                }

                // 移動先インデックス生成
                BoardIndex toIndex = new BoardIndex(columnX, writeY, columnZ);

                // モデル上で値を移動
                board.Set(toIndex, value);
                board.Set(fromIndex, EMPTY);

                // 移動情報を記録
                moves.Add((fromIndex, toIndex));

                // 書き込みポインタを進める
                writeY++;
            }

            return moves;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定列の空マス Y を取得
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列X</param>
        /// <param name="columnZ">列Z</param>
        /// <returns>空マス Y　無ければ -1</returns>
        private int FindEmptyY(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            // 盤面サイズ取得
            int boardSize = board.GetSize();

            // 下から探索
            for (int y = 0; y < boardSize; y++)
            {
                // インデックス生成
                BoardIndex index = new BoardIndex(columnX, y, columnZ);

                // 空マスなら返却
                if (board.Get(index) == EMPTY)
                {
                    return y;
                }
            }

            // 空きなし
            return -1;
        }
    }
}