// ======================================================
// TitleUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-05-07
// 概要     : タイトル UI の描画処理を担当するビュー
// ======================================================

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

        /// <summary>ポインター画像</summary>
        private readonly Image _pointerImage;

        /// <summary>ポインターRect</summary>
        private readonly RectTransform _pointerRect;

        /// <summary>CanvasRect</summary>
        private readonly RectTransform _canvasRect;

        /// <summary>ホバー状態Image配列</summary>
        private readonly Image[] _hoverImages;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TitleUIView(
            in Image pointerImage,
            in Color selectColor,
            in Color deselectColor)
        {
            _pointerImage = pointerImage;

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

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// ホバー状態更新
        /// </summary>
        public void SetHover(in int index, in bool isHover)
        {
            if (_hoverImages[index] == null)
            {
                return;
            }

            // ホバー時は白 / 非ホバー時は黒
            _hoverImages[index].color =
                isHover ? Color.white : Color.black;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// Selectable対象Transform検索
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
                Transform result =
                    FindSelectableTransform(child);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}