// ======================================================
// LineJudge.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-03
// 概要     : ライン判定クラス
//            任意サイズ盤面や連続マス指定に対応
// ======================================================

using System;
using System.Collections.Generic;
using UniRx;

namespace BoardSystem.Domain
{
    /// <summary>
    /// ライン判定
    /// </summary>
    public sealed class LineJudge : IDisposable
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

        /// <summary>ライン配列</summary>
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
        public LineJudge(in int boardSize, in int connectCount)
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
        /// 1つでもラインが成立していれば true を返す
        /// </summary>
        public bool CheckAll(in BoardState board)
        {
            // ライン成立フラグ
            bool isAnyLineComplete = false;

            foreach (int[][] line in _lines)
            {
                // ライン内の連続成立セル座標を取得
                List<(IReadOnlyList<BoardIndex> Cells, int Player)> consecutiveLines =
                    CalculateLinePositions(board, line);

                // 成立ラインが存在する場合
                if (consecutiveLines.Count > 0)
                {
                    isAnyLineComplete = true;
                }

                // 取得した連続ラインごとにイベント発火
                foreach ((IReadOnlyList<BoardIndex> Cells, int Player) lineInfo in consecutiveLines)
                {
                    _onLineComplete.OnNext(
                        new LineCompleteEvent(
                            lineInfo.Player,
                            new IReadOnlyList<BoardIndex>[] { lineInfo.Cells }
                        )
                    );
                }
            }

            return isAnyLineComplete;
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
        private List<(IReadOnlyList<BoardIndex> Cells, int Player)> CalculateLinePositions(
            in BoardState board,
            in int[][] line)
        {
            // --------------------------------------------------
            // 結果格納用
            // --------------------------------------------------
            List<(IReadOnlyList<BoardIndex> Cells, int Player)> result =
                new List<(IReadOnlyList<BoardIndex>, int)>();

            // --------------------------------------------------
            // 連続管理用
            // --------------------------------------------------
            int lastValue = 0;
            List<BoardIndex> consecutiveCells = new List<BoardIndex>();

            // --------------------------------------------------
            // ライン走査
            // --------------------------------------------------
            foreach (int[] cell in line)
            {
                BoardIndex index = new BoardIndex(cell[0], cell[1], cell[2]);
                int value = board.Get(index);

                // --------------------------------------------------
                // 空マスで区切り
                // --------------------------------------------------
                if (value == 0)
                {
                    // 成立判定
                    if (consecutiveCells.Count >= _connectCount)
                    {
                        result.Add((new List<BoardIndex>(consecutiveCells), lastValue));
                    }

                    // リセット
                    consecutiveCells.Clear();
                    lastValue = 0;
                    continue;
                }

                // --------------------------------------------------
                // 同一プレイヤー継続
                // --------------------------------------------------
                if (value == lastValue || lastValue == 0)
                {
                    consecutiveCells.Add(index);
                    lastValue = value;
                }
                else
                {
                    // --------------------------------------------------
                    // プレイヤー切替時に確定チェック
                    // --------------------------------------------------
                    if (consecutiveCells.Count >= _connectCount)
                    {
                        result.Add((new List<BoardIndex>(consecutiveCells), lastValue));
                    }

                    // 新しい連続開始
                    consecutiveCells.Clear();
                    consecutiveCells.Add(index);
                    lastValue = value;
                }
            }

            // --------------------------------------------------
            // ループ終了後の取りこぼしチェック
            // --------------------------------------------------
            if (consecutiveCells.Count >= _connectCount)
            {
                result.Add((new List<BoardIndex>(consecutiveCells), lastValue));
            }

            return result;
        }
    }
}