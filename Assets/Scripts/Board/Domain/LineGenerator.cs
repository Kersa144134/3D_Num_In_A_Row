// ======================================================
// LineGenerator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-17
// 更新日時 : 2026-04-07
// 概要     : ライン配列生成クラス
//            立方体対角は除外
// ======================================================

using System.Collections.Generic;
using UnityEngine;

namespace BoardSystem.Domain
{
    /// <summary>
    /// 盤面ライン生成クラス
    /// 3 次元盤面上での軸方向および斜め方向ラインを生成する
    /// </summary>
    public sealed class LineGenerator
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        /// <summary>ライン成立条件の最低連続マス数</summary>
        private readonly int _connectCount;

        /// <summary>生成ラインの一時プール</summary>
        private readonly List<int[][]> _linePool;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// LineGenerator のコンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        /// <param name="connectCount">ライン成立条件の最低連続マス数</param>
        public LineGenerator(in int boardSize, in int connectCount)
        {
            _boardSize = boardSize;
            _connectCount = connectCount;

            // ラインプールを初期化
            // 軸方向 + 斜め方向の最大数を目安に容量確保
            _linePool = new List<int[][]>(_boardSize * _boardSize * 6);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 盤面内の全ライン配列を生成する
        /// </summary>
        /// <returns>生成されたラインの 3 次元配列</returns>
        public int[][][] GenerateLines()
        {
            // 前回生成ラインをクリア
            _linePool.Clear();

            // 軸方向ライン生成
            GenerateAxisLines();

            // 斜め方向ライン生成
            GenerateDiagonalLines();

            // 生成したライン配列を返却
            return _linePool.ToArray();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// X, Y, Z 軸方向のラインを生成する
        /// </summary>
        private void GenerateAxisLines()
        {
            // X 軸方向ライン
            for (int y = 0; y < _boardSize; y++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLineAxis(0, y, z, _boardSize - 1, y, z);
                }
            }

            // Y 軸方向ライン
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLineAxis(x, 0, z, x, _boardSize - 1, z);
                }
            }

