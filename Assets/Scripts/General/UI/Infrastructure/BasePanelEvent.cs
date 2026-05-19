// ======================================================
// BasePanelEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-19
// 更新日時 : 2026-05-19
// 概要     : UI パネル入力通知の共通処理を管理する基底クラス
// ======================================================

using UnityEngine.EventSystems;
using UniRx;
using System.Diagnostics;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// UI パネルイベント通知を管理する基底クラス
    /// </summary>
    public abstract class BasePanelEvent : BaseUIEvent
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ホバー状態フラグ</summary>
        protected bool _isHovered;

        // ======================================================
        // Unityイベント
        // ======================================================

        protected override void Awake()
        {
            base.Awake();
        }

        protected virtual void OnDisable()
        {
            // ホバー終了通知
            OnHoverExitInternal();
        }

        protected override void OnDestroy()
        {
            // ホバー終了通知
            OnHoverExitInternal();

            Dispose();
        }

        // ======================================================
        // EventSystem 入力イベント
        // ======================================================

        /// <summary>
        /// クリック入力
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public override void OnPointerClick(
            PointerEventData eventData)
        {
        }

        /// <summary>
        /// マウスホバー開始
        /// </summary>
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
        public override void OnPointerExit(
            PointerEventData eventData)
        {
            // ホバー状態を解除
            _isHovered = false;

            base.OnPointerExit(eventData);
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
    }
}