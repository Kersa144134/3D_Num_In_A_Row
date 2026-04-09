// ======================================================
// LogRotationModel.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : ログ表示の状態管理・寿命管理・配置計算を行うモデル
// ======================================================

using System.Collections.Generic;
using UnityEngine;

namespace UISystem.Domain
{
    /// <summary>
    /// ログ表示の状態管理モデル
    /// </summary>
    public sealed class LogRotationModel
    {
        // ======================================================
        // プライベートクラス
        // ======================================================

        /// <summary>
        /// ログ内部データ
        /// </summary>
        private sealed class LogData
        {
            /// <summary>ログメッセージ</summary>
            public string Message;

            /// <summary>追加時刻</summary>
            public float AddedTime;
        }

        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>縦方向</summary>
        public enum VerticalDirection
        {
            Positive,
            Negative
        }

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>最大表示行数</summary>
        private const int VISIBLE_LINE_COUNT = 5;

        /// <summary>行間距離</summary>
        private const float LINE_SPACING_Y = 100.0f;

        /// <summary>基準 X 座標</summary>
        private const float BASE_POSITION_X = -10.0f;

        /// <summary>1行目 Y 座標</summary>
        private const float BASE_FIRST_Y = -50.0f;

        /// <summary>表示時間</summary>
        private const float LOG_VISIBLE_DURATION = 3.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ログキュー</summary>
        private readonly Queue<LogData> _logQueue;

        /// <summary>縦方向設定</summary>
        private readonly VerticalDirection _verticalDirection;

        /// <summary>ターゲット座標配列</summary>
        private readonly Vector2[] _targetPositions;

        /// <summary>排出対象ログリスト</summary>
        private readonly List<LogData> _exitingLogs;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LogRotationModel(in VerticalDirection verticalDirection)
        {
            _verticalDirection = verticalDirection;

            _logQueue = new Queue<LogData>();

            _exitingLogs = new List<LogData>();

            _targetPositions = CreateTargetPositions();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ログを追加する
        /// </summary>
        public void AddLog(in string message)
        {
            // 新しいログデータを生成してキューへ追加
            _logQueue.Enqueue(new LogData
            {
                Message = message,
                AddedTime = Time.unscaledTime
            });
        }

        /// <summary>
        /// 状態更新
        /// </summary>
        public void Update(in float currentTime)
        {
            // 排出処理を実行
            ProcessRemoval(currentTime);
        }

        /// <summary>
        /// ビュー用データを取得する
        /// </summary>
        public List<LogViewData> GetViewData()
        {
            // 出力用リストを生成
            List<LogViewData> result = new List<LogViewData>();

            int index = 1;

            // 表示中ログを列挙
            foreach (LogData data in _logQueue)
            {
                // インデックス上限制限
                int clampedIndex =
                    index > VISIBLE_LINE_COUNT
                        ? VISIBLE_LINE_COUNT
                        : index;

                // ビューデータを生成
                result.Add(new LogViewData
                {
                    Index = clampedIndex,
                    Message = data.Message,
                    TargetPosition = _targetPositions[clampedIndex],
                    IsExiting = false
                });

                index++;
            }

            // 排出ログを追加
            for (int i = 0; i < _exitingLogs.Count; i++)
            {
                LogData data = _exitingLogs[i];

                result.Add(new LogViewData
                {
                    Index = 0,
                    Message = data.Message,
                    TargetPosition = _targetPositions[0],
                    IsExiting = true
                });
            }

            return result;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ターゲット座標生成
        /// </summary>
        private Vector2[] CreateTargetPositions()
        {
            int total = VISIBLE_LINE_COUNT + 1;

            Vector2[] positions = new Vector2[total];

            // 非表示行
            float hiddenY =
                (_verticalDirection == VerticalDirection.Negative)
                    ? BASE_FIRST_Y + LINE_SPACING_Y
                    : BASE_FIRST_Y - LINE_SPACING_Y;

            positions[0] = new Vector2(BASE_POSITION_X, hiddenY);

            // 表示行
            for (int i = 1; i < total; i++)
            {
                float offset = LINE_SPACING_Y * (i - 1);

                float y =
                    (_verticalDirection == VerticalDirection.Negative)
                        ? BASE_FIRST_Y - offset
                        : BASE_FIRST_Y + offset;

                positions[i] = new Vector2(BASE_POSITION_X, y);
            }

            return positions;
        }

        /// <summary>
        /// 排出処理
        /// </summary>
        private void ProcessRemoval(in float currentTime)
        {
            while (_logQueue.Count > 0)
            {
                // 先頭要素を取得
                LogData oldest = _logQueue.Peek();

                // 時間超過判定
                bool isExpiredByTime =
                    currentTime - oldest.AddedTime >= LOG_VISIBLE_DURATION;

                // 行数超過判定
                bool isExpiredByCount =
                    _logQueue.Count > VISIBLE_LINE_COUNT;

                // 排出不要なら終了
                if (!isExpiredByTime && !isExpiredByCount)
                {
                    break;
                }

                // キューから削除
                LogData removed = _logQueue.Dequeue();

                // 排出リストへ追加
                _exitingLogs.Add(removed);
            }
        }
    }
}