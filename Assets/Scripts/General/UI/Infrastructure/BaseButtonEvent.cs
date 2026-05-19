// ======================================================
// BaseButtonEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-09
// 更新日時 : 2026-05-19
// 概要     : UI ボタン入力通知の共通処理を管理する基底クラス
// ======================================================

using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// UI ボタンイベント通知を管理する基底クラス
    /// </summary>
    public abstract class BaseButtonEvent :
        BaseUIEvent,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Button キャッシュ</summary>
        private Button _button;

        /// <summary>ホバー中フラグ</summary>
        private bool _isHovered;

        /// <summary>選択中フラグ</summary>
        private bool _isSelected;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>Button キャッシュ</summary>
        public Button Button => _button;
        
        // ======================================================
        // Unityイベント
        // ======================================================

        protected override void Awake()
        {
            base.Awake();
            
            _button = GetComponent<Button>();
        }

        protected virtual void OnDisable()
        {
            // ホバー終了通知
            OnHoverExitInternal();

            // 選択終了通知
            OnSelectExitInternal();
        }

        protected override void OnDestroy()
        {
            // ホバー終了通知
            OnHoverExitInternal();

            // 選択終了通知
            OnSelectExitInternal();

            Dispose();
        }

        // ======================================================
        // EventSystem 入力イベント
        // ======================================================

        // --------------------------------------------------
        // クリック
        // --------------------------------------------------
        /// <summary>
        /// クリック入力
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public override void OnPointerClick(
            PointerEventData eventData)
        {
            // 左クリック以外は処理なし
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            base.OnPointerClick(eventData);
        }

        // --------------------------------------------------
        // ホバー
        // --------------------------------------------------
        /// <summary>
        /// マウスホバー開始
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public override void OnPointerEnter(
            PointerEventData eventData)
        {
            // ホバー状態を有効化
            _isHovered = true;

            base.OnPointerEnter(eventData);
        }

        /// <summary>
        /// マウスホバー終了
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public override void OnPointerExit(
            PointerEventData eventData)
        {
            // ホバー状態を解除
            _isHovered = false;

            base.OnPointerExit(eventData);
        }

        // --------------------------------------------------
        // 選択
        // --------------------------------------------------
        /// <summary>
        /// EventSystem 選択開始
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public override void OnSelect(
            BaseEventData eventData)
        {
            // 選択状態を有効化
            _isSelected = true;

            base.OnSelect(eventData);
        }

        /// <summary>
        /// EventSystem 選択終了
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public override void OnDeselect(
            BaseEventData eventData)
        {
            // 選択状態を解除
            _isSelected = false;

            base.OnDeselect(eventData);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ホバー終了処理を実行する
        /// </summary>
        private void OnHoverExitInternal()
        {
            // 未ホバー状態の場合
            if (!_isHovered)
            {
                return;
            }

            // ホバー状態を解除
            _isHovered = false;

            // ホバー終了通知
            _onHoverExit?.OnNext(Unit.Default);
        }

        /// <summary>
        /// 選択終了処理を実行する
        /// </summary>
        private void OnSelectExitInternal()
        {
            // 未選択状態の場合
            if (!_isSelected)
            {
                return;
            }

            // 選択状態を解除
            _isSelected = false;

            // 選択終了通知
            _onSelectExit?.OnNext(Unit.Default);
        }
    }
}