// ======================================================
// UIEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-09
// 更新日時 : 2026-05-09
// 概要     : UI のイベント通知を管理する
// ======================================================

using System;
using UniRx;
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
        private readonly Subject<NormalButtonEvent> _onNormalButtonClick = new Subject<NormalButtonEvent>();

        /// <summary>オプションボタンクリック通知</summary>
        private readonly Subject<OptionButtonEvent> _onOptionButtonClick = new Subject<OptionButtonEvent>();

        /// <summary>ホバー通知</summary>
        private readonly Subject<BaseButtonEvent> _onHover = new Subject<BaseButtonEvent>();

        /// <summary>ホバー解除通知</summary>
        private readonly Subject<BaseButtonEvent> _onUnHover = new Subject<BaseButtonEvent>();

        /// <summary>フォーカス通知</summary>
        private readonly Subject<BaseButtonEvent> _onFocus = new Subject<BaseButtonEvent>();

        /// <summary>フォーカス解除通知</summary>
        private readonly Subject<BaseButtonEvent> _onUnFocus = new Subject<BaseButtonEvent>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>通常ボタンクリック通知</summary>
        public IObservable<NormalButtonEvent> OnNormalButtonClick => _onNormalButtonClick;

        /// <summary>オプションボタンクリック通知</summary>
        public IObservable<OptionButtonEvent> OnOptionButtonClick => _onOptionButtonClick;

        /// <summary>ホバー通知</summary>
        public IObservable<BaseButtonEvent> OnHover => _onHover;

        /// <summary>ホバー解除通知</summary>
        public IObservable<BaseButtonEvent> OnUnHover => _onUnHover;

        /// <summary>フォーカス通知</summary>
        public IObservable<BaseButtonEvent> OnFocus => _onFocus;

        /// <summary>フォーカス解除通知</summary>
        public IObservable<BaseButtonEvent> OnUnFocus => _onUnFocus;

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
            buttonEvent.OnNormalClickAsObservable
                .Subscribe(_ => _onNormalButtonClick.OnNext(buttonEvent))
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
                buttonEvent.OnOptionClickAsObservable
                    .Subscribe(_ => _onOptionButtonClick.OnNext(buttonEvent))
                    .AddTo(_disposable);
            }
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
            buttonEvent.OnHoverEnterAsObservable
                .Subscribe(_ => _onHover.OnNext(buttonEvent))
                .AddTo(_disposable);

            // ホバー終了
            buttonEvent.OnHoverExitAsObservable
                .Subscribe(_ => _onUnHover.OnNext(buttonEvent))
                .AddTo(_disposable);

            // フォーカス開始
            buttonEvent.OnSelectEnterAsObservable
                .Subscribe(_ => _onFocus.OnNext(buttonEvent))
                .AddTo(_disposable);

            // フォーカス終了
            buttonEvent.OnSelectExitAsObservable
                .Subscribe(_ => _onUnFocus.OnNext(buttonEvent))
                .AddTo(_disposable);
        }
    }
}