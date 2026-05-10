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

        /// <summary>選択 ON 時カラー</summary>
        private readonly Color _selectOnColor;

        /// <summary>選択 OFF 時カラー</summary>
        private readonly Color _selectOffColor;

        /// <summary>フォーカス ON 時カラー</summary>
        private readonly Color _focusOnColor;

        /// <summary>フォーカス OFF 時カラー</summary>
        private readonly Color _focusOffColor;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// Button に紐づく Image を保持するキャッシュ辞書
        /// </summary>
        private readonly Dictionary<Button, Image> _buttonImageCache = new();

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TitleUIView(
            in GameObject pointer,
            in Color selectOnColor,
            in Color selectOffColor,
            in Color focusOnColor,
            in Color focusOffColor)
        {
            _pointer = pointer;
            _selectOnColor = selectOnColor;
            _selectOffColor = selectOffColor;
            _focusOnColor = focusOnColor;
            _focusOffColor = focusOffColor;

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
                Button button =
                    buttonArray[index];

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
                image.color = selectStateArray[index] ? _selectOnColor : _selectOffColor;
            }
        }

        /// <summary>
        /// フォーカス状態更新
        /// </summary>
        public void SetFocus(in Button button, in bool isHover)
        {
            if (button == null)
            {
                return;
            }

            // キャッシュから Image を取得
            if (_buttonImageCache.TryGetValue(button, out Image image) == false)
            {
                // Button 本体の Image を取得
                image = button.image;

                if (image == null)
                {
                    return;
                }

                // 辞書登録
                _buttonImageCache.Add(button, image);
            }

            foreach (KeyValuePair<Button, Image> cache in _buttonImageCache)
            {
                if (cache.Value == null)
                {
                    continue;
                }

                // 対象 Button のみ指定状態を反映
                if (cache.Key == button)
                {
                    // フォーカス状態に応じて色反映
                    cache.Value.color = isHover
                        ? _focusOnColor
                        : _focusOffColor;

                    continue;
                }

                // 対象以外はフォーカス OFF 状態へ変更
                cache.Value.color = _focusOffColor;
            }
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
    }
}