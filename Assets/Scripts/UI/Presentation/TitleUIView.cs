// ======================================================
// TitleUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : タイトル UI の描画処理を担当するビュー
// ======================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UISystem.Application;

namespace UISystem.Presentation
{
    /// <summary>
    /// タイトル UI ビュー
    /// </summary>
    public sealed class TitleUIView
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ポインターImage</summary>
        private readonly Image _pointerImage;

        /// <summary>ポインター Rect</summary>
        private readonly RectTransform _pointerRect;

        /// <summary>Canvas Rect</summary>
        private readonly RectTransform _canvasRect;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TitleUIView(in Image pointerImage)
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
    }
}