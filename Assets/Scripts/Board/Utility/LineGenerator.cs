// ======================================================
// LineGenerator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-17
// 概要     : ライン配列生成ユーティリティ
// ======================================================

using System.Collections.Generic;
using UnityEngine;

namespace BoardSystem.Utility
{
    /// <summary>
    /// ライン生成ユーティリティ
    /// </summary>
    public sealed class LineGenerator
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        public LineGenerator(in int boardSize)
        {
            _boardSize = boardSize;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ライン配列を生成
        /// </summary>
        public int[][][] GenerateLines()
        {
            List<int[][]> lineList = new List<int[][]>();

            // X方向
            for (int y = 0; y < _boardSize; y++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLine(lineList, 0, y, z, _boardSize - 1, y, z);
                }
            }

            // Y方向
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLine(lineList, x, 0, z, x, _boardSize - 1, z);
                }
            }

            // Z方向
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    AddLine(lineList, x, y, 0, x, y, _boardSize - 1);
                }
            }

            AddDiagonalLines(lineList);

            return lineList.ToArray();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 2 端点からラインを生成して追加
        /// </summary>
        /// <param name="lineList">生成したラインを格納するリスト</param>
        /// <param name="startX">始点の X 座標</param>
        /// <param name="startY">始点の Y 座標</param>
        /// <param name="startZ">始点の Z 座標</param>
        /// <param name="endX">終点の X 座標</param>
        /// <param name="endY">終点の Y 座標</param>
        /// <param name="endZ">終点の Z 座標</param>
        private void AddLine(
            in List<int[][]> lineList,
            in int startX,
            in int startY,
            in int startZ,
            in int endX,
            in int endY,
            in int endZ)
        {
            // 1 ライン分の配列を確保
            int[][] line = new int[_boardSize][];

            // 線形補間で座標を生成
            for (int i = 0; i < _boardSize; i++)
            {
                float t = i / (float)(_boardSize - 1);

                // 各座標を補間
                int x = Mathf.RoundToInt(Mathf.Lerp(startX, endX, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(startY, endY, t));
                int z = Mathf.RoundToInt(Mathf.Lerp(startZ, endZ, t));

                // 内部ライン除外
                if ((_boardSize > 3) &&
                    (x != 0 && x != _boardSize - 1) &&
                    (z != 0 && z != _boardSize - 1))
                {
                    return;
                }

                line[i] = new int[] { x, y, z };
            }

            lineList.Add(line);
        }

        /// <summary>
        /// 対角線ライン生成
        /// </summary>
        /// <param name="lineList">生成したラインを格納するリスト</param>
        private void AddDiagonalLines(in List<int[][]> lineList)
        {
            int max = _boardSize;

            // XY 平面
            for (int z = 0; z < max; z++)
            {
                AddLine(lineList, 0, 0, z, max - 1, max - 1, z);
                AddLine(lineList, max - 1, 0, z, 0, max - 1, z);
            }

            // XZ 平面
            for (int y = 0; y < max; y++)
            {
                AddLine(lineList, 0, y, 0, max - 1, y, max - 1);
                AddLine(lineList, max - 1, y, 0, 0, y, max - 1);
            }

            // YZ 平面
            for (int x = 0; x < max; x++)
            {
                AddLine(lineList, x, 0, 0, x, max - 1, max - 1);
                AddLine(lineList, x, max - 1, 0, x, 0, max - 1);
            }

            // 3D 対角線
            AddLine(lineList, 0, 0, 0, max - 1, max - 1, max - 1);
            AddLine(lineList, max - 1, 0, 0, 0, max - 1, max - 1);
            AddLine(lineList, 0, max - 1, 0, max - 1, 0, max - 1);
            AddLine(lineList, max - 1, max - 1, 0, 0, 0, max - 1);
        }
    }
}