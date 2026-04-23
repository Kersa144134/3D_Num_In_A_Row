// ======================================================
// GameEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-06
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using BoardSystem.Domain;
using BoardSystem.Presentation;
using CameraSystem.Presentation;
using InputSystem;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using System;
using System.Linq;
using UISystem.Presentation;
using UniRx;
using UpdateSystem.Domain;

namespace GameSystem.Application
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed class GameEventRouter
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ遷移管理マシン</summary>
        private PhaseMachine _phaseMachine;

        /// <summary>InputManager キャッシュ</summary>
        private readonly InputManager _inputManager;

        /// <summary>SceneObjectContainer キャッシュ配列</summary>
        private readonly BoardPresenter[] _boardPresenters;

        /// <summary>CameraPresenter キャッシュ</summary>
        private readonly CameraPresenter _cameraPresenter;

        /// <summary>MainUIPresenter キャッシュ</summary>
        private readonly MainUIPresenter _mainUIPresenter;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>直前のアクティブ状態フェーズキャッシュ</summary>
        private PhaseType _cachedActivePhase;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>フェーズ変更通知用 Subject</summary>
        private readonly Subject<PhaseChangeEvent> _onPhaseChanged = new Subject<PhaseChangeEvent>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseChangeEvent> OnPhaseChanged => _onPhaseChanged;

        /// <summary>プレイヤー変更用 Subject</summary>
        private readonly Subject<int> _onPayerChanged = new Subject<int>();

        /// <summary>入力マッピング変更用 Subject</summary>
        private readonly Subject<int> _onMappingChanged = new Subject<int>();

        /// <summary>駒配置入力用 Subject</summary>
        private readonly Subject<Unit> _onDropRequested =
            new Subject<Unit>();

        /// <summary>回転入力用 Subject</summary>
        private readonly Subject<RotationCommand> _onRotateRequested =
            new Subject<RotationCommand>();

        /// <summary>ライン成立通知用 Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete =
            new Subject<LineCompleteEvent>();

        /// <summary>現在フェーズストリーム参照</summary>
        private readonly IReadOnlyReactiveProperty<PhaseType> _currentPhase;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public GameEventRouter(
            in IUpdatableReader updatableReader,
            in IReadOnlyReactiveProperty<PhaseType> currentPhase)
        {
            _currentPhase = currentPhase;

            // Context からコンポーネント取得
            _boardPresenters = updatableReader
                .GetAll(UpdatableType.BoardPresenter)
                .Cast<BoardPresenter>()
                .ToArray();

            _cameraPresenter = (CameraPresenter)
                updatableReader.Get(UpdatableType.CameraPresenter);

            _mainUIPresenter = (MainUIPresenter)
                updatableReader.Get(UpdatableType.MainUIPresenter);

            // インスタンスからコンポーネント取得
            _inputManager = InputManager.Instance;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベント購読
        /// </summary>
        public void Subscribe(in PhaseMachine phaseMachine)
        {
            _phaseMachine = phaseMachine;

            // --------------------------------------------------
            // フェーズ
            // --------------------------------------------------
            _currentPhase
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(phase =>
                {
                    HandlePhaseChanged(phase);
                })
                .AddTo(_disposables);

            PlayPhaseState playState = _phaseMachine.GetPlayState();

            if (playState != null)
            {
                playState.CurrentPlayerIndex
                    .DistinctUntilChanged()
                    .Subscribe(player =>
                    {
                        _onPayerChanged.OnNext(player);
                    })
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            _inputManager.BindMappingStream(_onMappingChanged);

            // ボタン A 押下
            _inputManager.ButtonA.OnUp
                .Subscribe(_ =>
                {
                    // 駒配置イベント発火
                    _onDropRequested.OnNext(Unit.Default);
                })
                .AddTo(_disposables);

            // ボタン X 押下
            _inputManager.ButtonX.OnDown
                .Subscribe(_ =>
                {
                    // 回転イベント発火（X+）
                    _onRotateRequested.OnNext(
                        new RotationCommand(
                            RotationAxis.X,
                            RotationDirection.Positive
                        )
                    );
                })
                .AddTo(_disposables);

            // ボタン Y 押下
            _inputManager.ButtonY.OnDown
                .Subscribe(_ =>
                {
                    // 回転イベント発火（X-）
                    _onRotateRequested.OnNext(
                        new RotationCommand(
                            RotationAxis.X,
                            RotationDirection.Negative
                        )
                    );
                })
                .AddTo(_disposables);

            // スタートボタン押下
            _inputManager.StartButton.OnDown
                .Subscribe(e => HandleStartButtonPressed(new StartButtonEvent(_currentPhase.Value)))
                .AddTo(_disposables);

            // --------------------------------------------------
            // ボード
            // --------------------------------------------------
            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.BindPhaseStream(_currentPhase);
                boardPresenter.BindPlayerChangeStream(_onPayerChanged);
                boardPresenter.BindInputStream(_onDropRequested, _onRotateRequested);

                boardPresenter.OnInputReceived
                    .Subscribe(_ => SetTargetPhase(_currentPhase.Value))
                    .AddTo(_disposables);

                boardPresenter.OnLineComplete
                    .Subscribe(e => _onLineComplete.OnNext(e))
                    .AddTo(_disposables);

                boardPresenter.OnPlayerEnd
                    .Subscribe(_ => SetTargetPhase(_currentPhase.Value))
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // カメラ
            // --------------------------------------------------
            _cameraPresenter.BindPhaseStream(_currentPhase);

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            _phaseMachine.LimitTime
                .Subscribe(e =>
                {
                    _mainUIPresenter.UpdateLimitTimeDisplay(e);
                })
                .AddTo(_disposables);

            _mainUIPresenter.OnSwitchProjection
                .Subscribe(e =>
                {
                    _cameraPresenter.SwitchProjection(e);
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// イベント解除
        /// </summary>
        public void Dispose()
        {
            // 購読解除
            _disposables.Dispose();

            _inputManager.UnbindMappingStream();

            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.UnbindPhaseStream();
                boardPresenter.UnbindPlayerChangeStream();
                boardPresenter.UnbindInputStream();
            }

            _cameraPresenter.UnbindPhaseStream();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>
        /// フェーズ変更を通知する
        /// </summary>
        private void NotifyPhaseChanged(in PhaseType nextPhase)
        {
            _onPhaseChanged.OnNext(
                new PhaseChangeEvent(
                    _currentPhase.Value,
                    nextPhase
                )
            );
        }

        /// <summary>
        /// フェーズ変更時の処理
        /// </summary>
        /// <param name="phase">変更後のフェーズ</param>
        private void HandlePhaseChanged(in PhaseType phase)
        {
            if (phase == PhaseType.Play)
            {
                _mainUIPresenter.SetLimitTimeVisible(true);
                _mainUIPresenter.SetPointerVisible(true);

                PlayPhaseState playState = _phaseMachine.GetPlayState();
                int playerID = playState.CurrentPlayerIndex.Value;
            }
            else
            {
                _mainUIPresenter.SetLimitTimeVisible(false);
                _mainUIPresenter.SetPointerVisible(false);
            }

            if (phase == PhaseType.Pause)
            {
                _mainUIPresenter.SetPauseState(true);
            }
            else
            {
                _mainUIPresenter.SetPauseState(false);
            }

            if (phase == PhaseType.Ready)
            {
                _mainUIPresenter.SetReadyState(true);
            }
            else
            {
                _mainUIPresenter.SetReadyState(false);
            }
        }

        /// <summary>
        /// フェーズを変更する
        /// </summary>
        private void SetTargetPhase(in PhaseType currentPhase)
        {
            PhaseType nextPhase;

            if (currentPhase == PhaseType.Play)
            {
                nextPhase = PhaseType.Event;
            }
            else if (currentPhase == PhaseType.Event)
            {
                nextPhase = PhaseType.ChangePlayer;
            }
            else
            {
                return;
            }

            // フェーズ変更通知
            _onPhaseChanged.OnNext(
                new PhaseChangeEvent(
                    _currentPhase.Value,
                    nextPhase
                )
            );
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// 入力マッピング変更を通知する
        /// </summary>
        private void NotifyMappingChanged(in int mappingIndex)
        {
            _onMappingChanged.OnNext(mappingIndex);
        }

        /// <summary>
        /// スタートボタン押下時の処理を行う
        /// </summary>
        private void HandleStartButtonPressed(in StartButtonEvent e)
        {
            // マッピングと遷移先を決定する
            int mappingIndex;
            PhaseType nextPhase;

            if (e.Phase == PhaseType.Play)
            {
                mappingIndex = 1;
                nextPhase = PhaseType.Pause;

                // 現在のアクティブ状態フェーズをキャッシュ
                _cachedActivePhase = e.Phase;
            }
            else if (e.Phase == PhaseType.Pause)
            {
                mappingIndex = 0;

                // キャッシュしていたアクティブ状態フェーズへ復帰
                nextPhase = _cachedActivePhase;
            }
            else
            {
                return;
            }

            NotifyMappingChanged(mappingIndex);
            NotifyPhaseChanged(nextPhase);
        }
    }
}