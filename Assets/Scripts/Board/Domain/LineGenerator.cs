// ======================================================
// LineGenerator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-17
// 更新日時 : 2026-04-07
// 概要     : ライン配列生成クラス
//            - 立方体対角は除外
//            - 面内斜めは45度のみ
//            - 6面すべてに対応
// ======================================================

using System.Collections.Generic;
using UnityEngine;

namespace BoardSystem.Domain
{
    /// <summary>
    /// 盤面ライン生成クラス
    /// 3次元盤面上での軸方向および面内45度斜めラインを生成する
    /// </summary>
    public sealed class LineGenerator
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面サイズ（X,Y,Z 共通）</summary>
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

            // ラインプールを初期化、軸方向 + 面内斜めの最大数を目安に容量確保
            _linePool = new List<int[][]>(_boardSize * _boardSize * 6);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 盤面内の全ライン配列を生成する
        /// </summary>
        /// <returns>生成されたラインの3次元配列</returns>
        public int[][][] GenerateLines()
        {
            // 前回生成ラインをクリア
            _linePool.Clear();

            // 軸方向ライン生成
            GenerateAxisLines();

            // 面内45°斜めライン生成
            GenerateDiagonal45Lines();

            // 生成したライン配列を返却
            return _linePool.ToArray();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// X,Y,Z 軸方向のラインを生成する
        /// </summary>
        private void GenerateAxisLines()
        {
            // X軸方向ライン
            for (int y = 0; y < _boardSize; y++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLineAxis(0, y, z, _boardSize - 1, y, z);
                }
            }

            // Y軸方向ライン
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLineAxis(x, 0, z, x, _boardSize - 1, z);
                }
            }

            // Z軸方向ライン
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    AddLineAxis(x, y, 0, x, y, _boardSize - 1);
                }
            }
        }

        /// <summary>
        /// XY,XZ,YZ 各面の45°斜めラインを生成する
        /// </summary>
        private void GenerateDiagonal45Lines()
        {
            int n = _boardSize;

            // XY面
            for (int z = 0; z < n; z++)
            {
                for (int startY = 0; startY < n; startY++)
                {
                    AddDiagonalXYFixed(0, startY, z, 1, 1);
                }

                for (int startX = 1; startX < n; startX++)
                {
                    AddDiagonalXYFixed(startX, 0, z, 1, 1);
                }

                for (int startY = 0; startY < n; startY++)
                {
                    AddDiagonalXYFixed(0, startY, z, 1, -1);
                }

                for (int startX = 1; startX < n; startX++)
                {
                    AddDiagonalXYFixed(startX, n - 1, z, 1, -1);
                }
            }

            // XZ面
            for (int y = 0; y < n; y++)
            {
                for (int startZ = 0; startZ < n; startZ++)
                {
                    AddDiagonalXZFixed(0, y, startZ, 1, 1);
                }

                for (int startX = 1; startX < n; startX++)
                {
                    AddDiagonalXZFixed(startX, y, 0, 1, 1);
                }

                for (int startZ = 0; startZ < n; startZ++)
                {
                    AddDiagonalXZFixed(0, y, startZ, 1, -1);
                }

                for (int startX = 1; startX < n; startX++)
                {
                    AddDiagonalXZFixed(startX, y, n - 1, 1, -1);
                }
            }

            // YZ面
            for (int x = 0; x < n; x++)
            {
                for (int startZ = 0; startZ < n; startZ++)
                {
                    AddDiagonalYZFixed(x, 0, startZ, 1, 1);
                }

                for (int startY = 1; startY < n; startY++)
                {
                    AddDiagonalYZFixed(x, startY, 0, 1, 1);
                }

                for (int startZ = 0; startZ < n; startZ++)
                {
                    AddDiagonalYZFixed(x, n - 1, startZ, -1, 1);
                }

                for (int startY = 1; startY < n; startY++)
                {
                    AddDiagonalYZFixed(x, startY, 0, -1, 1);
                }
            }
        }

        /// <summary>
        /// 軸方向ラインを生成してプールに追加する
        /// </summary>
        /// <param name="startX">開始X座標</param>
        /// <param name="startY">開始Y座標</param>
        /// <param name="startZ">開始Z座標</param>
        /// <param name="endX">終了X座標</param>
        /// <param name="endY">終了Y座標</param>
        /// <param name="endZ">終了Z座標</param>
        private void AddLineAxis(int startX, int startY, int startZ, int endX, int endY, int endZ)
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
        /// XY面の45°斜めラインを生成してプールに追加する
        /// </summary>
        /// <param name="startX">開始X座標</param>
        /// <param name="startY">開始Y座標</param>
        /// <param name="z">固定Z座標</param>
        /// <param name="dx">X方向の増分（±1）</param>
        /// <param name="dy">Y方向の増分（±1）</param>
        private void AddDiagonalXYFixed(int startX, int startY, int z, int dx, int dy)
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
        /// XZ面の45°斜めラインを生成してプールに追加する
        /// </summary>
        /// <param name="startX">開始X座標</param>
        /// <param name="y">固定Y座標</param>
        /// <param name="startZ">開始Z座標</param>
        /// <param name="dx">X方向の増分（±1）</param>
        /// <param name="dz">Z方向の増分（±1）</param>
        private void AddDiagonalXZFixed(int startX, int y, int startZ, int dx, int dz)
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
        /// YZ面の45°斜めラインを生成してプールに追加する
        /// </summary>
        /// <param name="x">固定X座標</param>
        /// <param name="startY">開始Y座標</param>
        /// <param name="startZ">開始Z座標</param>
        /// <param name="dy">Y方向の増分（±1）</param>
        /// <param name="dz">Z方向の増分（±1）</param>
        private void AddDiagonalYZFixed(int x, int startY, int startZ, int dy, int dz)
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