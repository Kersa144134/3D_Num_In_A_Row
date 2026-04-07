// ======================================================
// LineGenerator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-17
// 更新日時 : 2026-04-03
// 概要     : ライン配列生成クラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;

namespace BoardSystem.Domain
{
    /// <summary>
    /// ライン生成クラス
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

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        public LineGenerator(in int boardSize, in int connectCount)
        {
            _boardSize = boardSize;
            _connectCount = connectCount;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 盤面内のライン配列を生成
        /// </summary>
        /// <returns>生成されたライン配列 (int[][][])</returns>
        public int[][][] GenerateLines()
        {
            List<int[][]> lineList = new List<int[][]>();

            // X方向ライン生成
            for (int y = 0; y < _boardSize; y++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLine(lineList, 0, y, z, _boardSize - 1, y, z);
                }
            }

            // Y方向ライン生成
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLine(lineList, x, 0, z, x, _boardSize - 1, z);
                }
            }

            // Z方向ライン生成
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    AddLine(lineList, x, y, 0, x, y, _boardSize - 1);
                }
            }

            return lineList.ToArray();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 2 端点からラインを生成してリストに追加
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
            int[][] line = new int[_boardSize][];

            for (int i = 0; i < _boardSize; i++)
            {
                float t = i / (float)(_boardSize - 1);
                int x = Mathf.RoundToInt(Mathf.Lerp(startX, endX, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(startY, endY, t));
                int z = Mathf.RoundToInt(Mathf.Lerp(startZ, endZ, t));

                line[i] = new int[] { x, y, z };
            }

            lineList.Add(line);
        }
    }
}