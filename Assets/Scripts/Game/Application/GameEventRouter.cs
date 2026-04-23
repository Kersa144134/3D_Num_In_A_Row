// ======================================================
// GameEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-06
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using System;
using System.Linq;
using UniRx;
using BoardSystem.Domain;
using BoardSystem.Presentation;
using CameraSystem.Presentation;
using InputSystem;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using UISystem.Presentation;
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

        /// <summary>入力用購読管理</summary>
        private CompositeDisposable _inputDisposables;

        /// <summary>フェーズ変更通知用 Subject</summary>
        private readonly Subject<PhaseChangeEvent> _onPhaseChanged = new Subject<PhaseChangeEvent>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseChangeEvent> OnPhaseChanged => _onPhaseChanged;

        /// <summary>プレイヤー変更用 Subject</summary>
        private readonly Subject<int> _onPlayerChanged = new Subject<int>();

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

            _phaseMachine.CurrentPlayerIndex
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(player =>
                {
                    _onPlayerChanged.OnNext(player);
                })
                .AddTo(_disposables);

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            _inputManager.BindMappingStream(_onMappingChanged);

            // --------------------------------------------------
            // ボード
            // --------------------------------------------------
            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.BindPlayerChangeStream(_onPlayerChanged);
                boardPresenter.BindInputStream(_onDropRequested, _onRotateRequested);

                boardPresenter.OnInputReceived
                    .Subscribe(_ => NotifyPhaseChanged(PhaseType.Event))
                    .AddTo(_disposables);

                boardPresenter.OnLineComplete
                    .Subscribe(e => _onLineComplete.OnNext(e))
                    .AddTo(_disposables);

                boardPresenter.OnPlayerEnd
                    .Subscribe(_ => NotifyPhaseChanged(PhaseType.ChangePlayer))
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // カメラ
            // --------------------------------------------------
            _cameraPresenter.BindInputLockStream(
                _currentPhase.Select(phase => phase != PhaseType.Play)
            );

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            _mainUIPresenter.BindPhaseStream(_currentPhase);

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

            _onRotateRequested
                .Subscribe(e =>
                {
                    _mainUIPresenter.SetSwitchProjection(true);
                })
                .AddTo(_disposables);

            _onPlayerChanged
                .Subscribe(e =>
                {
                    _mainUIPresenter.SetChangePlayerState(e);
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

                boardPresenter.UnbindPlayerChangeStream();
                boardPresenter.UnbindInputStream();
            }

            _cameraPresenter.UnbindInputLockStream();

            _mainUIPresenter.UnbindPhaseStream();
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
            // --------------------------------------------------
            // Play
            // --------------------------------------------------
            if (phase == PhaseType.Play)
            {
                foreach (BoardPresenter boardPresenter in _boardPresenters)
                {
                    if (boardPresenter == null)
                    {
                        continue;
                    }

                    boardPresenter.BindInputStream(_onDropRequested, _onRotateRequested);
                }

                BindPlayPhaseInputCommands();
            }
            else
            {
                foreach (BoardPresenter boardPresenter in _boardPresenters)
                {
                    if (boardPresenter == null)
                    {
                        continue;
                    }

                    boardPresenter.UnbindInputStream();
                }

                UnbindPlayPhaseInputCommands();
            }
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
        /// Playフェーズ用の入力コマンド購読を登録する
        /// </summary>
        private void BindPlayPhaseInputCommands()
        {
            // 既存購読を破棄
            _inputDisposables?.Dispose();

            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            // ボタン A 押下
            _inputManager.ButtonA.OnUp
                .Subscribe(_ =>
                {
                    // 駒配置イベント発火
                    _onDropRequested.OnNext(Unit.Default);
                })
                .AddTo(_inputDisposables);

            // ボタン B 押下
            _inputManager.ButtonB.OnDown
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
                .AddTo(_inputDisposables);

            // スタートボタン押下
            _inputManager.StartButton.OnDown
                .Subscribe(e => HandleStartButtonPressed(_currentPhase.Value))
                .AddTo(_inputDisposables);
        }

        /// <summary>
        /// Playフェーズ用の入力コマンド購読を解除する
        /// </summary>
        private void UnbindPlayPhaseInputCommands()
        {
            _inputDisposables?.Dispose();
            _inputDisposables = null;
        }

        /// <summary>
        /// スタートボタン押下時の処理を行う
        /// </summary>
        private void HandleStartButtonPressed(in PhaseType phase)
        {
            // マッピングと遷移先を決定する
            int mappingIndex;
            PhaseType nextPhase;

            if (phase == PhaseType.Play)
            {
                mappingIndex = 1;
                nextPhase = PhaseType.Pause;

                // 現在のアクティブ状態フェーズをキャッシュ
                _cachedActivePhase = phase;
            }
            else if (phase == PhaseType.Pause)
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