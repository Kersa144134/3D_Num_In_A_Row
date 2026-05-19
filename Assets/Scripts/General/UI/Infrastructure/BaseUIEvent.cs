// ======================================================
// BaseUIEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-19
// 更新日時 : 2026-05-19
// 概要     : UI 入力通知の共通処理を管理する基底クラス
// ======================================================

using System;
using UISystem.Domain;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// UI イベント通知を管理する基底クラス
    /// </summary>
    public abstract class BaseUIEvent :
        MonoBehaviour,
        IDisposable,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler,
        ISubmitHandler
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>RectTransform キャッシュ</summary>
        private RectTransform _rectTransform;

        /// <summary>Dispose 実行済みフラグ</summary>
        private bool _isDisposed;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>RectTransform キャッシュ</summary>
        public RectTransform RectTransform => _rectTransform;

        // ======================================================
        // UniRx 変数
        // ======================================================

        // --------------------------------------------------
        // クリック
        // --------------------------------------------------
        /// <summary>クリック通知用 Subject</summary>
        protected readonly Subject<UIClickType> _onClick = new Subject<UIClickType>();

        /// <summary>クリックストリーム</summary>
        public IObservable<UIClickType> OnClick => _onClick;

        // --------------------------------------------------
        // ホバー
        // --------------------------------------------------
        /// <summary>ホバー開始通知用 Subject</summary>
        protected readonly Subject<Unit> _onHoverEnter = new Subject<Unit>();

        /// <summary>ホバー開始ストリーム</summary>
        public IObservable<Unit> OnHoverEnter => _onHoverEnter;

        /// <summary>ホバー終了通知用 Subject</summary>
        protected readonly Subject<Unit> _onHoverExit = new Subject<Unit>();

        /// <summary>ホバー終了ストリーム</summary>
        public IObservable<Unit> OnHoverExit => _onHoverExit;

        // --------------------------------------------------
        // 選択
        // --------------------------------------------------
        /// <summary>選択開始通知用 Subject</summary>
        protected readonly Subject<Unit> _onSelectEnter = new Subject<Unit>();

        /// <summary>選択開始ストリーム</summary>
        public IObservable<Unit> OnSelectEnter => _onSelectEnter;

        /// <summary>選択終了通知用 Subject</summary>
        protected readonly Subject<Unit> _onSelectExit = new Subject<Unit>();

        /// <summary>選択終了ストリーム</summary>
        public IObservable<Unit> OnSelectExit => _onSelectExit;

        // ======================================================
        // Unityイベント
        // ======================================================

        protected virtual void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        // ======================================================
        // IDisposable
        // ======================================================

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public virtual void Dispose()
        {
            // 多重実行防止
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _onClick?.Dispose();
            _onHoverEnter?.Dispose();
            _onHoverExit?.Dispose();
            _onSelectEnter?.Dispose();
            _onSelectExit?.Dispose();
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
        public virtual void OnPointerClick(
            PointerEventData eventData)
        {
            // 入力種別を判定
            UIClickType clickType = eventData.button switch
            {
                PointerEventData.InputButton.Left => UIClickType.Left,
                PointerEventData.InputButton.Right => UIClickType.Right,
                PointerEventData.InputButton.Middle => UIClickType.Middle,
                _ => UIClickType.None
            };

            if (clickType == UIClickType.None)
            {
                return;
            }

            _onClick?.OnNext(clickType);
        }

        /// <summary>
        /// 決定入力
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public virtual void OnSubmit(
            BaseEventData eventData)
        {
            // 左クリック入力として扱う
            _onClick?.OnNext(UIClickType.Left);
        }

        // --------------------------------------------------
        // ホバー
        // --------------------------------------------------
        /// <summary>
        /// マウスホバー開始
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public virtual void OnPointerEnter(
            PointerEventData eventData)
        {
            _onHoverEnter?.OnNext(Unit.Default);
        }

        /// <summary>
        /// マウスホバー終了
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public virtual void OnPointerExit(
            PointerEventData eventData)
        {
            _onHoverExit?.OnNext(Unit.Default);
        }

        // --------------------------------------------------
        // 選択
        // --------------------------------------------------
        /// <summary>
        /// EventSystem 選択開始
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public virtual void OnSelect(
            BaseEventData eventData)
        {
            _onSelectEnter?.OnNext(Unit.Default);
        }

        /// <summary>
        /// EventSystem 選択終了
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public virtual void OnDeselect(
            BaseEventData eventData)
        {
            _onSelectExit?.OnNext(Unit.Default);
        }
    }
}