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
        /// 盤面を 90 度回転させ、移動情報を返却する
        /// </summary>
        /// <param name="state">対象盤面状態</param>
        /// <param name="axis">回転軸</param>
        /// <param name="direction">回転方向（+/-）</param>
        /// <returns>移動情報リスト（from → to）</returns>
        public IReadOnlyList<(BoardIndex from, BoardIndex to)> Rotate90(
            BoardState state,
            in RotationAxis axis,
            in RotationDirection direction)
        {
            if (state == null)
            {
                return new List<(BoardIndex, BoardIndex)>();
            }

            int size = state.GetSize();
            int[,,] newBoard = new int[size, size, size];
            List<(BoardIndex from, BoardIndex to)> moves =
                new List<(BoardIndex, BoardIndex)>(size * size * size);

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        BoardIndex fromIndex = new BoardIndex(x, y, z);
                        int value = state.Get(fromIndex);

                        if (value == EMPTY) continue;

                        int newX = x;
                        int newY = y;
                        int newZ = z;

                        // X軸回転
                        if (axis == RotationAxis.X)
                        {
                            if (direction == RotationDirection.Positive)
                            {
                                newX = x;
                                newY = z;
                                newZ = size - 1 - y;
                            }
                            else
                            {
                                newX = x;
                                newY = size - 1 - z;
                                newZ = y;
                            }
                        }
                        // Z軸回転
                        else if (axis == RotationAxis.Z)
                        {
                            if (direction == RotationDirection.Positive)
                            {
                                newX = y;
                                newY = size - 1 - x;
                                newZ = z;
                            }
                            else
                            {
                                newX = size - 1 - y;
                                newY = x;
                                newZ = z;
                            }
                        }

                        BoardIndex toIndex = new BoardIndex(newX, newY, newZ);
                        newBoard[newX, newY, newZ] = value;
                        moves.Add((fromIndex, toIndex));
                    }
                }
            }

            ApplyRotatedBoard(state, newBoard, size);
            return moves;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 回転後の盤面データを反映する
        /// </summary>
        private void ApplyRotatedBoard(
            BoardState state,
            int[,,] newBoard,
            int size)
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        state.Set(new BoardIndex(x, y, z), newBoard[x, y, z]);
                    }
                }
            }
        }
    }
}