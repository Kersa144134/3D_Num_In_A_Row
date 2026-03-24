// ======================================================
// LineJudgeService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-03-17
// 概要     : ライン判定処理
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
    public sealed class LineJudgeService
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>ライン生成ユーティリティ</summary>
        private LineGenerator _lineGenerator;
        
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
        // UniRx 変数
        // ======================================================

        /// <summary>ライン成立イベント</summary>
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
        /// <param name="boardSize">盤面サイズ</param>
        /// <param name="connectCount">ライン成立条件の連続マス数</param>
        public LineJudgeService(in int boardSize, in int connectCount)
        {
            _boardSize = boardSize;
            _connectCount = connectCount;

            // ライン生成クラス初期化
            _lineGenerator = new LineGenerator(_boardSize);

            // ライン生成
            _lines = _lineGenerator.GenerateLines();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 盤面全体のライン判定を行い、成立時イベント通知
        /// </summary>
        public void CheckAll(in BoardState board)
        {
            // プレイヤーごとのライン情報を一時保持
            Dictionary<int, List<int>> result = new Dictionary<int, List<int>>();

            // 全ライン走査
            foreach (int[][] line in _lines)
            {
                // ラインごとの最大連続数を取得
                Dictionary<int, int> maxMap =
                    CalculateLineMaxConsecutive(board, line);

                // ライン結果を集約
                AccumulateResult(maxMap, ref result);
            }

            // イベント通知
            NotifyLineComplete(result);
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
        /// ライン内のプレイヤーごとの最大連続数を算出
        /// </summary>
        private Dictionary<int, int> CalculateLineMaxConsecutive(
            in BoardState board,
            in int[][] line)
        {
            // 直前の値
            int lastValue = 0;

            // 現在の連続数
            int consecutive = 0;

            // プレイヤーごとの最大連続数
            Dictionary<int, int> maxMap = new Dictionary<int, int>();

            // ライン内走査
            foreach (int[] cell in line)
            {
                // 座標取得
                int x = cell[0];
                int y = cell[1];
                int z = cell[2];

                // 盤面値取得
                int value = board.Get(x, y, z);

                // 空マスならリセット
                if (value == 0)
                {
                    consecutive = 0;
                    lastValue = 0;
                    continue;
                }

                // 同一値なら加算
                if (value == lastValue)
                {
                    consecutive++;
                }
                else
                {
                    // 値切り替え
                    lastValue = value;
                    consecutive = 1;
                }

                // プレイヤーごとの最大値初期化
                if (!maxMap.ContainsKey(value))
                {
                    maxMap[value] = 0;
                }

                // 最大連続数更新
                if (consecutive > maxMap[value])
                {
                    maxMap[value] = consecutive;
                }
            }

            return maxMap;
        }

        /// <summary>
        /// ライン成立結果をプレイヤー単位で集約
        /// </summary>
        private void AccumulateResult(
            in Dictionary<int, int> maxMap,
            ref Dictionary<int, List<int>> result)
        {
            foreach (KeyValuePair<int, int> pair in maxMap)
            {
                if (pair.Value < _connectCount)
                {
                    continue;
                }

                // プレイヤーごとのリスト確保
                if (!result.ContainsKey(pair.Key))
                {
                    result[pair.Key] = new List<int>();
                }

                result[pair.Key].Add(pair.Value);
            }
        }

        /// <summary>
        /// ライン成立イベントを発火
        /// </summary>
        private void NotifyLineComplete(
            in Dictionary<int, List<int>> result)
        {
            foreach (KeyValuePair<int, List<int>> pair in result)
            {
                // プレイヤー単位でまとめて発火
                _onLineComplete.OnNext(
                    new LineCompleteEvent(
                        pair.Key,
                        pair.Value.ToArray()));
            }
        }
    }
}