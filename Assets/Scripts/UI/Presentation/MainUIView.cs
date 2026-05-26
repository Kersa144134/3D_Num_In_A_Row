// ======================================================
// MainUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : メイン UI の描画処理を担当するビュー
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UISystem.Application;

namespace UISystem.Presentation
{
    /// <summary>
    /// メイン UI ビュー
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

        /// <summary>
        /// 制限時間表示テキスト
        /// インデックス 1 以降はエフェクト用とする
        /// </summary>
        private readonly TextMeshProUGUI[] _limitTimeTexts;

        /// <summary>ポインター</summary>
        private readonly GameObject _pointer;

        /// <summary>ポインター Rect</summary>
        private readonly RectTransform _pointerRect;

        /// <summary>Canvas Rect</summary>
        private readonly RectTransform _canvasRect;

        /// <summary>通常フォーカス時カラー</summary>
        private Color _normalFocusOnColor;

        /// <summary>通常非フォーカス時カラー</summary>
        private Color _normalFocusOffColor;

        /// <summary>前回表示スコア</summary>
        private int[] _previousDisplayScores;

        /// <summary>前回表示秒数</summary>
        private int _previousDisplayTotalSeconds;

        /// <summary>時間表示用の数値配列（分・秒）</summary>
        private int[] _timeValues = new int[2];

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 通常ボタン に紐づく Image キャッシュ
        /// </summary>
        private readonly Dictionary<Button, Image> _normalButtonImageCache =
            new Dictionary<Button, Image>();

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
            in TextMeshProUGUI[] limitTimeTexts,
            in GameObject pointer,
            in Color normalFocusOnColor,
            in Color normalfocusOffColor)
        {
            _scoreTexts = scoreTexts;
            _limitTimeTexts = limitTimeTexts;
            _pointer = pointer;
            _normalFocusOnColor = normalFocusOnColor;
            _normalFocusOffColor = normalfocusOffColor;

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
            if (_limitTimeTexts != null)
            {
                _timeFormatter =
                    new TextFormatter(
                        LIMIT_TIME_FORMAT,
                        LIMIT_TIME_DIGITS);
            }

            // --------------------------------------------------
            // ポインター初期化
            // --------------------------------------------------
            if (_pointer != null)
            {
                // RectTransform を取得
                _pointerRect = _pointer.GetComponent<RectTransform>();

                // 親 Canvas を取得
                Canvas canvas = _pointer.GetComponentInParent<Canvas>();

                if (canvas != null)
                {
                    // Canvas の RectTransform をキャッシュ
                    _canvasRect = canvas.transform as RectTransform;
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// 通常ボタンのフォーカス状態を更新する
        /// </summary>
        /// <param name="button">対象ボタン</param>
        /// <param name="isFocus">フォーカス状態</param>
        public void SetNormalFocus(in Button button, in bool isFocus)
        {
            // 通常ボタン辞書へ登録
            RegisterButtonImageCache(
                button,
                _normalButtonImageCache);

            SetFocusState(
                button,
                isFocus,
                _normalButtonImageCache,
                _normalFocusOnColor,
                _normalFocusOffColor);
        }

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
            Debug.Log($"playerId {playerId}, score {score}");
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
            if (_limitTimeTexts == null)
            {
                return;
            }

            for (int i = 0; i < _limitTimeTexts.Length; i++)
            {
                if (i == 0)
                {
                    _limitTimeTexts[i].enabled = isVisible;

                    continue;
                }

                // エフェクト用テキストは非表示時のみ制御対象とする
                if (!isVisible)
                {
                    _limitTimeTexts[i].enabled = false;
                }
            }
        }

        /// <summary>
        /// 制限時間表示更新
        /// </summary>
        public void UpdateLimitTime(in float limitTime)
        {
            if (_limitTimeTexts == null)
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
            for (int i = 0; i < _limitTimeTexts.Length; i++)
            {
                _limitTimeTexts[i].SetCharArray(buffer);
            }
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
            if (_pointer == null)
            {
                return;
            }

            _pointer.SetActive(isVisible);
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

        /// <summary>
        /// Button と Image の対応情報を登録する
        /// </summary>
        /// <param name="button">対象ボタン</param>
        /// <param name="cacheDictionary">登録先辞書</param>
        private void RegisterButtonImageCache(
            in Button button,
            in Dictionary<Button, Image> cacheDictionary)
        {
            if (button == null)
            {
                return;
            }

            // 既に登録済みの場合は処理なし
            if (cacheDictionary.ContainsKey(button))
            {
                return;
            }

            // Button 本体の Image を取得
            Image image = button.image;

            if (image == null)
            {
                return;
            }

            cacheDictionary.Add(button, image);
        }

        /// <summary>
        /// 指定ボタンのフォーカス状態を更新する
        /// </summary>
        /// <param name="button">対象ボタン</param>
        /// <param name="isFocus">フォーカス状態</param>
        /// <param name="cacheDictionary">対象辞書</param>
        /// <param name="focusOnColor">フォーカス ON 時の色</param>
        /// <param name="focusOffColor">対象ボタンの OFF 色</param>
        private void SetFocusState(
            in Button button,
            in bool isFocus,
            in Dictionary<Button, Image> cacheDictionary,
            in Color focusOnColor,
            in Color focusOffColor)
        {
            if (button == null)
            {
                return;
            }
            
            // --------------------------------------------------
            // 指定辞書のフォーカス状態更新
            // --------------------------------------------------
            foreach (KeyValuePair<Button, Image> cache in cacheDictionary)
            {
                if (cache.Value == null)
                {
                    continue;
                }

                // 対象ボタンの場合
                if (cache.Key == button)
                {
                    // フォーカス状態に応じた色を設定
                    cache.Value.color = isFocus
                        ? focusOnColor
                        : focusOffColor;

                    continue;
                }

                // 対象外ボタンはフォーカス OFF 状態へ変更
                cache.Value.color = focusOffColor;
            }
        }
    }
}