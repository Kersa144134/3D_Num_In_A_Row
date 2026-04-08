// ======================================================
// PhasePresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-24
// 更新日時 : 2026-03-24
// 概要     : PhaseModel を操作してフェーズ遷移・更新を管理する Presenter
// ======================================================

using System;
using UniRx;
using PhaseSystem.Data;

namespace PhaseSystem
{
    /// <summary>
    /// フェーズ進行管理用 Presenter
    /// </summary>
    public sealed class PhasePresenter
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ進行管理用 Model</summary>
        private readonly PhaseModel _model = new();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Play フェーズ配列</summary>
        private readonly PhaseType[] _playPhases;

        /// <summary>Play フェーズから Finish フェーズへ遷移するまでの時間（秒）</summary>
        private readonly float _playToFinishWaitTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ゲームプレイ経過時間の取得</summary>
        public float GamePlayElapsedTime => _model.GamePlayElapsedTime;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private CompositeDisposable _disposables;

        /// <summary>スタートボタン押下用 Subject</summary>
        private readonly Subject<StartButtonEvent> _onStartButtonPressed = new Subject<StartButtonEvent>();

        /// <summary>スタートボタン押下ストリーム</summary>
        public IObservable<StartButtonEvent> OnStartButtonPressed => _onStartButtonPressed;

        /// <summary>残り時間更新用 Subject<</summary>
        private readonly Subject<LimitTimeEvent> _onLimitTimeUpdated = new Subject<LimitTimeEvent>();

        /// <summary>残り時間更新ストリーム</summary>
        public IObservable<LimitTimeEvent> OnLimitTimeUpdated => _onLimitTimeUpdated;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// PhasePresenter の生成
        /// </summary>
        /// <param name="model">Play フェーズから Finish フェーズへ遷移するまでの時間（秒）</param>
        public PhasePresenter(in float playToFinishWaitTime)
        {
            _playToFinishWaitTime = playToFinishWaitTime;

            // Play フェーズをキャッシュ
            _playPhases = new[]
            {
                PhaseType.Play_1,
                PhaseType.Play_2
            };
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズイベントを購読する
        /// </summary>
        public void BindPhaseEvents()
        {
            // --------------------------------------------------
            // 多重購読防止
            // --------------------------------------------------
            _disposables?.Dispose();

            _disposables = new CompositeDisposable();

            // --------------------------------------------------
            // Play フェーズ
            // --------------------------------------------------
            for (int i = 0; i < _playPhases.Length; i++)
            {
                PhaseType phase = _playPhases[i];

                PlayPhaseState play = _model.GetState(phase) as PlayPhaseState;

                if (play != null)
                {
                    play.OnStartButtonPressed
                        .Subscribe(_ =>
                        {
                            // Play フェーズのスタート押下として通知
                            _onStartButtonPressed.OnNext(
                                new StartButtonEvent(phase));
                        })
                        .AddTo(_disposables);
                }
            }

            // --------------------------------------------------
            // Pause フェーズ
            // --------------------------------------------------
            PausePhaseState pause = _model.GetState(PhaseType.Pause) as PausePhaseState;

            if (pause != null)
            {
                pause.OnStartButtonPressed
                    .Subscribe(_ =>
                    {
                        // Pause フェーズのスタート押下として通知
                        _onStartButtonPressed.OnNext(
                            new StartButtonEvent(PhaseType.Pause));
                    })
                    .AddTo(_disposables);
            }
        }

        /// <summary>
        /// フェーズイベントの購読を解除する
        /// </summary>
        public void UnbindPhaseEvents()
        {
            _disposables?.Dispose();
            _disposables = null;
        }

        /// <summary>
        /// フェーズ進行の更新処理
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale 影響なしの経過時間</param>
        /// <param name="currentPhase">現在フェーズ</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        public void Update(
            in float unscaledDeltaTime,
            in PhaseType currentPhase,
            out PhaseType targetPhase)
        {
            targetPhase = currentPhase;

            // 現フェーズのステート取得
            IPhaseState currentState = _model.GetState(currentPhase);
            if (currentState == null)
            {
                return;
            }

            // --------------------------------------------------
            // フェーズ切替判定
            // --------------------------------------------------
            if (currentPhase != _model.PreviousPhase)
            {
                // 前フェーズ終了処理
                IPhaseState prevState = _model.GetState(_model.PreviousPhase);
                prevState?.OnExit();

                // Ready フェーズ開始時にゲームプレイ時間リセット
                if (currentPhase == PhaseType.Ready)
                {
                    _model.ResetElapsedTime();
                }

                // 現フェーズ開始処理
                currentState.OnEnter();
            }

            // --------------------------------------------------
            // フェーズ更新処理
            // --------------------------------------------------
            currentState.OnUpdate(unscaledDeltaTime);

            // Play フェーズか判定
            bool isPlayPhase = false;

            for (int i = 0; i < _playPhases.Length; i++)
            {
                if (currentPhase == _playPhases[i])
                {
                    isPlayPhase = true;
                    break;
                }
            }

            if (isPlayPhase)
            {
                _model.AddElapsedTime(unscaledDeltaTime);

                _onLimitTimeUpdated.OnNext(
                    new LimitTimeEvent(
                        _model.GamePlayElapsedTime,
                        _playToFinishWaitTime));
            }

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------
            if (currentPhase != PhaseType.Finish &&
                _model.GamePlayElapsedTime > _playToFinishWaitTime)
            {
                targetPhase = PhaseType.Finish;
            }

            // --------------------------------------------------
            // 前フレームフェーズ更新
            // --------------------------------------------------
            _model.PreviousPhase = currentPhase;
        }
    }
}