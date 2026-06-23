// ======================================================
// UIEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-09
// 更新日時 : 2026-05-09
// 概要     : UI のイベント通知を管理する
// ======================================================

using System;
using UISystem.Domain;
using UISystem.Infrastructure;
using UniRx;

namespace UISystem.Application
{
    /// <summary>
    /// UI のイベント通知を管理する
    /// </summary>
    public sealed class UIEventRouter : IDisposable
    {
        // ======================================================
        // UniRx 関連
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        /// <summary>クリック通知</summary>
        private readonly Subject<UIClickEvent> _onClick = new Subject<UIClickEvent>();

        /// <summary>ホバー通知</summary>
        private readonly Subject<BaseUIEvent> _onHover = new Subject<BaseUIEvent>();

        /// <summary>ホバー解除通知</summary>
        private readonly Subject<BaseUIEvent> _onUnHover = new Subject<BaseUIEvent>();

        /// <summary>フォーカス通知</summary>
        private readonly Subject<BaseUIEvent> _onFocus = new Subject<BaseUIEvent>();

        /// <summary>フォーカス解除通知</summary>
        private readonly Subject<BaseUIEvent> _onUnFocus = new Subject<BaseUIEvent>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>クリック通知</summary>
        public IObservable<UIClickEvent> OnClick => _onClick;

        /// <summary>ホバー通知</summary>
        public IObservable<BaseUIEvent> OnHover => _onHover;

        /// <summary>ホバー解除通知</summary>
        public IObservable<BaseUIEvent> OnUnHover => _onUnHover;

        /// <summary>フォーカス通知</summary>
        public IObservable<BaseUIEvent> OnFocus => _onFocus;

        /// <summary>フォーカス解除通知</summary>
        public IObservable<BaseUIEvent> OnUnFocus => _onUnFocus;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// イベント購読を解除する
        /// </summary>
        public void Dispose()
        {
            _disposable?.Dispose();
        }

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// 通常ボタンイベント登録処理
        /// </summary>
        /// <param name="buttonEvent">対象通常ボタンイベント</param>
        public void RegisterNormalButton(NormalButtonEvent buttonEvent)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // 共通イベント
            RegisterCommonButton(buttonEvent);

            // クリック
            buttonEvent.OnNormalClick
                .Subscribe(clickType => _onClick.OnNext(new UIClickEvent(clickType, buttonEvent)))
                .AddTo(_disposable);
        }

        /// <summary>
        /// オプションボタンイベント登録処理
        /// </summary>
        /// <param name="buttonEvents">対象オプションボタンイベント配列</param>
        public void RegisterOptionButtons(OptionButtonEvent[] buttonEvents)
        {
            if (buttonEvents == null)
            {
                return;
            }

            foreach (OptionButtonEvent buttonEvent in buttonEvents)
            {
                if (buttonEvent == null)
                {
                    continue;
                }

                // 共通イベント
                RegisterCommonButton(buttonEvent);

                // クリック
                buttonEvent.OnOptionClick
                    .Subscribe(clickType => _onClick.OnNext(new UIClickEvent(clickType, buttonEvent)))
                    .AddTo(_disposable);
            }
        }

        // --------------------------------------------------
        // パネル
        // --------------------------------------------------
        /// <summary>
        /// パネルイベント登録処理
        /// </summary>
        /// <param name="panelEvent">対象パネルイベント</param>
        public void RegisterPanelEvent(BasePanelEvent panelEvent)
        {
            if (panelEvent == null)
            {
                return;
            }

            // クリック通知
            panelEvent.OnClick
                .Subscribe(clickType => _onClick.OnNext(new UIClickEvent(clickType, panelEvent)))
                .AddTo(_disposable);

            // ホバー開始
            panelEvent.OnHoverEnter
                .Subscribe(_ => _onHover.OnNext(panelEvent))
                .AddTo(_disposable);

            // ホバー終了
            panelEvent.OnHoverExit
                .Subscribe(_ => _onUnHover.OnNext(panelEvent))
                .AddTo(_disposable);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// ボタンの共通登録処理
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void RegisterCommonButton(BaseButtonEvent buttonEvent)
        {
            // ホバー開始
            buttonEvent.OnHoverEnter
                .Subscribe(_ => _onHover.OnNext(buttonEvent))
                .AddTo(_disposable);

            // ホバー終了
            buttonEvent.OnHoverExit
                .Subscribe(_ => _onUnHover.OnNext(buttonEvent))
                .AddTo(_disposable);

            // フォーカス開始
            buttonEvent.OnSelectEnter
                .Subscribe(_ => _onFocus.OnNext(buttonEvent))
                .AddTo(_disposable);

            // フォーカス終了
            buttonEvent.OnSelectExit
                .Subscribe(_ => _onUnFocus.OnNext(buttonEvent))
                .AddTo(_disposable);
        }
    }
}