// ======================================================
// MainUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : メインUIの描画処理を担当するビュー
// ======================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UISystem.Application;

namespace UISystem.Presentation
{
    /// <summary>
    /// メインUI描画ビュー
    /// </summary>
    public sealed class MainUIView
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>スコアフォーマットクラス</summary>
        private readonly TextFormatter[] _scoreFormatters;

        /// <summary>時間フォーマットクラス</summary>
        private readonly TextFormatter _timeFormatter;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>スコア表示テキスト</summary>
        private readonly TextMeshProUGUI[] _scoreTexts;

        /// <summary>制限時間表示テキスト</summary>
        private readonly TextMeshProUGUI _limitTimeText;

        /// <summary>ポインターImage</summary>
        private readonly Image _pointerImage;

        /// <summary>ポインターRect</summary>
        private readonly RectTransform _pointerRect;

        /// <summary>Canvas Rect</summary>
        private readonly RectTransform _canvasRect;

        /// <summary>前回表示スコア</summary>
        private int[] _previousDisplayScores;

        /// <summary>前回表示秒数</summary>
        private int _previousDisplayTotalSeconds;

        /// <summary>時間表示用の数値配列（分・秒）</summary>
        private int[] _timeValues = new int[2];

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>スコア表示フォーマット</summary>
        private const string SCORE_FORMAT = "Score: {0}";

        /// <summary>スコア桁数</summary>
        private static readonly int[] SCORE_DIGIT = { 6 };

        /// <summary>制限時間表示フォーマット</summary>
        private const string LIMIT_TIME_FORMAT = "{0}:{1}";

        /// <summary>制限時間桁数</summary>
        private static readonly int[] LIMIT_TIME_DIGITS = { 2, 2 };

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainUIView(
            in TextMeshProUGUI[] scoreTexts,
            in TextMeshProUGUI limitTimeText,
            in Image pointerImage)
        {
            _scoreTexts = scoreTexts;
            _limitTimeText = limitTimeText;

            _pointerImage = pointerImage;

            _previousDisplayTotalSeconds = -1;

            // --------------------------------------------------
            // スコア初期化
            // --------------------------------------------------
            if (_scoreTexts != null)
            {
                int length = _scoreTexts.Length;

                // scoreTexts の長さで配列生成
                _scoreFormatters = new TextFormatter[length];
                _previousDisplayScores = new int[length];

                for (int i = 0; i < length; i++)
                {
                    // 各 Text ごとにフォーマッタ生成
                    _scoreFormatters[i] =
                        new TextFormatter(
                            SCORE_FORMAT,
                            SCORE_DIGIT);
                }
            }

            // --------------------------------------------------
            // タイマー初期化
            // --------------------------------------------------
            if (_limitTimeText != null)
            {
                _timeFormatter =
                    new TextFormatter(
                        LIMIT_TIME_FORMAT,
                        LIMIT_TIME_DIGITS);
            }

            // --------------------------------------------------
            // ポインター初期化
            // --------------------------------------------------
            if (_pointerImage != null)
            {
                _pointerRect = _pointerImage.rectTransform;

                Canvas canvas =
                    _pointerImage.GetComponentInParent<Canvas>();

                if (canvas != null)
                {
                    _canvasRect = canvas.transform as RectTransform;
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// スコア表示更新
        /// </summary>
        /// <param name="playerId">プレイヤーID（1 ベース）</param>
        /// <param name="score">スコア</param>
        public void UpdateScore(in int playerId, in int score)
        {
            // --------------------------------------------------
            // インデックス変換（1 ベース → 0 ベース）
            // --------------------------------------------------
            int index = playerId - 1;

            if (_scoreTexts == null ||
                index < 0 ||
                index >= _scoreTexts.Length)
            {
                return;
            }

            TMP_Text targetText = _scoreTexts[index];

            if (targetText == null)
            {
                return;
            }

            // --------------------------------------------------
            // スコア計算
            // --------------------------------------------------
            // 同一値なら更新スキップ
            if (score == _previousDisplayScores[index])
            {
                return;
            }

            _previousDisplayScores[index] = score;

            // --------------------------------------------------
            // 表示更新
            // --------------------------------------------------
            // フォーマット済みバッファ取得
            char[] buffer = _scoreFormatters[index].Format(score);

            // TextMeshPro に反映
            targetText.SetCharArray(buffer);
        }

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        /// <summary>
        /// 制限時間テキストの表示状態を切り替える
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        public void SetLimitTimeVisible(in bool isVisible)
        {
            if (_limitTimeText == null)
            {
                return;
            }

            _limitTimeText.enabled = isVisible;
        }

        /// <summary>
        /// 制限時間表示更新
        /// </summary>
        public void UpdateLimitTime(in float limitTime)
        {
            if (_limitTimeText == null)
            {
                return;
            }

            // --------------------------------------------------
            // 時間計算
            // --------------------------------------------------
            // 秒へ変換
            int totalSeconds = Mathf.CeilToInt(limitTime);

            // 同一値なら更新スキップ
            if (totalSeconds == _previousDisplayTotalSeconds)
            {
                return;
            }

            _previousDisplayTotalSeconds = totalSeconds;

            // 分計算
            int minutes = totalSeconds / 60;

            // 秒計算
            int seconds = totalSeconds % 60;

            // --------------------------------------------------
            // 表示更新
            // --------------------------------------------------
            // 時間表示配列を更新
            _timeValues[0] = minutes;
            _timeValues[1] = seconds;

            // フォーマット済みバッファ取得
            char[] buffer = _timeFormatter.Format(_timeValues);

            // TextMeshPro に反映
            _limitTimeText.SetCharArray(buffer);
        }

        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を切り替える
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        public void SetPointerVisible(in bool isVisible)
        {
            if (_pointerImage == null)
            {
                return;
            }

            _pointerImage.enabled = isVisible;
        }

        /// <summary>
        /// ポインター位置更新
        /// </summary>
        public void UpdatePointer(in Vector2 screenPosition)
        {
            if (_pointerRect == null || _canvasRect == null)
            {
                return;
            }

            // Canvas中心基準へ変換
            Vector2 anchoredPos = screenPosition - (_canvasRect.sizeDelta * 0.5f);

            // 位置反映
            _pointerRect.anchoredPosition = anchoredPos;
        }
    }
}