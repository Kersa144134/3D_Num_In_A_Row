// ======================================================
// LineGenerator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-17
// 更新日時 : 2026-04-07
// 概要     : ライン配列生成クラス
//            立方体対角は除外
//            面内斜めは45度のみ
// ======================================================

using System.Collections.Generic;
using UnityEngine;

namespace BoardSystem.Domain
{
    public sealed class LineGenerator
    {
        private readonly int _boardSize;
        private readonly int _connectCount;
        private readonly List<int[][]> _linePool;

        public LineGenerator(in int boardSize, in int connectCount)
        {
            _boardSize = boardSize;
            _connectCount = connectCount;
            _linePool = new List<int[][]>(_boardSize * _boardSize * 6);
        }

        /// <summary>
        /// 盤面内のライン配列を生成
        /// </summary>
        public int[][][] GenerateLines()
        {
            _linePool.Clear();
            GenerateAxisLines();      // 縦横軸ライン
            GenerateDiagonal45Lines(); // 面内45度斜めライン
            return _linePool.ToArray();
        }

        /// <summary>
        /// X,Y,Z 軸方向ライン生成（既存処理）
        /// </summary>
        private void GenerateAxisLines()
        {
            for (int y = 0; y < _boardSize; y++)
                for (int z = 0; z < _boardSize; z++)
                    AddLineAxis(0, y, z, _boardSize - 1, y, z);

            for (int x = 0; x < _boardSize; x++)
                for (int z = 0; z < _boardSize; z++)
                    AddLineAxis(x, 0, z, x, _boardSize - 1, z);

            for (int x = 0; x < _boardSize; x++)
                for (int y = 0; y < _boardSize; y++)
                    AddLineAxis(x, y, 0, x, y, _boardSize - 1);
        }

        /// <summary>
        /// XY, XZ, YZ 面内 45° 斜めライン生成
        /// </summary>
        private void GenerateDiagonal45Lines()
        {
            // XY 面
            for (int z = 0; z < _boardSize; z++)
            {
                // 左下→右上、左上→右下
                AddDiagonalXY(0, 0, z, 1, 1);                     // 左下→右上
                AddDiagonalXY(0, _boardSize - 1, z, 1, -1);      // 左上→右下
                AddDiagonalXY(_boardSize - 1, 0, z, -1, 1);      // 右下→左上
                AddDiagonalXY(_boardSize - 1, _boardSize - 1, z, -1, -1); // 右上→左下
            }

            // XZ 面
            for (int y = 0; y < _boardSize; y++)
            {
                AddDiagonalXZ(0, y, 0, 1, 1);
                AddDiagonalXZ(0, y, _boardSize - 1, 1, -1);
                AddDiagonalXZ(_boardSize - 1, y, 0, -1, 1);
                AddDiagonalXZ(_boardSize - 1, y, _boardSize - 1, -1, -1);
            }

            // YZ 面
            for (int x = 0; x < _boardSize; x++)
            {
                AddDiagonalYZ(x, 0, 0, 1, 1);
                AddDiagonalYZ(x, 0, _boardSize - 1, 1, -1);
                AddDiagonalYZ(x, _boardSize - 1, 0, -1, 1);
                AddDiagonalYZ(x, _boardSize - 1, _boardSize - 1, -1, -1);
            }
        }

        private void AddLineAxis(int startX, int startY, int startZ, int endX, int endY, int endZ)
        {
            int[][] line = new int[_boardSize][];
            for (int i = 0; i < _boardSize; i++)
                line[i] = new int[] { startX + i * (endX - startX) / (_boardSize - 1),
                                       startY + i * (endY - startY) / (_boardSize - 1),
                                       startZ + i * (endZ - startZ) / (_boardSize - 1) };
            _linePool.Add(line);
        }

        private void AddDiagonalXY(int startX, int startY, int z, int dx, int dy)
        {
            int length = _boardSize;
            int[][] line = new int[length][];
            for (int i = 0; i < length; i++)
                line[i] = new int[] {
                    Mathf.Clamp(startX + i * dx, 0, _boardSize - 1),
                    Mathf.Clamp(startY + i * dy, 0, _boardSize - 1),
                    z
                };
            _linePool.Add(line);
        }

        private void AddDiagonalXZ(int startX, int y, int startZ, int dx, int dz)
        {
            int length = _boardSize;
            int[][] line = new int[length][];
            for (int i = 0; i < length; i++)
                line[i] = new int[] {
                    Mathf.Clamp(startX + i * dx, 0, _boardSize - 1),
                    y,
                    Mathf.Clamp(startZ + i * dz, 0, _boardSize - 1)
                };
            _linePool.Add(line);
        }

        private void AddDiagonalYZ(int x, int startY, int startZ, int dy, int dz)
        {
            int length = _boardSize;
            int[][] line = new int[length][];
            for (int i = 0; i < length; i++)
                line[i] = new int[] {
                    x,
                    Mathf.Clamp(startY + i * dy, 0, _boardSize - 1),
                    Mathf.Clamp(startZ + i * dz, 0, _boardSize - 1)
                };
            _linePool.Add(line);
        }
    }
}