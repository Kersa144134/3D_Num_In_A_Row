// ======================================================
// MainUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : メインUIの描画処理を担当するビュー
// ======================================================

using System;
using TMPro;
using UISystem.Application;
using UnityEngine;
using UnityEngine.UI;

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

        /// <summary>時間フォーマットサービス</summary>
        private readonly TextFormatter _timeFormatter;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>制限時間表示テキスト</summary>
        private readonly TextMeshProUGUI _limitTimeText;

        /// <summary>ポインターImage</summary>
        private readonly Image _pointerImage;

        /// <summary>ポインターRect</summary>
        private readonly RectTransform _pointerRect;

        /// <summary>Canvas Rect</summary>
        private readonly RectTransform _canvasRect;

        /// <summary>前回表示秒数</summary>
        private int _previousDisplayTotalSeconds;

        /// <summary>時間表示用の数値配列（分・秒）</summary>
        private int[] _timeValues = new int[2];

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>表示フォーマット</summary>
        private const string LIMIT_TIME_FORMAT = "{0}:{1}";

        /// <summary>桁数</summary>
        private static readonly int[] LIMIT_TIME_DIGITS = { 2, 2 };

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainUIView(
            in TextMeshProUGUI limitTimeText,
            in Image pointerImage)
        {
            _limitTimeText = limitTimeText;

            _pointerImage = pointerImage;

            _previousDisplayTotalSeconds = -1;

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
            Vector2 anchoredPos =
                screenPosition - (_canvasRect.sizeDelta * 0.5f);

            // 位置反映
            _pointerRect.anchoredPosition = anchoredPos;
        }

        /// <summary>
        /// 制限時間表示更新
        /// </summary>
        public void UpdateLimitTime(in float remainingTime)
        {
            if (_limitTimeText == null)
            {
                return;
            }

            // --------------------------------------------------
            // 時間計算
            // --------------------------------------------------
            // 秒へ変換
            int totalSeconds =
                Mathf.FloorToInt(remainingTime);

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
    }
}