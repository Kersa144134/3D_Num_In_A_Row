// ======================================================
// LineJudgeService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-03-06
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

        /// <summary>
        /// 盤面サイズ
        /// </summary>
        private readonly int _boardSize;

        /// <summary>
        /// ライン成立条件の連続マス数
        /// </summary>
        private readonly int _connectCount;

        /// <summary>
        /// ラインリスト
        /// </summary>
        private readonly List<int[][]> _lineList;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        /// <param name="connectCount">ライン成立条件の連続マス数</param>
        public LineJudgeService(int boardSize, int connectCount)
        {
            _boardSize = boardSize;
            _connectCount = connectCount;
            _lineList = new List<int[][]>();

            GenerateLines();
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
        public bool Check(BoardState board, int player)
        {
            foreach (var line in _lineList)
            {
                int consecutive = 0;

                foreach (var cell in line)
                {
                    int x = cell[0];
                    int y = cell[1];
                    int z = cell[2];

                    if (board.Get(x, y, z) == player)
                    {
                        consecutive++;
                        if (consecutive >= _connectCount)
                            return true;
                    }
                    else
                    {
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
        /// 中央や内部列は除外
        /// </summary>
        private void GenerateLines()
        {
            // X方向
            for (int y = 0; y < _boardSize; y++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLine(0, y, z, _boardSize - 1, y, z);
                }
            }

            // Y方向
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    AddLine(x, 0, z, x, _boardSize - 1, z);
                }
            }

            // Z方向
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    AddLine(x, y, 0, x, y, _boardSize - 1);
                }
            }

            // 対角線
            AddDiagonalLines();
        }

        /// <summary>
        /// 2端点からライン作成
        /// 中央や内部列は除外
        /// </summary>
        /// <param name="x1">端点1のX</param>
        /// <param name="y1">端点1のY</param>
        /// <param name="z1">端点1のZ</param>
        /// <param name="x2">端点2のX</param>
        /// <param name="y2">端点2のY</param>
        /// <param name="z2">端点2のZ</param>
        private void AddLine(int x1, int y1, int z1, int x2, int y2, int z2)
        {
            int[][] line = new int[_boardSize][];

            for (int i = 0; i < _boardSize; i++)
            {
                int x = Mathf.RoundToInt(Mathf.Lerp(x1, x2, i / (float)(_boardSize - 1)));
                int y = Mathf.RoundToInt(Mathf.Lerp(y1, y2, i / (float)(_boardSize - 1)));
                int z = Mathf.RoundToInt(Mathf.Lerp(z1, z2, i / (float)(_boardSize - 1)));

                // 中央や内部列は除外
                if ((_boardSize > 3) && (x != 0 && x != _boardSize - 1) && (z != 0 && z != _boardSize - 1))
                    return;

                line[i] = new int[] { x, y, z };
            }

            _lineList.Add(line);
        }

        /// <summary>
        /// 対角線ライン追加
        /// </summary>
        private void AddDiagonalLines()
        {
            int max = _boardSize;

            // XY平面対角線（Z固定）
            for (int z = 0; z < max; z++)
            {
                AddLine(0, 0, z, max - 1, max - 1, z);
                AddLine(max - 1, 0, z, 0, max - 1, z);
            }

            // XZ平面対角線（Y固定）
            for (int y = 0; y < max; y++)
            {
                AddLine(0, y, 0, max - 1, y, max - 1);
                AddLine(max - 1, y, 0, 0, y, max - 1);
            }

            // YZ平面対角線（X固定）
            for (int x = 0; x < max; x++)
            {
                AddLine(x, 0, 0, x, max - 1, max - 1);
                AddLine(x, max - 1, 0, x, 0, max - 1);
            }

            // 3D対角線
            AddLine(0, 0, 0, max - 1, max - 1, max - 1);
            AddLine(max - 1, 0, 0, 0, max - 1, max - 1);
            AddLine(0, max - 1, 0, max - 1, 0, max - 1);
            AddLine(max - 1, max - 1, 0, 0, 0, max - 1);
        }
    }
}