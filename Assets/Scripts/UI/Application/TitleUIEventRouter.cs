// ======================================================
// UIEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-09
// 更新日時 : 2026-05-09
// 概要     : UI のイベント通知を管理する
// ======================================================

using System;
using UniRx;
using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// UI のイベント通知を管理する
    /// </summary>
    public sealed class UIEventRouter : IDisposable
    {
        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        /// <summary>通常ボタンクリック通知</summary>
        private readonly Subject<(UIClickType, NormalButtonEvent)> _onNormalButtonClick
            = new Subject<(UIClickType, NormalButtonEvent)>();

        /// <summary>オプションボタンクリック通知</summary>
        private readonly Subject<(UIClickType, OptionButtonEvent)> _onOptionButtonClick
            = new Subject<(UIClickType, OptionButtonEvent)>();

        /// <summary>パネルクリック通知</summary>
        private readonly Subject<(UIClickType, BasePanelEvent)> _onPanelClick
            = new Subject<(UIClickType, BasePanelEvent)>();

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

        /// <summary>通常ボタンクリック通知</summary>
        public IObservable<(UIClickType, NormalButtonEvent)> OnNormalButtonClick => _onNormalButtonClick;

        /// <summary>オプションボタンクリック通知</summary>
        public IObservable<(UIClickType, OptionButtonEvent)> OnOptionButtonClick => _onOptionButtonClick;

        /// <summary>パネルクリック通知</summary>
        public IObservable<(UIClickType, BasePanelEvent)> OnPanelClick => _onPanelClick;

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

        /// <summary>
        /// NormalButton 登録処理
        /// </summary>
        /// <param name="buttonEvent">対象 NormalButton イベント</param>
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
                .Subscribe(type => _onNormalButtonClick.OnNext((type, buttonEvent)))
                .AddTo(_disposable);
        }

        /// <summary>
        /// OptionButton 登録処理
        /// </summary>
        /// <param name="buttonEvents">対象 OptionButton イベント配列</param>
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
                    .Subscribe(type => _onOptionButtonClick.OnNext((type, buttonEvent)))
                    .AddTo(_disposable);
            }
        }

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
                .Subscribe(type => _onPanelClick.OnNext((type, panelEvent)))
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

        /// <summary>
        /// イベント購読を解除する
        /// </summary>
        public void Dispose()
        {
            _disposable?.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

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