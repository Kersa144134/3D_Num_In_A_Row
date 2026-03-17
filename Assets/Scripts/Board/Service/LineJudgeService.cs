// ======================================================
// LineJudgeService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-03-17
// 概要     : ライン判定処理
//            任意サイズ盤面や連続マス指定に対応
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using BoardSystem.Data;

namespace BoardSystem.Service
{
    /// <summary>
    /// ライン判定サービス
    /// </summary>
    public sealed class LineJudgeService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        /// <summary>ライン配列</summary>
        private readonly int[][][] _lines;

        /// <summary>ライン成立条件の連続マス数</summary>
        private readonly int _connectCount;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        /// <param name="connectCount">ライン成立条件の連続マス数</param>
        public LineJudgeService(in int boardSize, in int connectCount)
        {
            _boardSize = boardSize;
            _connectCount = connectCount;

            // ライン生成
            _lines = GenerateLines();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定プレイヤーのライン成立を判定
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="player">プレイヤー番号</param>
        /// <returns>ライン成立している場合はtrue</returns>
        public bool Check(in BoardState board, in int player)
        {
            // 全ラインを走査
            foreach (int[][] line in _lines)
            {
                // 現在の連続数を初期化
                int consecutive = 0;

                // ライン内の各セルを走査
                foreach (int[] cell in line)
                {
                    // 座標を取得
                    int x = cell[0];
                    int y = cell[1];
                    int z = cell[2];

                    // 指定プレイヤーの駒か判定
                    if (board.Get(x, y, z) == player)
                    {
                        // 連続数を加算
                        consecutive++;

                        // 勝利条件を満たしたら即終了
                        if (consecutive >= _connectCount)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        // 不一致なら連続数リセット
                        consecutive = 0;
                    }
                }
            }

            return false;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ライン生成
        /// </summary>
        /// <returns>生成したライン配列</returns>
        private int[][][] GenerateLines()
        {
            // ライン配列構築用リスト
            List<int[][]> lineList = new List<int[][]>();

            // X 方向ライン生成
            for (int y = 0; y < _boardSize; y++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLine(lineList, 0, y, z, _boardSize - 1, y, z);
                }
            }

            // Y 方向ライン生成
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLine(lineList, x, 0, z, x, _boardSize - 1, z);
                }
            }

            // Z 方向ライン生成
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    AddLine(lineList, x, y, 0, x, y, _boardSize - 1);
                }
            }

            // 対角線生成
            AddDiagonalLines(lineList);

            // 配列へ変換
            return lineList.ToArray();
        }

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
                // 補間係数（0 ～ 1）
                float t = i / (float)(_boardSize - 1);

                // 各座標を補間
                int x = Mathf.RoundToInt(Mathf.Lerp(startX, endX, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(startY, endY, t));
                int z = Mathf.RoundToInt(Mathf.Lerp(startZ, endZ, t));

                // 内部ラインを除外
                if ((_boardSize > 3) &&
                    (x != 0 && x != _boardSize - 1) &&
                    (z != 0 && z != _boardSize - 1))
                {
                    return;
                }

                // 座標を格納
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