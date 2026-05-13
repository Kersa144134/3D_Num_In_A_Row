// ======================================================
// BaseButtonEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-09
// 更新日時 : 2026-05-09
// 概要     : UIボタン入力通知の共通処理を管理する基底クラス
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// UIイベント通知を管理する基底クラス
    /// </summary>
    public abstract class BaseButtonEvent :
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

        /// <summary>GameObject キャッシュ</summary>
        private GameObject _gameObject;

        /// <summary>RectTransform キャッシュ</summary>
        private RectTransform _rectTransform;

        /// <summary>Button キャッシュ</summary>
        private Button _button;

        /// <summary>EventSystem ホバー中フラグ</summary>
        private bool _isHovered;

        /// <summary>EventSystem 選択中フラグ</summary>
        private bool _isSelected;

        /// <summary>Dispose 実行済みフラグ</summary>
        private bool _isDisposed;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>GameObject キャッシュ</summary>
        public GameObject GameObject => _gameObject;

        /// <summary>RectTransform キャッシュ</summary>
        public RectTransform RectTransform => _rectTransform;

        /// <summary>Button キャッシュ</summary>
        public Button Button => _button;
        
        // ======================================================
        // UniRx 変数
        // ======================================================

        // --------------------------------------------------
        // クリック
        // --------------------------------------------------
        /// <summary>クリック通知用 Subject</summary>
        protected readonly Subject<Unit> OnClickSubject = new Subject<Unit>();

        /// <summary>クリックストリーム</summary>
        public IObservable<Unit> OnClickAsObservable => OnClickSubject;

        // --------------------------------------------------
        // ホバー
        // --------------------------------------------------
        /// <summary>ホバー開始通知用 Subject</summary>
        protected readonly Subject<Unit> OnHoverEnterSubject = new Subject<Unit>();

        /// <summary>ホバー開始ストリーム</summary>
        public IObservable<Unit> OnHoverEnterAsObservable => OnHoverEnterSubject;

        /// <summary>ホバー終了通知用 Subject</summary>
        protected readonly Subject<Unit> OnHoverExitSubject = new Subject<Unit>();

        /// <summary>ホバー終了ストリーム</summary>
        public IObservable<Unit> OnHoverExitAsObservable => OnHoverExitSubject;

        // --------------------------------------------------
        // 選択
        // --------------------------------------------------
        /// <summary>選択開始通知用 Subject</summary>
        protected readonly Subject<Unit> OnSelectEnterSubject = new Subject<Unit>();

        /// <summary>選択開始ストリーム</summary>
        public IObservable<Unit> OnSelectEnterAsObservable => OnSelectEnterSubject;

        /// <summary>選択終了通知用 Subject</summary>
        protected readonly Subject<Unit> OnSelectExitSubject = new Subject<Unit>();

        /// <summary>選択終了ストリーム</summary>
        public IObservable<Unit> OnSelectExitAsObservable => OnSelectExitSubject;

        // ======================================================
        // Unityイベント
        // ======================================================

        protected virtual void Awake()
        {
            _gameObject = gameObject;
            _rectTransform = transform as RectTransform;
            _button = GetComponent<Button>();
        }

        protected virtual void OnDisable()
        {
            // ホバー中の場合
            if (_isHovered)
            {
                // ホバー終了通知
                OnHoverExitInternal();
            }
            
            // 選択中の場合
            if (_isSelected)
            {
                // 選択終了通知
                OnSelectExitInternal();
            }
        }

        protected virtual void OnDestroy()
        {
            // ホバー中の場合
            if (_isHovered)
            {
                // ホバー終了通知
                OnHoverExitInternal();
            }

            // 選択中の場合
            if (_isSelected)
            {
                // 選択終了通知
                OnSelectExitInternal();
            }

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

            OnClickSubject.Dispose();
            OnHoverEnterSubject.Dispose();
            OnHoverExitSubject.Dispose();
            OnSelectEnterSubject.Dispose();
            OnSelectExitSubject.Dispose();
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
            // 左クリック以外は処理なし
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            OnClickSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// 決定入力
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public virtual void OnSubmit(
            BaseEventData eventData)
        {
            OnClickSubject.OnNext(Unit.Default);
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
            // ホバー状態を有効化
            _isHovered = true;

            OnHoverEnterSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// マウスホバー終了
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public virtual void OnPointerExit(
            PointerEventData eventData)
        {
            // ホバー状態を解除
            _isHovered = false;

            OnHoverExitSubject.OnNext(Unit.Default);
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
            // 選択状態を有効化
            _isSelected = true;

            OnSelectEnterSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// EventSystem 選択終了
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public virtual void OnDeselect(
            BaseEventData eventData)
        {
            // 選択状態を解除
            _isSelected = false;

            OnSelectExitSubject.OnNext(Unit.Default);
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
            OnHoverExitSubject.OnNext(Unit.Default);
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
            OnSelectExitSubject.OnNext(Unit.Default);
        }
    }
}