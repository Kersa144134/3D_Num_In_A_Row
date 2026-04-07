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

        /// <summary>再利用可能なラインバッファプール</summary>
        private readonly Dictionary<int, Stack<int[][]>> _lineBufferPool = new Dictionary<int, Stack<int[][]>>();

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
            // --------------------------------------------------
            // X
            // --------------------------------------------------
            for (int y = 0; y < _boardSize; y++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLineAxis(0, y, z, _boardSize - 1, y, z);
                }
            }

            // --------------------------------------------------
            // Y
            // --------------------------------------------------
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLineAxis(x, 0, z, x, _boardSize - 1, z);
                }
            }

            // --------------------------------------------------
            // Z
            // --------------------------------------------------
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    AddLineAxis(x, y, 0, x, y, _boardSize - 1);
                }
            }
        }

        /// <summary>
        /// XY, XZ, YZ 各面の斜めラインを生成する
        /// </summary>
        private void GenerateDiagonalLines()
        {
            // --------------------------------------------------
            // XY
            // --------------------------------------------------
            for (int z = 0; z < _boardSize; z++)
            {
                for (int startY = 0; startY < _boardSize; startY++)
                {
                    AddLineDiagonal(z, 0, startY, 1, 1, 0);
                }

                for (int startX = 1; startX < _boardSize; startX++)
                {
                    AddLineDiagonal(z, startX, 0, 1, 1, 0);
                }

                for (int startY = 0; startY < _boardSize; startY++)
                {
                    AddLineDiagonal(z, 0, startY, 1, -1, 0);
                }

                for (int startX = 1; startX < _boardSize; startX++)
                {
                    AddLineDiagonal(z, startX, _boardSize - 1, 1, -1, 0);
                }
            }

            // --------------------------------------------------
            // XZ
            // --------------------------------------------------
            for (int y = 0; y < _boardSize; y++)
            {
                for (int startZ = 0; startZ < _boardSize; startZ++)
                {
                    AddLineDiagonal(y, 0, startZ, 1, 1, 1);
                }

                for (int startX = 1; startX < _boardSize; startX++)
                {
                    AddLineDiagonal(y, startX, 0, 1, 1, 1);
                }

                for (int startZ = 0; startZ < _boardSize; startZ++)
                {
                    AddLineDiagonal(y, 0, startZ, 1, -1, 1);
                }

                for (int startX = 1; startX < _boardSize; startX++)
                {
                    AddLineDiagonal(y, startX, _boardSize - 1, 1, -1, 1);
                }
            }

            // --------------------------------------------------
            // YZ
            // --------------------------------------------------
            for (int x = 0; x < _boardSize; x++)
            {
                for (int startZ = 0; startZ < _boardSize; startZ++)
                {
                    AddLineDiagonal(x, 0, startZ, 1, 1, 2);
                }

                for (int startY = 1; startY < _boardSize; startY++)
                {
                    AddLineDiagonal(x, startY, 0, 1, 1, 2);
                }

                for (int startZ = 0; startZ < _boardSize; startZ++)
                {
                    AddLineDiagonal(x, _boardSize - 1, startZ, -1, 1, 2);
                }

                for (int startY = 1; startY < _boardSize; startY++)
                {
                    AddLineDiagonal(x, startY, 0, -1, 1, 2);
                }
            }
        }

        /// <summary>
        /// X, Y, Z 軸方向のラインを生成してラインプールに追加する
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

            // プールからバッファ取得
            int[][] line = GetLineBuffer(_boardSize);

            // 各マスの座標を生成
            for (int i = 0; i < _boardSize; i++)
            {
                line[i][0] = startX + i * deltaX;
                line[i][1] = startY + i * deltaY;
                line[i][2] = startZ + i * deltaZ;
            }

            _linePool.Add(line);
        }

        /// <summary>
        /// XY, XZ, YZ 面の斜めラインを共通生成
        /// </summary>
        /// <param name="fixedCoord">固定軸の座標値</param>
        /// <param name="startA">可変軸Aの開始座標</param>
        /// <param name="startB">可変軸Bの開始座標</param>
        /// <param name="deltaA">可変軸A方向の増分</param>
        /// <param name="deltaB">可変軸B方向の増分</param>
        /// <param name="plane">面指定（0=XY,1=XZ,2=YZ）</param>
        private void AddLineDiagonal(
            in int fixedCoord,
            in int startA,
            in int startB,
            in int deltaA,
            in int deltaB,
            in int plane)
        {
            int length = 0;

            // 増分方向に応じたライン長計算
            if (deltaA > 0 && deltaB > 0)
            {
                length = Mathf.Min(_boardSize - startA, _boardSize - startB);
            }
            else if (deltaA > 0 && deltaB < 0)
            {
                length = Mathf.Min(_boardSize - startA, startB + 1);
            }
            else if (deltaA < 0 && deltaB > 0)
            {
                length = Mathf.Min(startA + 1, _boardSize - startB);
            }
            else
            {
                length = Mathf.Min(startA + 1, startB + 1);
            }

            // プールからバッファ取得
            int[][] line = GetLineBuffer(length);

            // 座標生成
            for (int i = 0; i < length; i++)
            {
                switch (plane)
                {
                    // XY
                    case 0:
                        line[i][0] = startA + i * deltaA;
                        line[i][1] = startB + i * deltaB;
                        line[i][2] = fixedCoord;
                        break;

                    // XZ
                    case 1:
                        line[i][0] = startA + i * deltaA;
                        line[i][1] = fixedCoord;
                        line[i][2] = startB + i * deltaB;
                        break;

                    // YZ
                    case 2:
                        line[i][0] = fixedCoord;
                        line[i][1] = startA + i * deltaA;
                        line[i][2] = startB + i * deltaB;
                        break;
                }
            }

            // プールに追加
            _linePool.Add(line);
        }

        /// <summary>
        /// ラインバッファをプールから取得
        /// </summary>
        /// <param name="length">取得するラインの長さ</param>
        private int[][] GetLineBuffer(in int length)
        {
            if (!_lineBufferPool.TryGetValue(length, out Stack<int[][]> stack))
            {
                stack = new Stack<int[][]>();
                _lineBufferPool[length] = stack;
            }

            if (stack.Count > 0)
            {
                // 既存バッファを再利用
                return stack.Pop();
            }

            // 新規生成
            int[][] line = new int[length][];
            for (int i = 0; i < length; i++)
            {
                line[i] = new int[3];
            }

            return line;
        }

        /// <summary>
        /// ラインバッファをプールに返却
        /// </summary>
        private void ReleaseLineBuffer(int[][] line)
        {
            int length = line.Length;

            if (!_lineBufferPool.TryGetValue(length, out Stack<int[][]> stack))
            {
                stack = new Stack<int[][]>();
                _lineBufferPool[length] = stack;
            }

            stack.Push(line);
        }
    }
}