            // Z 軸方向ライン
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    AddLineAxis(x, y, 0, x, y, _boardSize - 1);
                }
            }
        }

        /// <summary>
        /// XY,XZ,YZ 各面の斜めラインを生成する
        /// </summary>
        private void GenerateDiagonalLines()
        {
            // XY 面
            for (int z = 0; z < _boardSize; z++)
            {
                for (int startY = 0; startY < _boardSize; startY++)
                {
                    AddLineDiagonalXY(0, startY, z, 1, 1);
                }

                for (int startX = 1; startX < _boardSize; startX++)
                {
                    AddLineDiagonalXY(startX, 0, z, 1, 1);
                }

                for (int startY = 0; startY < _boardSize; startY++)
                {
                    AddLineDiagonalXY(0, startY, z, 1, -1);
                }

                for (int startX = 1; startX < _boardSize; startX++)
                {
                    AddLineDiagonalXY(startX, _boardSize - 1, z, 1, -1);
                }
            }

            // XZ 面
            for (int y = 0; y < _boardSize; y++)
            {
                for (int startZ = 0; startZ < _boardSize; startZ++)
                {
                    AddLineDiagonalXZ(0, y, startZ, 1, 1);
                }

                for (int startX = 1; startX < _boardSize; startX++)
                {
                    AddLineDiagonalXZ(startX, y, 0, 1, 1);
                }

                for (int startZ = 0; startZ < _boardSize; startZ++)
                {
                    AddLineDiagonalXZ(0, y, startZ, 1, -1);
                }

                for (int startX = 1; startX < _boardSize; startX++)
                {
                    AddLineDiagonalXZ(startX, y, _boardSize - 1, 1, -1);
                }
            }

            // YZ 面
            for (int x = 0; x < _boardSize; x++)
            {
                for (int startZ = 0; startZ < _boardSize; startZ++)
                {
                    AddLineDiagonalYZ(x, 0, startZ, 1, 1);
                }

                for (int startY = 1; startY < _boardSize; startY++)
                {
                    AddLineDiagonalYZ(x, startY, 0, 1, 1);
                }

                for (int startZ = 0; startZ < _boardSize; startZ++)
                {
                    AddLineDiagonalYZ(x, _boardSize - 1, startZ, -1, 1);
                }

                for (int startY = 1; startY < _boardSize; startY++)
                {
                    AddLineDiagonalYZ(x, startY, 0, -1, 1);
                }
            }
        }

        /// <summary>
        /// 軸方向ラインを生成してプールに追加する
        /// </summary>
        /// <param name="startX">開始 X 座標</param>
        /// <param name="startY">開始 Y 座標</param>
        /// <param name="startZ">開始 Z 座標</param>
        /// <param name="endX">終了 X 座標</param>
        /// <param name="endY">終了 Y 座標</param>
        /// <param name="endZ">終了 Z 座標</param>
        private void AddLineAxis(
            in int startX,
            in int startY,
            in int startZ,
            in int endX,
            in int endY,
            in int endZ)
        {
            int deltaX = (endX - startX) / (_boardSize - 1);
            int deltaY = (endY - startY) / (_boardSize - 1);
            int deltaZ = (endZ - startZ) / (_boardSize - 1);

            int[][] line = new int[_boardSize][];
            for (int i = 0; i < _boardSize; i++)
            {
                line[i] = new int[3];
                line[i][0] = startX + i * deltaX;
                line[i][1] = startY + i * deltaY;
                line[i][2] = startZ + i * deltaZ;
            }

            _linePool.Add(line);
        }

        /// <summary>
        /// XY 面の斜め方向ラインを生成してプールに追加する
        /// </summary>
        /// <param name="startX">開始 X 座標</param>
        /// <param name="startY">開始 Y 座標</param>
        /// <param name="z">固定 Z 座標</param>
        /// <param name="dx">X 方向の増分（±1）</param>
        /// <param name="dy">Y 方向の増分（±1）</param>
        private void AddLineDiagonalXY(
            in int startX,
            in int startY,
            in int z,
            in int dx,
            in int dy)
        {
            int n = _boardSize;

            int length = 0;

            if (dx > 0 && dy > 0)
            {
                length = Mathf.Min(n - startX, n - startY);
            }
            else if (dx > 0 && dy < 0)
            {
                length = Mathf.Min(n - startX, startY + 1);
            }
            else if (dx < 0 && dy > 0)
            {
                length = Mathf.Min(startX + 1, n - startY);
            }
            else
            {
                length = Mathf.Min(startX + 1, startY + 1);
            }

            int[][] line = new int[length][];

            for (int i = 0; i < length; i++)
            {
                line[i] = new int[3];
                line[i][0] = startX + i * dx;
                line[i][1] = startY + i * dy;
                line[i][2] = z;
            }

            _linePool.Add(line);
        }

        /// <summary>
        /// XZ 面の斜め方向ラインを生成してプールに追加する
        /// </summary>
        /// <param name="startX">開始 X 座標</param>
        /// <param name="y">固定 Y 座標</param>
        /// <param name="startZ">開始 Z 座標</param>
        /// <param name="dx">X 方向の増分（±1）</param>
        /// <param name="dz">Z 方向の増分（±1）</param>
        private void AddLineDiagonalXZ(
            in int startX,
            in int y,
            in int startZ,
            in int dx,
            in int dz)
        {
            int n = _boardSize;

            int length = 0;

            if (dx > 0 && dz > 0)
            {
                length = Mathf.Min(n - startX, n - startZ);
            }
            else if (dx > 0 && dz < 0)
            {
                length = Mathf.Min(n - startX, startZ + 1);
            }
            else if (dx < 0 && dz > 0)
            {
                length = Mathf.Min(startX + 1, n - startZ);
            }
            else
            {
                length = Mathf.Min(startX + 1, startZ + 1);
            }

            int[][] line = new int[length][];

            for (int i = 0; i < length; i++)
            {
                line[i] = new int[3];
                line[i][0] = startX + i * dx;
                line[i][1] = y;
                line[i][2] = startZ + i * dz;
            }

            _linePool.Add(line);
        }

        /// <summary>
        /// YZ 面の斜め方向ラインを生成してプールに追加する
        /// </summary>
        /// <param name="x">固定 X 座標</param>
        /// <param name="startY">開始 Y 座標</param>
        /// <param name="startZ">開始 Z 座標</param>
        /// <param name="dy">Y 方向の増分（±1）</param>
        /// <param name="dz">Z 方向の増分（±1）</param>
        private void AddLineDiagonalYZ(
            in int x,
            in int startY,
            in int startZ,
            in int dy,
            in int dz)
        {
            int n = _boardSize;

            int length = 0;

            if (dy > 0 && dz > 0)
            {
                length = Mathf.Min(n - startY, n - startZ);
            }
            else if (dy > 0 && dz < 0)
            {
                length = Mathf.Min(n - startY, startZ + 1);
            }
            else if (dy < 0 && dz > 0)
            {
                length = Mathf.Min(startY + 1, n - startZ);
            }
            else
            {
                length = Mathf.Min(startY + 1, startZ + 1);
            }

            int[][] line = new int[length][];

            for (int i = 0; i < length; i++)
            {
                line[i] = new int[3];
                line[i][0] = x;
                line[i][1] = startY + i * dy;
                line[i][2] = startZ + i * dz;
            }

            _linePool.Add(line);
        }
    }
}