// ======================================================
// PausePhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : 一時停止フェーズの振る舞い
// ======================================================

using System;
using UniRx;
using InputSystem;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// Pauseフェーズの処理
    /// </summary>
    public sealed class PausePhaseState : IPhaseState
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズ経過時間</summary>
        private float _elapsedTime = 0.0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>フェーズ経過時間</summary>
        public float ElapsedTime => _elapsedTime;

        /// <summary>スタートボタン押下ストリーム</summary>
        public IObservable<Unit> OnStartButtonPressed => _onStartButtonPressed;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private CompositeDisposable _disposables;

        /// <summary>スタートボタン押下通知用 Subject</summary>
        private readonly Subject<Unit> _onStartButtonPressed = new Subject<Unit>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ開始時処理
        /// </summary>
        public void OnEnter()
        {
            _disposables = new CompositeDisposable();

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            // スタートボタン押下時
            InputManager.Instance.StartButton.OnDown
                .Subscribe(_ => PublishStartButtonPressed())
                .AddTo(_disposables);
        }

        /// <summary>
        /// フェーズ終了時処理
        /// </summary>
        public void OnExit()
        {
            // イベント購読解除
            _disposables?.Dispose();
        }

        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        public void OnUpdate(in float unscaledDeltaTime)
        {
            _elapsedTime += unscaledDeltaTime;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// スタートボタン押下イベントを発火する
        /// </summary>
        private void PublishStartButtonPressed()
        {
            _onStartButtonPressed?.OnNext(Unit.Default);
        }
    }
}