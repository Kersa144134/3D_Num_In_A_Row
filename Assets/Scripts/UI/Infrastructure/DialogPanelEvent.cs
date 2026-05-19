// ======================================================
// DialogPanelEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-19
// 更新日時 : 2026-05-19
// 概要     : ダイアログ用パネルイベント
// ======================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// ダイアログ用パネルイベント
    /// </summary>
    public class DialogPanelEvent : BasePanelEvent
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>ホバー時カラー</summary>
        [SerializeField]
        private Color _hoverColor = Color.white;

        /// <summary>非ホバー時カラー</summary>
        [SerializeField]
        private Color _unhoverColor = Color.gray;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Image キャッシュ</summary>
        private Image _image;

        // ======================================================
        // Unity イベント
        // ======================================================

        protected override void Awake()
        {
            base.Awake();

            // Image 取得
            _image = GetComponent<Image>();

            ApplyColor(_isHovered);
        }

        private void OnEnable()
        {
            ApplyColor(_isHovered);
        }

        // ======================================================
        // 入力イベント
        // ======================================================

        /// <summary>
        /// クリック入力
        /// </summary>
        public override void OnPointerClick(
            PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
        }

        /// <summary>
        /// マウスホバー開始
        /// </summary>
        public override void OnPointerEnter(
            PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            ApplyColor(_isHovered);
        }

        /// <summary>
        /// マウスホバー終了
        /// </summary>
        public override void OnPointerExit(
            PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            ApplyColor(_isHovered);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ホバー状態に応じて色を適用する
        /// </summary>
        /// <param name="isHovered">ホバー中かどうか</param>
        private void ApplyColor(bool isHovered)
        {
            if (_image == null)
            {
                return;
            }

            // 状態に応じて色を切り替える
            _image.color = isHovered ? _hoverColor : _unhoverColor;
        }
    }
}