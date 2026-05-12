// ======================================================
// TitleUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-05-07
// 概要     : タイトル UI の描画処理を担当するビュー
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem.Presentation
{
    /// <summary>
    /// タイトル UI ビュー
    /// </summary>
    public sealed class TitleUIView
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>SelectImage タグ</summary>
        private const string SELECTABLE_TAG = "Selectable";

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ポインター</summary>
        private readonly GameObject _pointer;

        /// <summary>ポインターRect</summary>
        private readonly RectTransform _pointerRect;

        /// <summary>CanvasRect</summary>
        private readonly RectTransform _canvasRect;

        /// <summary>通常フォーカス時カラー</summary>
        private Color _normalFocusOnColor;

        /// <summary>通常非フォーカス時カラー</summary>
        private Color _normalFocusOffColor;

        /// <summary>オプション選択時カラー</summary>
        private Color _optionSelectOnColor;

        /// <summary>オプション非選択時カラー</summary>
        private Color _optionSelectOffColor;

        /// <summary>オプションフォーカス時カラー</summary>
        private Color _optionFocusOnColor;

        /// <summary>オプション非フォーカス時カラー</summary>
        private Color _optionFocusOffColor;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 通常ボタン に紐づく Image キャッシュ
        /// </summary>
        private readonly Dictionary<Button, Image> _normalButtonImageCache =
            new Dictionary<Button, Image>();

        /// <summary>
        /// オプションボタン に紐づく Image キャッシュ
        /// </summary>
        private readonly Dictionary<Button, Image> _optionButtonImageCache =
            new Dictionary<Button, Image>();

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TitleUIView(
            in GameObject pointer,
            in Color normalFocusOnColor,
            in Color normalfocusOffColor,
            in Color optionSelectOnColor,
            in Color optionSelectOffColor,
            in Color optionFocusOnColor,
            in Color optionFocusOffColor)
        {
            _pointer = pointer;
            _normalFocusOnColor = normalFocusOnColor;
            _normalFocusOffColor = normalfocusOffColor;
            _optionSelectOnColor = optionSelectOnColor;
            _optionSelectOffColor = optionSelectOffColor;
            _optionFocusOnColor = optionFocusOnColor;
            _optionFocusOffColor = optionFocusOffColor;

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

        /// <summary>
        /// ポインター位置更新
        /// </summary>
        public void UpdatePointer(in Vector2 screenPosition)
        {
            if (_pointerRect == null || _canvasRect == null)
            {
                return;
            }

            // Canvas 中心基準へ変換
            Vector2 anchoredPos = screenPosition - (_canvasRect.sizeDelta * 0.5f);

            // 位置反映
            _pointerRect.anchoredPosition = anchoredPos;
        }

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// ボタン選択状態を全体反映する
        /// </summary>
        /// <param name="buttonArray">対象ボタン配列</param>
        /// <param name="selectStateArray">選択状態配列</param>
        public void ApplyButtonSelectionState(
            in Button[] buttonArray,
            in bool[] selectStateArray)
        {
            if (buttonArray == null || selectStateArray == null)
            {
                return;
            }

            // 要素数チェック
            int count = Mathf.Min(buttonArray.Length, selectStateArray.Length);

            // --------------------------------------------------
            // ボタン状態反映
            // --------------------------------------------------
            for (int index = 0; index < count; index++)
            {
                Button button = buttonArray[index];

                if (button == null)
                {
                    continue;
                }

                // Selectable 取得
                Transform target = FindSelectableTransform(button.transform);

                if (target == null)
                {
                    continue;
                }

                // Image 取得
                Image image = target.GetComponent<Image>();

                if (image == null)
                {
                    continue;
                }

                // 選択状態に応じて色反映
                image.color = selectStateArray[index] ? _optionSelectOnColor : _optionSelectOffColor;
            }
        }

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

        /// <summary>
        /// オプションボタンのフォーカス状態を更新する
        /// </summary>
        /// <param name="button">対象ボタン</param>
        /// <param name="isFocus">フォーカス状態</param>
        public void SetOptionFocus(in Button button, in bool isFocus)
        {
            // オプションボタン辞書へ登録
            RegisterButtonImageCache(
                button,
                _optionButtonImageCache);

            SetFocusState(
                button,
                isFocus,
                _optionButtonImageCache,
                _optionFocusOnColor,
                _optionFocusOffColor);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// Selectable 対象の Transform 検索
        /// </summary>
        private Transform FindSelectableTransform(in Transform root)
        {
            int childCount = root.childCount;

            for (int index = 0; index < childCount; index++)
            {
                Transform child = root.GetChild(index);

                // タグ一致なら返却
                if (child.CompareTag(SELECTABLE_TAG))
                {
                    return child;
                }

                // 再帰検索
                Transform result = FindSelectableTransform(child);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
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

            // 対象外辞書を取得
            Dictionary<Button, Image> otherDictionary =
                cacheDictionary == _normalButtonImageCache
                    ? _optionButtonImageCache
                    : _normalButtonImageCache;

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

            // --------------------------------------------------
            // 非対象辞書のフォーカス状態更新
            // --------------------------------------------------
            foreach (KeyValuePair<Button, Image> cache in otherDictionary)
            {
                if (cache.Value == null)
                {
                    continue;
                }

                // 対象外辞書に対応する OFF カラーを取得
                Color otherDictionaryOffColor =
                    cacheDictionary == _normalButtonImageCache
                        ? _optionFocusOffColor
                        : _normalFocusOffColor;
                
                // 非対象辞書はすべてフォーカス OFF 状態へ変更
                cache.Value.color = otherDictionaryOffColor;
            }
        }
    }
}