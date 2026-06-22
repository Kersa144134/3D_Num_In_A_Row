// ======================================================
// BoardRotate.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : 盤面回転処理を担当するクラス
//            軸と方向に対応
// ======================================================

using System.Collections.Generic;

namespace BoardSystem.Domain
{
    /// <summary>
    /// 盤面回転クラス
    /// </summary>
    public sealed class BoardRotate
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>空マスを表す値</summary>
        private const int EMPTY = 0;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 盤面を 90 度回転した結果を取得する
        /// </summary>
        /// <param name="board">参照対象の盤面</param>
        /// <param name="axis">回転軸</param>
        /// <param name="direction">回転方向</param>
        /// <param name="moves">回転による移動情報</param>
        /// <returns>回転後の盤面データ</returns>
        public int[,,] Rotate90(
            in IBoardReader board,
            in RotationAxis axis,
            in RotationDirection direction,
            out IReadOnlyList<BoardMoveResult> moves)
        {
            if (board == null)
            {
                moves = new List<BoardMoveResult>();

                return new int[0, 0, 0];
            }

            // 盤面サイズ取得
            int size = board.GetSize();

            // 回転後盤面生成
            int[,,] rotatedBoard = new int[size, size, size];

            // 移動情報生成
            List<BoardMoveResult> rotateMoves =
                new List<BoardMoveResult>(
                    size * size * size
                );

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        // 元座標生成
                        BoardIndex fromIndex = new BoardIndex(x, y, z);

                        // セル値取得
                        int value = board.Get(fromIndex);

                        // 空マスはスキップ
                        if (value == EMPTY)
                        {
                            continue;
                        }

                        // 回転後座標初期化
                        int newX = x;
                        int newY = y;
                        int newZ = z;

                        // X 軸回転
                        if (axis == RotationAxis.X)
                        {
                            if (direction == RotationDirection.Positive)
                            {
                                newY = z;
                                newZ = size - 1 - y;
                            }
                            else
                            {
                                newY = size - 1 - z;
                                newZ = y;
                            }
                        }
                        // Z 軸回転
                        else if (axis == RotationAxis.Z)
                        {
                            if (direction == RotationDirection.Positive)
                            {
                                newX = y;
                                newY = size - 1 - x;
                            }
                            else
                            {
                                newX = size - 1 - y;
                                newY = x;
                            }
                        }

                        // 移動先生成
                        BoardIndex toIndex = new BoardIndex(newX, newY, newZ);

                        // 回転後盤面へ反映
                        rotatedBoard[newX, newY, newZ] = value;

                        // 移動情報記録
                        rotateMoves.Add(new BoardMoveResult(fromIndex, toIndex));
                    }
                }
            }

            // 移動情報返却
            moves = rotateMoves;

            // 回転後盤面返却
            return rotatedBoard;
        }
    }
}