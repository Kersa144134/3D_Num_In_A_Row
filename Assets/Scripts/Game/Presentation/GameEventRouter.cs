// ======================================================
// GameEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
// ======================================================

using BoardSystem.Domain;
using BoardSystem.Presentation;
using CameraSystem.Presentation;
using InputSystem.Domain;
using InputSystem.Presentation;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using ScoreSystem.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using UISystem.Presentation;
using UniRx;
using UpdateSystem.Domain;

namespace GameSystem.Presentation
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

        /// <summary>ScoreManager キャッシュ</summary>
        private readonly ScoreManager _scoreManager;

        /// <summary>SceneObjectContainer キャッシュ配列</summary>
        private readonly BoardPresenter[] _boardPresenters;

        /// <summary>CameraPresenter キャッシュ</summary>
        private readonly CameraPresenter _cameraPresenter;

        /// <summary>TitleUIPresenter キャッシュ</summary>
        private readonly TitleUIPresenter _titleUIPresenter;

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

        private readonly Subject<ScoreEvent> _onScoreUpdated = new Subject<ScoreEvent>();

        /// <summary>入力マッピング変更用 Subject</summary>
        private readonly Subject<int> _onMappingChanged = new Subject<int>();

        /// <summary>ゲームパッド検知用 Subject</summary>
        private readonly Subject<bool> _onGamepadUsed = new Subject<bool>();

        /// <summary>プレイヤー変更用 Subject</summary>
        private readonly Subject<int> _onPlayerChanged = new Subject<int>();

        /// <summary>駒配置入力用 Subject</summary>
        private readonly Subject<Unit> _onDropRequested = new Subject<Unit>();

        /// <summary>回転入力用 Subject</summary>
        private readonly Subject<Unit> _onRotateRequested = new Subject<Unit>();

        /// <summary>回転実行用 Subject</summary>
        private readonly Subject<RotationCommand> _onRotateExecuted = new Subject<RotationCommand>();

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

            _titleUIPresenter = (TitleUIPresenter)
                updatableReader.Get(UpdatableType.TitleUIPresenter);

            _mainUIPresenter = (MainUIPresenter)
                updatableReader.Get(UpdatableType.MainUIPresenter);

            // インスタンスからコンポーネント取得
            _inputManager = InputManager.Instance;
            _scoreManager = ScoreManager.Instance;
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
            // ルーター
            // --------------------------------------------------
            _onRotateRequested
                .Subscribe(_ => NotifyPhaseChanged(PhaseType.Event))
                .AddTo(_disposables);

            // --------------------------------------------------
            // フェーズ
            // --------------------------------------------------
            _currentPhase
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(phase =>
                {
                    HandlePhaseInputSwitch(phase);
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

            // スタートボタン押下
            _inputManager.StartButton.OnDown
                .Subscribe(e => HandleStartButtonPressed(_currentPhase.Value))
                .AddTo(_disposables);

            // アクティブコントローラー変更
            _inputManager.ActiveDeviceType
                .Subscribe(e => HandleActiveControllerChanged(e))
                .AddTo(_disposables);

            // --------------------------------------------------
            // ボード
            // --------------------------------------------------
            if (_boardPresenters != null)
            {
                foreach (BoardPresenter boardPresenter in _boardPresenters)
                {
                    if (boardPresenter == null)
                    {
                        continue;
                    }

                    boardPresenter.BindPlayerChangeStream(_onPlayerChanged);

                    boardPresenter.OnInputReceived
                        .Subscribe(_ => NotifyPhaseChanged(PhaseType.Event))
                        .AddTo(_disposables);

                    boardPresenter.OnLineComplete
                        .Subscribe(e => HandleLineCompleted(e))
                        .AddTo(_disposables);

                    boardPresenter.OnPlayerEnd
                        .Subscribe(_ => NotifyPhaseChanged(PhaseType.ChangePlayer))
                        .AddTo(_disposables);
                }
            }

            // --------------------------------------------------
            // カメラ
            // --------------------------------------------------
            if (_cameraPresenter != null)
            {
                _cameraPresenter.BindInputLockStream(
                    _currentPhase.Select(phase => phase != PhaseType.Play)
                );
                _cameraPresenter.BindBoardRotationPreparationStream(
                    _mainUIPresenter.OnSwitchProjection.Select(_ => Unit.Default)
                );
            }

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            if (_titleUIPresenter != null)
            {
                _titleUIPresenter.BindPhaseStream(_currentPhase);
                _titleUIPresenter.BindInputLockStream(
                    _currentPhase.Select(phase => phase != PhaseType.Title)
                );
                // ポインター表示条件に合わせて反転して渡す
                _titleUIPresenter.BindPointerVisibleStream(
                    _onGamepadUsed.Select(x => !x)
                );
            }
            if (_mainUIPresenter != null)
            {
                _mainUIPresenter.BindPhaseStream(_currentPhase);
                _mainUIPresenter.BindLineCompleteStream(_onScoreUpdated);
                _mainUIPresenter.BindLimitTimeStream(_phaseMachine.LimitTime);
                _mainUIPresenter.BindInputLockStream(
                    _currentPhase.Select(phase => phase != PhaseType.Play)
                );
                _mainUIPresenter.BindPlayerChangeStream(_onPlayerChanged);
                _mainUIPresenter.BindRotateStream(_onRotateRequested);

                _mainUIPresenter.OnSwitchProjection
                    .Subscribe(e =>
                    {
                        _cameraPresenter.SwitchProjection(e);
                    })
                    .AddTo(_disposables);
            }
        }

        /// <summary>
        /// イベント購読解除
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

            if (_cameraPresenter != null)
            {
                _cameraPresenter.UnbindInputLockStream();
                _cameraPresenter.UnbindBoardRotationPreparationStream();
            }

            if (_titleUIPresenter != null)
            {
                _titleUIPresenter.UnbindPhaseStream();
                _titleUIPresenter.UnbindInputLockStream();
                _titleUIPresenter.UnbindPointerVisibleStream();
            }
            if (_mainUIPresenter != null)
            {
                _mainUIPresenter.UnbindPhaseStream();
                _mainUIPresenter.UnbindLineCompleteStream();
                _mainUIPresenter.UnbindLimitTimeStream();
                _mainUIPresenter.UnbindInputLockStream();
                _mainUIPresenter.UnbindPlayerChangeStream();
                _mainUIPresenter.UnbindRotateStream();
            }
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
        /// デバイス切替処理を行う
        /// </summary>
        private void HandleActiveControllerChanged(in InputDeviceType device)
        {
            if (device == InputDeviceType.Gamepad)
            {
                _onGamepadUsed.OnNext( true );
            }
            else
            {
                _onGamepadUsed.OnNext(false);
            }
        }

        /// <summary>
        /// フェーズ変更時の入力切替処理を行う
        /// </summary>
        /// <param name="phase">変更後のフェーズ</param>
        private void HandlePhaseInputSwitch(in PhaseType phase)
        {
            // 入力購読解除
            UnbindInputCommands();

            // --------------------------------------------------
            // Play
            // --------------------------------------------------
            if (phase == PhaseType.Play)
            {
                BindPlayPhaseInputCommands();

                foreach (BoardPresenter boardPresenter in _boardPresenters)
                {
                    if (boardPresenter == null)
                    {
                        continue;
                    }

                    boardPresenter.BindDropInputStream(_onDropRequested);
                }

                return;
            }

            // --------------------------------------------------
            // Event
            // --------------------------------------------------
            if (phase == PhaseType.Event)
            {
                BindEventPhaseInputCommands();

                foreach (BoardPresenter boardPresenter in _boardPresenters)
                {
                    if (boardPresenter == null)
                    {
                        continue;
                    }

                    boardPresenter.BindRotateInputStream(_onRotateExecuted);
                }

                return;
            }

            // --------------------------------------------------
            // その他
            // --------------------------------------------------
            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.UnbindInputStream();
            }
        }

        /// <summary>
        /// Play フェーズ用の入力コマンド購読を登録する
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
                    // 回転準備イベント発火
                    _onRotateRequested.OnNext(Unit.Default);
                })
                .AddTo(_inputDisposables);

            _onRotateExecuted.OnNext(
                new RotationCommand(
                    RotationAxis.X,
                    RotationDirection.Positive
                )
            );
        }

        /// <summary>
        /// Event フェーズ用の入力コマンド購読を登録する
        /// </summary>
        private void BindEventPhaseInputCommands()
        {
            // 既存購読を破棄
            _inputDisposables?.Dispose();

            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            // 左スティック左押下
            _inputManager.ButtonX.OnDown
                .Subscribe(_ =>
                {
                    // Z- 回転実行イベント発火
                    _onRotateExecuted.OnNext(
                        new RotationCommand(
                            RotationAxis.Z,
                            RotationDirection.Negative
                        )
                    );
                })
                .AddTo(_inputDisposables);

            // 左スティック右押下
            _inputManager.ButtonB.OnDown
                .Subscribe(_ =>
                {
                    // Z+ 回転実行イベント発火
                    _onRotateExecuted.OnNext(
                        new RotationCommand(
                            RotationAxis.Z,
                            RotationDirection.Positive
                        )
                    );
                })
                .AddTo(_inputDisposables);

            // 左スティック上押下
            _inputManager.ButtonY.OnDown
                .Subscribe(_ =>
                {
                    // Z- 回転実行イベント発火
                    _onRotateExecuted.OnNext(
                        new RotationCommand(
                            RotationAxis.X,
                            RotationDirection.Negative
                        )
                    );
                })
                .AddTo(_inputDisposables);

            // 左スティック下押下
            _inputManager.ButtonA.OnDown
                .Subscribe(_ =>
                {
                    // Z+ 回転実行イベント発火
                    _onRotateExecuted.OnNext(
                        new RotationCommand(
                            RotationAxis.X,
                            RotationDirection.Positive
                        )
                    );
                })
                .AddTo(_inputDisposables);
        }

        /// <summary>
        /// 入力コマンド購読を解除する
        /// </summary>
        private void UnbindInputCommands()
        {
            _inputDisposables?.Dispose();
            _inputDisposables = null;
        }

        /// <summary>
        /// スタートボタン押下時のマッピング変更処理を行う
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

        // --------------------------------------------------
        // ボード
        // --------------------------------------------------
        /// <summary>
        /// ライン成立時の処理を行う
        /// </summary>
        private void HandleLineCompleted(LineCompleteEvent e)
        {
            // プレイヤー ID
            int playerId = e.Player;

            // ラインリスト
            IReadOnlyList<BoardIndex>[] lines = e.LinePositions;

            // ラインの長さ
            int lineLength = 0;

            // ラインごとのセル数を合計する
            for (int i = 0; i < lines.Length; i++)
            {
                // 各ラインのセル数を加算
                lineLength += lines[i]?.Count ?? 0;
            }

            // スコアイベントとして発火
            _onScoreUpdated.OnNext(new ScoreEvent(playerId, lineLength));
        }
    }
}