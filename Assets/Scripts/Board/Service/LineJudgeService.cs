// ======================================================
// LineJudgeService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-03
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

        /// <summary>ライン成立条件の最低連続マス数</summary>
        private readonly int _connectCount;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>ライン成立イベント Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete =
            new Subject<LineCompleteEvent>();

        /// <summary>ライン成立イベント購読用</summary>
        public IObservable<LineCompleteEvent> OnLineComplete =>
            _onLineComplete;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LineJudgeService(in int boardSize, in int connectCount)
        {
            _boardSize = boardSize;
            _connectCount = connectCount;

            // ライン生成ユーティリティ初期化
            _lineGenerator = new LineGenerator(_boardSize, _connectCount);

            // 全ライン生成
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
            foreach (int[][] line in _lines)
            {
                // ライン内の連続成立セル座標を取得
                List<(int Player, IReadOnlyList<BoardIndex> Cells)> consecutiveLines =
                    CalculateLinePositions(board, line);

                // 取得した連続ラインごとにイベント発火
                foreach ((int Player, IReadOnlyList<BoardIndex> Cells) lineInfo in consecutiveLines)
                {
                    _onLineComplete.OnNext(
                        new LineCompleteEvent(
                            lineInfo.Player,
                            new IReadOnlyList<BoardIndex>[] { lineInfo.Cells }
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
        private List<(int Player, IReadOnlyList<BoardIndex> Cells)> CalculateLinePositions(
            in BoardState board,
            in int[][] line)
        {
            // 結果格納用リスト
            List<(int Player, IReadOnlyList<BoardIndex> Cells)> result =
                new List<(int, IReadOnlyList<BoardIndex>)>();

            // 直前セルのプレイヤー番号
            int lastValue = 0;

            // 連続セルリスト
            List<BoardIndex> consecutiveCells =
                new List<BoardIndex>();

            // ライン内セル走査
            foreach (int[] cell in line)
            {
                // BoardIndex に変換
                BoardIndex index = new BoardIndex(cell[0], cell[1], cell[2]);

                // 盤面値取得
                int value =
                    board.Get(index);

                // 空マスならリセット
                if (value == 0)
                {
                    consecutiveCells.Clear();
                    lastValue = 0;
                    continue;
                }

                // 同一プレイヤーまたは開始状態
                if (value == lastValue || lastValue == 0)
                {
                    consecutiveCells.Add(index);
                    lastValue = value;
                }
                else
                {
                    // プレイヤー切替時リセット
                    consecutiveCells.Clear();
                    consecutiveCells.Add(index);
                    lastValue = value;
                }

                // 成立判定
                if (consecutiveCells.Count == _connectCount)
                {
                    // コピーして結果追加
                    result.Add(
                        (value, new List<BoardIndex>(consecutiveCells))
                    );

                    // スライド
                    consecutiveCells.RemoveAt(0);
                }
            }

            return result;
        }
    }
}