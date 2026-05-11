// ======================================================
// GameEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
// ======================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using BoardSystem.Domain;
using BoardSystem.Presentation;
using CameraSystem.Presentation;
using InputSystem.Domain;
using InputSystem.Presentation;
using OptionSystem.Domain;
using OptionSystem.Presentation;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using ScoreSystem.Presentation;
using UISystem.Presentation;
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

        /// <summary>GameOptionManager キャッシュ</summary>
        private readonly GameOptionManager _gameOptionManager;

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
        private PhaseType _cachedActivePhase = PhaseType.None;

        /// <summary>現在の入力マッピング番号</summary>
        private int _currentMappingIndex = -1;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>入力用購読管理</summary>
        private CompositeDisposable _inputDisposables;

        /// <summary>シーン遷移予約通知用 Subject</summary>
        private readonly Subject<string> _onSceneChangeRequested = new Subject<string>();

        /// <summary>シーン遷移予約ストリーム</summary>
        public IObservable<string> OnSceneChangeRequested => _onSceneChangeRequested;

        /// <summary>シーン遷移実行通知用 Subject</summary>
        private readonly Subject<Unit> _onSceneChangeExecuted = new Subject<Unit>();

        /// <summary>シーン遷移実行ストリーム</summary>
        public IObservable<Unit> OnSceneChangeExecuted => _onSceneChangeExecuted;

        /// <summary>フェーズ変更通知用 Subject</summary>
        private readonly Subject<PhaseChangeEvent> _onPhaseChanged = new Subject<PhaseChangeEvent>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseChangeEvent> OnPhaseChanged => _onPhaseChanged;

        /// <summary>スコア更新用 Subject</summary>
        private readonly Subject<ScoreEvent> _onScoreUpdated = new Subject<ScoreEvent>();

        /// <summary>入力マッピング変更用 Subject</summary>
        private readonly Subject<int> _onMappingChanged = new Subject<int>();

        /// <summary>ポインター座標変更用 Subject</summary>
        private readonly Subject<Vector2> _onPointerPositionChanged = new Subject<Vector2>();

        /// <summary>ゲームパッド検知用 Subject</summary>
        private readonly Subject<bool> _onGamepadUsed = new Subject<bool>();

        /// <summary>画面フェードイン開始通知用 Subject</summary>
        private readonly Subject<float> _fadeInTrigger = new Subject<float>();

        /// <summary>画面フェードアウト開始通知用 Subject</summary>
        private readonly Subject<float> _fadeOutTrigger = new Subject<float>();

        /// <summary>画面フェード完了通知用 Subject</summary>
        private readonly Subject<Unit> _onFadeCompleted = new Subject<Unit>();

        /// <summary>画面フェード完了ストリーム</summary>
        public IObservable<Unit> OnFadeCompleted => _onFadeCompleted;

        /// <summary>ゲーム開始入力用 Subject</summary>
        private readonly Subject<Unit> _onGameStartRequested = new Subject<Unit>();

        /// <summary>プレイヤー変更用 Subject</summary>
        private readonly Subject<int> _onPlayerChanged = new Subject<int>();

        /// <summary>駒配置入力用 Subject</summary>
        private readonly Subject<Unit> _onDropRequested = new Subject<Unit>();

        /// <summary>回転入力用 Subject</summary>
        private readonly Subject<Unit> _onRotateRequested = new Subject<Unit>();

        /// <summary>回転実行用 Subject</summary>
        private readonly Subject<RotationCommand> _onRotateExecuted = new Subject<RotationCommand>();

        /// <summary>シーンロード用購読管理</summary>
        private IDisposable _sceneLoadSubscription;

        /// <summary>シーンロード準備開始購読</summary>
        private IDisposable _loadPrepareStartSubscription;

        /// <summary>シーンロード準備完了購読</summary>
        private IDisposable _loadPrepareEndSubscription;

        /// <summary>シーン変更用購読管理</summary>
        private IDisposable _sceneChangeSubscription;

        /// <summary>現在フェーズストリーム参照</summary>
        private readonly IReadOnlyReactiveProperty<PhaseType> _currentPhase;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>3 x 3 ボードサイズ</summary>
        private const int BOARD_SIZE_THREE = 3;

        /// <summary>5 x 5 ボードサイズ</summary>
        private const int BOARD_SIZE_FIVE = 5;

        /// <summary>タイトルシーン名</summary>
        private const string TITLE_SCENE_NAME = "TitleScene";

        /// <summary>3 x 3 ボードシーン名</summary>
        private const string THREE_SIZE_SCENE_NAME = "ThreeSizeScene";

        /// <summary>5 x 5 ボードシーン名</summary>
        private const string FIVE_SIZE_SCENE_NAME = "FiveSizeScene";

        /// <summary>リザルトシーン名</summary>
        private const string RESULT_SCENE_NAME = "ResultScene";

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
            _gameOptionManager = GameOptionManager.Instance;
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
                .Subscribe(phase => HandlePhaseInputSwitch(phase))
                .AddTo(_disposables);

            _phaseMachine.CurrentPlayerIndex
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(player => _onPlayerChanged.OnNext(player))
                .AddTo(_disposables);

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            _inputManager.BindMappingStream(_onMappingChanged);
            _inputManager.BindPointerPositionStream(_onPointerPositionChanged);

            // スタートボタン押下
            _inputManager.StartButton.OnDown
                .Subscribe(e => HandleStartButtonPressed(_currentPhase.Value))
                .AddTo(_disposables);

            // アクティブコントローラー変更
            _inputManager.ActiveDeviceType
                .Subscribe(e => NotifyActiveControllerChanged(e))
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
                _titleUIPresenter.BindInputLockStream(
                    _currentPhase.Select(phase => phase != PhaseType.Title)
                );
                _titleUIPresenter.BindGamePadInputStream(_onGamepadUsed);

                _titleUIPresenter.OnFocusPosition
                    .Subscribe(e =>
                     {
                         _onPointerPositionChanged.OnNext(e);
                     })
                    .AddTo(_disposables);

                _titleUIPresenter.OnUpdateGameOption
                    .Subscribe(e => HandleGameOptionUpdated(e))
                    .AddTo(_disposables);

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _titleUIPresenter.BindFadeInStream(_fadeInTrigger);
                _titleUIPresenter.BindFadeOutStream(_fadeOutTrigger);

                _titleUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneTChangeRequested())
                    .AddTo(_disposables);
                _titleUIPresenter.OnFadeInCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _titleUIPresenter.OnFadeOutCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
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
                    .Subscribe(e => _cameraPresenter.SwitchProjection(e))
                    .AddTo(_disposables);

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _mainUIPresenter.BindFadeInStream(_fadeInTrigger);
                _mainUIPresenter.BindFadeOutStream(_fadeOutTrigger);

                _mainUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneTChangeRequested())
                    .AddTo(_disposables);
                _mainUIPresenter.OnFadeInCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _mainUIPresenter.OnFadeOutCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
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
                boardPresenter.UnbindDropInputStream();
                boardPresenter.UnbindRotateInputStream();
            }

            if (_cameraPresenter != null)
            {
                _cameraPresenter.UnbindInputLockStream();
                _cameraPresenter.UnbindBoardRotationPreparationStream();
            }

            if (_titleUIPresenter != null)
            {
                _titleUIPresenter.UnbindInputLockStream();
                _titleUIPresenter.UnbindGamePadInputStream();

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _titleUIPresenter.UnbindFadeStream();
            }
            if (_mainUIPresenter != null)
            {
                _mainUIPresenter.UnbindPhaseStream();
                _mainUIPresenter.UnbindLineCompleteStream();
                _mainUIPresenter.UnbindLimitTimeStream();
                _mainUIPresenter.UnbindInputLockStream();
                _mainUIPresenter.UnbindPlayerChangeStream();
                _mainUIPresenter.UnbindRotateStream();

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _mainUIPresenter.UnbindFadeStream();
            }
        }

        /// <summary>
        /// シーンロード準備ストリームを購読する
        /// </summary>
        public void BindSceneLoadPrepareStream(
            IObservable<string> onPrepareStart,
            IObservable<float> onPrepareEnd)
        {
            // 多重購読防止
            _loadPrepareStartSubscription?.Dispose();
            _loadPrepareEndSubscription?.Dispose();

            _loadPrepareStartSubscription = onPrepareStart
                .Subscribe(sceneName =>
                {
                    // ロード準備開始
                    HandleLoadPrepareStart(sceneName);
                });

            _loadPrepareEndSubscription = onPrepareEnd
                .Subscribe(fadeTime =>
                {
                    // ロード準備完了
                    HandleLoadPrepareEnd(fadeTime);
                });
        }

        /// <summary>
        /// シーンロード準備ストリームの購読を解除する
        /// </summary>
        public void UnbindSceneLoadPrepareStream()
        {
            _loadPrepareStartSubscription?.Dispose();
            _loadPrepareEndSubscription?.Dispose();
            _loadPrepareStartSubscription = null;
            _loadPrepareEndSubscription = null;
        }

        /// <summary>
        /// シーン変更ストリームを購読する
        /// </summary>
        public void BindSceneChangeStream(IObservable<float> onSceneChanged)
        {
            // 多重購読防止
            _sceneChangeSubscription?.Dispose();

            _loadPrepareStartSubscription = onSceneChanged
                .Subscribe(seconds =>
                {
                    // フェードアウト時間を通知
                    _fadeOutTrigger.OnNext(seconds);
                });
        }

        /// <summary>
        /// シーン変更ストリームの購読を解除する
        /// </summary>
        public void UnbindSceneChangeStream()
        {
            _sceneChangeSubscription?.Dispose();
            _sceneChangeSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // シーン
        // --------------------------------------------------
        /// <summary>
        /// シーン遷移予約を通知する
        /// </summary>
        private void NotifySceneTChangeRequested()
        {
            switch (_currentPhase.Value)
            {
                case PhaseType.Title:
                    // 3 x 3 ボードシーンへ遷移
                    if (_gameOptionManager.BoardSize == BOARD_SIZE_THREE)
                    {
                        _onSceneChangeRequested.OnNext(THREE_SIZE_SCENE_NAME);
                        break;
                    }

                    // 5 x 5 ボードシーンへ遷移
                    if (_gameOptionManager.BoardSize == BOARD_SIZE_FIVE)
                    {
                        _onSceneChangeRequested.OnNext(FIVE_SIZE_SCENE_NAME);
                    }

                    break;

                case PhaseType.Finish:
                    // リザルトシーンへ遷移
                    _onSceneChangeRequested.OnNext(RESULT_SCENE_NAME);

                    break;

                case PhaseType.Pause:
                case PhaseType.Result:
                    // タイトルシーンへ遷移
                    _onSceneChangeRequested.OnNext(TITLE_SCENE_NAME);

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// シーンロード準備開始時の処理を行う
        /// </summary>
        /// <param name="sceneName">現在のシーン名</param>
        private void HandleLoadPrepareStart(in string sceneName)
        {
            // 多重購読防止
            _sceneLoadSubscription?.Dispose();
            
            switch (sceneName)
            {
                case TITLE_SCENE_NAME:
                    // ゲーム開始入力
                    _sceneLoadSubscription = _onGameStartRequested
                        .Subscribe(_ =>
                        {
                            // シーン遷移実行通知
                            _onSceneChangeExecuted.OnNext(Unit.Default);
                        });

                    break;

                case THREE_SIZE_SCENE_NAME:
                    break;

                case FIVE_SIZE_SCENE_NAME:
                    break;

                case RESULT_SCENE_NAME:
                    break;
                
                default:
                    break;
            }
        }

        /// <summary>
        /// シーンロード準備開始時の処理を行う
        /// </summary>
        private void HandleLoadPrepareEnd(in float fadeTime)
        {
            // 購読解除
            _sceneLoadSubscription?.Dispose();
            _sceneLoadSubscription = null;
            
            // フェードイン時間を通知
            _fadeInTrigger.OnNext(fadeTime);
        }

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
            // 現在のマッピング番号と一致している場合は処理なし
            if (_currentMappingIndex == mappingIndex)
            {
                return;
            }

            _currentMappingIndex = mappingIndex;

            _onMappingChanged.OnNext(mappingIndex);
        }

        /// <summary>
        /// デバイス変更を通知する
        /// </summary>
        private void NotifyActiveControllerChanged(in InputDeviceType device)
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

            switch (phase)
            {
                // --------------------------------------------------
                // Title
                // --------------------------------------------------
                case PhaseType.Title:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    // Title フェーズ時の入力コマンド登録
                    BindTitlePhaseInputCommands();

                    return;

                // --------------------------------------------------
                // Ready
                // --------------------------------------------------
                case PhaseType.Ready:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    return;

                // --------------------------------------------------
                // Play
                // --------------------------------------------------
                case PhaseType.Play:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // Play フェーズ時の入力コマンド登録
                    BindPlayPhaseInputCommands();

                    foreach (BoardPresenter boardPresenter in _boardPresenters)
                    {
                        if (boardPresenter == null)
                        {
                            continue;
                        }

                        // 入力ストリーム登録
                        boardPresenter.BindDropInputStream(_onDropRequested);

                        // 入力ストリーム解除
                        boardPresenter.UnbindRotateInputStream();
                    }

                    return;

                // --------------------------------------------------
                // Event
                // --------------------------------------------------
                case PhaseType.Event:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // Event フェーズ時の入力コマンド登録
                    BindEventPhaseInputCommands();

                    foreach (BoardPresenter boardPresenter in _boardPresenters)
                    {
                        if (boardPresenter == null)
                        {
                            continue;
                        }

                        // 入力ストリーム登録
                        boardPresenter.BindRotateInputStream(_onRotateExecuted);

                        // 入力ストリーム解除
                        boardPresenter.UnbindDropInputStream();
                    }

                    return;

                // --------------------------------------------------
                // ChangePlayer
                // --------------------------------------------------
                case PhaseType.ChangePlayer:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    foreach (BoardPresenter boardPresenter in _boardPresenters)
                    {
                        if (boardPresenter == null)
                        {
                            continue;
                        }

                        // 入力ストリーム解除
                        boardPresenter.UnbindDropInputStream();
                        boardPresenter.UnbindRotateInputStream();
                    }

                    return;

                // --------------------------------------------------
                // Pause
                // --------------------------------------------------
                case PhaseType.Pause:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    foreach (BoardPresenter boardPresenter in _boardPresenters)
                    {
                        if (boardPresenter == null)
                        {
                            continue;
                        }

                        // 入力ストリーム解除
                        boardPresenter.UnbindDropInputStream();
                        boardPresenter.UnbindRotateInputStream();
                    }

                    return;

                // --------------------------------------------------
                // Finish
                // --------------------------------------------------
                case PhaseType.Finish:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    return;

                // --------------------------------------------------
                // Result
                // --------------------------------------------------
                case PhaseType.Result:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    return;

                default:
                    return;
            }
        }

        /// <summary>
        /// Title フェーズ用の入力コマンド購読を登録する
        /// </summary>
        private void BindTitlePhaseInputCommands()
        {
            // 既存購読を破棄
            _inputDisposables?.Dispose();

            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            // ボタン A 押下
            _inputManager.ButtonA.OnUp
                .Subscribe(_ =>
                {
                    // ゲーム開始イベント発火
                    _onGameStartRequested.OnNext(Unit.Default);
                })
                .AddTo(_inputDisposables);
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
                    Debug.Log("drop");
                })
                .AddTo(_inputDisposables);

            // ボタン B 押下
            _inputManager.ButtonB.OnDown
                .Subscribe(_ =>
                {
                    // 回転準備イベント発火
                    _onRotateRequested.OnNext(Unit.Default);
                    Debug.Log("rotate");
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
            PhaseType nextPhase;

            if (phase == PhaseType.Play)
            {
                nextPhase = PhaseType.Pause;

                // 現在のアクティブ状態フェーズをキャッシュ
                _cachedActivePhase = phase;
            }
            else if (phase == PhaseType.Pause)
            {
                // キャッシュしていたアクティブ状態フェーズへ復帰
                nextPhase = _cachedActivePhase;
            }
            else
            {
                return;
            }

            NotifyPhaseChanged(nextPhase);
        }

        // --------------------------------------------------
        // オプション
        // --------------------------------------------------
        /// <summary>
        /// ゲームオプション更新時の処理を行う
        /// </summary>
        private void HandleGameOptionUpdated(in OptionButtonData data)
        {
            switch (data.Type)
            {
                case OptionType.PlayerCount:
                    _gameOptionManager.SetPlayerCount(data.IntValue);
                    break;

                case OptionType.LimitTime:
                    _gameOptionManager.SetLimitTime(data.FloatValue);
                    break;

                case OptionType.BoardSize:
                    _gameOptionManager.SetBoardSize(data.BoardSizeType);
                    break;

                case OptionType.ConnectCount:
                    _gameOptionManager.SetConnectCount(data.IntValue);
                    break;

                case OptionType.CameraRotationSpeed:
                    _gameOptionManager.SetCameraRotationSpeed(data.FloatValue);
                    break;

                case OptionType.PointerSpeed:
                    _gameOptionManager.SetPointerSpeed(data.FloatValue);
                    break;
            }
        }

        // --------------------------------------------------
        // ボード
        // --------------------------------------------------
        /// <summary>
        /// ライン成立時の処理を行う
        /// 複数ラインを 1 本ずつスコアへ分解する
        /// </summary>
        private void HandleLineCompleted(in LineCompleteEvent e)
        {
            // プレイヤー ID
            int playerId = e.Player;

            // ライン配列
            IReadOnlyList<BoardIndex>[] lines = e.LinePositions;

            if (lines == null)
            {
                return;
            }

            // --------------------------------------------------
            // 各ラインを個別にスコアへ変換
            // --------------------------------------------------
            for (int i = 0; i < lines.Length; i++)
            {
                // 現在のライン取得
                IReadOnlyList<BoardIndex> line = lines[i];

                if (line == null)
                {
                    continue;
                }

                // ラインの長さ
                int lineLength = line.Count;

                // スコアイベント発火
                _onScoreUpdated.OnNext(new ScoreEvent(playerId, lineLength));
            }
        }
    }
}