// ======================================================
// LineJudgeService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-02
// 概要     : ライン判定サービス
//            任意サイズ盤面や連続マス指定に対応
// ======================================================

using System;
using System.Collections.Generic;
using UniRx;
using BoardSystem.Data;
using BoardSystem.Utility;

namespace BoardSystem.Service
{
    /// <summary>
    /// ライン判定サービス
    /// </summary>
    public sealed class LineJudgeService : IDisposable
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>ライン生成ユーティリティ</summary>
        private readonly LineGenerator _lineGenerator;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        /// <summary>ライン配列（各ラインは座標配列）</summary>
        private readonly int[][][] _lines;

        /// <summary>ライン成立条件の連続マス数</summary>
        private readonly int _connectCount;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>ライン成立イベント Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete = new Subject<LineCompleteEvent>();

        /// <summary>ライン成立イベント購読用</summary>
        public IObservable<LineCompleteEvent> OnLineComplete => _onLineComplete;

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

            // --------------------------------------------------
            // ライン生成ユーティリティ初期化
            // --------------------------------------------------
            _lineGenerator = new LineGenerator(_boardSize);

            // --------------------------------------------------
            // 全ライン生成
            // --------------------------------------------------
            _lines = _lineGenerator.GenerateLines();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 盤面全体のライン判定を行い、成立時にイベントを発火する
        /// </summary>
        public void CheckAll(in BoardState board)
        {
            // --------------------------------------------------
            // 全ラインを走査
            // --------------------------------------------------
            foreach (int[][] line in _lines)
            {
                // ライン内の連続成立セル座標を取得
                List<(int Player, IReadOnlyList<(int x, int y, int z)> Cells)> consecutiveLines =
                    CalculateLinePositions(board, line);

                // --------------------------------------------------
                // 取得した連続ラインごとにイベント発火
                // --------------------------------------------------
                foreach (var lineInfo in consecutiveLines)
                {
                    _onLineComplete.OnNext(
                        new LineCompleteEvent(
                            lineInfo.Player,
                            new IReadOnlyList<(int x, int y, int z)>[] { lineInfo.Cells }
                        )
                    );
                }
            }
        }

        /// <summary>
        /// サブジェクト終了処理
        /// </summary>
        public void Dispose()
        {
            _onLineComplete.OnCompleted();
            _onLineComplete.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定ライン内の連続セル座標を取得
        /// </summary>
        /// <param name="board">盤面状態</param>
        /// <param name="line">判定対象ライン座標配列</param>
        /// <returns>成立した連続ラインのプレイヤー番号と座標リスト</returns>
        private List<(int Player, IReadOnlyList<(int x, int y, int z)> Cells)> CalculateLinePositions(
            in BoardState board,
            in int[][] line)
        {
            // --------------------------------------------------
            // 結果格納用リスト
            // --------------------------------------------------
            List<(int Player, IReadOnlyList<(int x, int y, int z)> Cells)> result =
                new List<(int, IReadOnlyList<(int x, int y, int z)>)>();

            // --------------------------------------------------
            // 判定用変数
            // lastValue : 直前セルのプレイヤー番号
            // consecutiveCells : 連続しているセルの座標リスト
            // --------------------------------------------------
            int lastValue = 0;
            List<(int x, int y, int z)> consecutiveCells = new List<(int x, int y, int z)>();

            // --------------------------------------------------
            // ライン内セル走査
            // --------------------------------------------------
            foreach (int[] cell in line)
            {
                int x = cell[0];
                int y = cell[1];
                int z = cell[2];

                // --------------------------------------------------
                // 盤面値取得
                // --------------------------------------------------
                int value = board.Get(x, y, z);

                // --------------------------------------------------
                // 空マスなら連続リセット
                // --------------------------------------------------
                if (value == 0)
                {
                    consecutiveCells.Clear();
                    lastValue = 0;
                    continue;
                }

                // --------------------------------------------------
                // 前セルと同じプレイヤーか判定
                // --------------------------------------------------
                if (value == lastValue || lastValue == 0)
                {
                    consecutiveCells.Add((x, y, z));
                    lastValue = value;
                }
                else
                {
                    // プレイヤー切替時は連続リセット
                    consecutiveCells.Clear();
                    consecutiveCells.Add((x, y, z));
                    lastValue = value;
                }

                // --------------------------------------------------
                // 連続マスが成立条件に達したら結果に追加
                // --------------------------------------------------
                if (consecutiveCells.Count == _connectCount)
                {
                    // 結果にコピーして追加
                    result.Add((value, new List<(int x, int y, int z)>(consecutiveCells)));

                    // スライドさせて次の判定に備える
                    consecutiveCells.RemoveAt(0);
                }
            }

            return result;
        }
    }
}