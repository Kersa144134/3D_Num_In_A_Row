// ======================================================
// LogRotationUIController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-20
// 更新日時 : 2026-01-23
// 概要     : ログ表示をスロット方式で制御する UI コントローラー
//            ターゲット座標に基づく線形補間移動と
//            TextMeshPro の循環再利用を行う
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace UISystem.Controller
{
    public sealed class LogRotationUIController
    {
        // ======================================================
        // プライベートクラス
        // ======================================================

        /// <summary>
        /// 表示中ログの管理情報
        /// </summary>
        private sealed class LogEntry
        {
            /// <summary>表示対象の RectTransform</summary>
            public RectTransform Rect;

            /// <summary>キューに追加された時刻</summary>
            public float AddedTime;
        }

        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>ログの縦方向表示方向</summary>
        public enum VerticalDirection
        {
            Positive,
            Negative
        }

        /// <summary>ログの挿入方向</summary>
        public enum InsertDirection
        {
            Left,
            Right
        }

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>表示可能行数</summary>
        private const int VISIBLE_LINE_COUNT = 5;

        /// <summary>ログ1行分の縦方向間隔</summary>
        private const float LINE_SPACING_Y = 100.0f;

        /// <summary>ログ表示の基準X座標</summary>
        private const float LOG_BASE_POSITION_X = -10.0f;

        /// <summary>ログ表示1行目の基準Y座標</summary>
        private const float LOG_BASE_FIRST_POSITION_Y = -50.0f;

        /// <summary>ログ挿入時の初期X移動距離</summary>
        private const float INSERT_OFFSET_X = 1000.0f;

        /// <summary>ログの移動速度</summary>
        private const float MOVE_SPEED = 2000.0f;

        /// <summary>ログが表示され続ける秒数</summary>
        private const float LOG_VISIBLE_DURATION = 3.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ログ表示に使用する TextMeshPro 配列（循環利用）</summary>
        private readonly TextMeshProUGUI[] _logTexts;

        /// <summary>縦方向の表示方向</summary>
        private readonly VerticalDirection _verticalDirection;

        /// <summary>ログ挿入方向</summary>
        private readonly InsertDirection _insertDirection;

        /// <summary>ログ表示用ターゲット座標配列（0 は非表示行）</summary>
        private Vector2[] _targetPositions;

        /// <summary>表示中ログの管理キュー</summary>
        private readonly Queue<LogEntry> _logQueue;
        
        /// <summary>非表示行へ補間移動中のログ管理リスト</summary>
        private readonly List<RectTransform> _exitingLogs = new List<RectTransform>();

        /// <summary>次に使用する TextMeshPro の循環インデックス</summary>
        private int _currentTextIndex;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public LogRotationUIController(
            in TextMeshProUGUI[] logTexts,
            in VerticalDirection verticalDirection,
            in InsertDirection insertDirection)
        {
            _logTexts = logTexts;
            _verticalDirection = verticalDirection;
            _insertDirection = insertDirection;

            _logQueue = new Queue<LogEntry>();

            _currentTextIndex = 0;

            // ターゲット座標を初期化
            InitializeTargetPositions();

            // 全ログを非表示行へ初期配置
            InitializeLogTextPositions();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 新しいログを追加する
        /// </summary>
        public void AddLog(in string logMessage)
        {
            TextMeshProUGUI logText = GetNextLogText();
            RectTransform rect = logText.rectTransform;

            // 表示テキストを設定
            logText.text = logMessage;

            // --------------------------------------------------
            // 挿入方向に応じた初期座標を算出、配置
            // --------------------------------------------------
            float startX = GetInsertStartX();

            // 追加後に配置される表示行インデックスを算出
            int targetLineIndex = GetTargetLineIndex();
            float startY = _targetPositions[targetLineIndex].y;

            // 初期位置へ配置
            rect.anchoredPosition = new Vector2(startX, startY);

            // --------------------------------------------------
            // 管理情報を生成してキューに追加
            // --------------------------------------------------
            _logQueue.Enqueue(
                new LogEntry
                {
                    Rect = rect,
                    AddedTime = Time.unscaledTime
                });
        }

        /// <summary>
        /// 毎フレーム呼び出してログを制御する
        /// </summary>
        public void Update(in float unscaledDeltaTime)
        {
            ProcessLogRemoval();
            UpdateLogPositions(unscaledDeltaTime);
            UpdateExitingLogPositions(unscaledDeltaTime);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ログ排出条件を満たす要素を非表示行へ移動させる
        /// </summary>
        private void ProcessLogRemoval()
        {
            // timeScaleに影響されない経過時間を使用する
            float currentTime = Time.unscaledTime;

            while (_logQueue.Count > 0)
            {
                LogEntry oldest = _logQueue.Peek();

                // 表示時間超過判定
                bool isExpiredByTime =
                    currentTime - oldest.AddedTime >= LOG_VISIBLE_DURATION;

                // 行数超過判定
                bool isExpiredByLineCount =
                    _logQueue.Count > VISIBLE_LINE_COUNT;

                // 排出条件を満たさない場合は終了
                if (!isExpiredByTime && !isExpiredByLineCount)
                {
                    break;
                }

                // 排出対象を取得
                LogEntry removed = _logQueue.Dequeue();

                // 非表示行へ補間移動させるため退場リストへ追加
                _exitingLogs.Add(removed.Rect);
            }
        }

        /// <summary>
        /// 表示中ログをターゲット座標へ補間移動させる
        /// </summary>
        private void UpdateLogPositions(in float unscaledDeltaTime)
        {
            float moveDelta = MOVE_SPEED * unscaledDeltaTime;

            int index = 1;

            foreach (LogEntry entry in _logQueue)
            {
                RectTransform rect = entry.Rect;

                // 対応するターゲット座標を取得
                Vector2 target = _targetPositions[index];

                // 補間移動処理
                MoveTowardsTarget(rect, target, moveDelta);

                index++;
            }
        }

        /// <summary>
        /// 非表示行へ移動中のログを補間更新する
        /// </summary>
        private void UpdateExitingLogPositions(in float unscaledDeltaTime)
        {
            float moveDelta = MOVE_SPEED * unscaledDeltaTime;
            Vector2 hiddenTarget = _targetPositions[0];

            for (int i = _exitingLogs.Count - 1; i >= 0; i--)
            {
                RectTransform rect = _exitingLogs[i];

                Vector2 current = rect.anchoredPosition;

                float nextY =
                    Mathf.MoveTowards(current.y, hiddenTarget.y, moveDelta);

                rect.anchoredPosition =
                    new Vector2(current.x, nextY);

                // 非表示行に到達したら管理対象から除外
                if (Mathf.Approximately(nextY, hiddenTarget.y))
                {
                    _exitingLogs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 指定 RectTransform をターゲット座標へ線形補間移動させる
        /// </summary>
        private void MoveTowardsTarget(
            in RectTransform rect,
            in Vector2 target,
            in float moveDelta)
        {
            Vector2 current = rect.anchoredPosition;

            float nextX =
                Mathf.MoveTowards(current.x, target.x, moveDelta);

            float nextY =
                Mathf.MoveTowards(current.y, target.y, moveDelta);

            rect.anchoredPosition = new Vector2(nextX, nextY);
        }

        /// <summary>
        /// ログ挿入時に使用する初期 X 座標を取得する
        /// </summary>
        private float GetInsertStartX()
        {
            // --------------------------------------------------
            // 左挿入の場合、負方向にオフセットした位置を初期位置に設定
            // 右挿入の場合、正方向にオフセットした位置を初期位置に設定
            // --------------------------------------------------
            return
                (_insertDirection == InsertDirection.Left)
                    ? LOG_BASE_POSITION_X - INSERT_OFFSET_X
                    : LOG_BASE_POSITION_X + INSERT_OFFSET_X;
        }

        /// <summary>
        /// 現在のキュー状態から表示行インデックスを算出する
        /// </summary>
        private int GetTargetLineIndex()
        {
            int index = _logQueue.Count + 1;

            if (index > VISIBLE_LINE_COUNT)
            {
                index = VISIBLE_LINE_COUNT;
            }

            return index;
        }

        /// <summary>
        /// ログ表示に使用するターゲット座標配列を初期化する
        /// </summary>
        private void InitializeTargetPositions()
        {
            int totalLineCount = VISIBLE_LINE_COUNT + 1;
            _targetPositions = new Vector2[totalLineCount];

            // 非表示行の Y 座標を算出
            float hiddenLineY =
                (_verticalDirection == VerticalDirection.Negative)
                    ? LOG_BASE_FIRST_POSITION_Y + LINE_SPACING_Y
                    : LOG_BASE_FIRST_POSITION_Y - LINE_SPACING_Y;

            // 非表示行のターゲット座標を設定
            _targetPositions[0] =
                new Vector2(LOG_BASE_POSITION_X, hiddenLineY);

            // 表示行（index 1 ～）のターゲット座標を設定
            for (int i = 1; i < totalLineCount; i++)
            {
                // 表示1行目からのオフセット量を算出
                float offset = LINE_SPACING_Y * (i - 1);

                // 表示方向に応じて Y 座標を決定
                float y =
                    (_verticalDirection == VerticalDirection.Negative)
                        ? LOG_BASE_FIRST_POSITION_Y - offset
                        : LOG_BASE_FIRST_POSITION_Y + offset;

                // 表示行のターゲット座標を設定
                _targetPositions[i] =
                    new Vector2(LOG_BASE_POSITION_X, y);
            }
        }


        /// <summary>
        /// すべてのログテキストを非表示行へ初期配置する
        /// </summary>
        private void InitializeLogTextPositions()
        {
            // 非表示行のターゲット座標を取得
            Vector2 hiddenPosition = _targetPositions[0];

            // すべての TextMeshPro を非表示行へ瞬間移動
            for (int i = 0; i < _logTexts.Length; i++)
            {
                _logTexts[i].rectTransform.anchoredPosition = hiddenPosition;
            }
        }

        /// <summary>
        /// 次に使用する TextMeshPro を取得する
        /// </summary>
        private TextMeshProUGUI GetNextLogText()
        {
            TextMeshProUGUI text = _logTexts[_currentTextIndex];

            _currentTextIndex++;

            // 循環処理
            if (_currentTextIndex >= _logTexts.Length)
            {
                _currentTextIndex = 0;
            }

            return text;
        }
    }
}