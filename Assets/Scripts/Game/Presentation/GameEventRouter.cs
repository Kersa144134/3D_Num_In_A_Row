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

        /// <summary>ScoreManager キャッシュ</summary>
        private readonly ScoreManager _scoreManager;

        /// <summary>InputManager キャッシュ</summary>
        private readonly InputManager _inputManager;

        /// <summary>TitleUIPresenter キャッシュ</summary>
        private readonly TitleUIPresenter _titleUIPresenter;

        /// <summary>MainUIPresenter キャッシュ</summary>
        private readonly MainUIPresenter _mainUIPresenter;

        /// <summary>CameraPresenter キャッシュ</summary>
        private readonly CameraPresenter _cameraPresenter;

        /// <summary>SceneObjectContainer キャッシュ配列</summary>
        private readonly BoardPresenter[] _boardPresenters;

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

        // --------------------------------------------------
        // 購読管理
        // --------------------------------------------------
        /// <summary>共通購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>入力用購読管理</summary>
        private CompositeDisposable _inputDisposables;

        /// <summary>シーンロード用購読管理</summary>
        private IDisposable _sceneLoadSubscription;

        /// <summary>スキップ入力用購読管理</summary>
        private IDisposable _skipInputSubscription;

        // --------------------------------------------------
        // シーン
        // --------------------------------------------------
        /// <summary>シーン遷移予約通知用 Subject</summary>
        private readonly Subject<string> _onSceneChangeRequested = new Subject<string>();

        /// <summary>シーン遷移予約ストリーム</summary>
        public IObservable<string> OnSceneChangeRequested => _onSceneChangeRequested;

        /// <summary>シーン遷移実行通知用 Subject</summary>
        private readonly Subject<Unit> _onSceneChangeExecuted = new Subject<Unit>();

        /// <summary>シーン遷移実行ストリーム</summary>
        public IObservable<Unit> OnSceneChangeExecuted => _onSceneChangeExecuted;

        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>現在フェーズストリーム</summary>
        private readonly IReadOnlyReactiveProperty<PhaseType> _currentPhase;

        /// <summary>フェーズ変更通知用 Subject</summary>
        private readonly Subject<PhaseChangeEvent> _onPhaseChanged = new Subject<PhaseChangeEvent>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseChangeEvent> OnPhaseChanged => _onPhaseChanged;

        /// <summary>プレイヤー変更用 Subject</summary>
        private readonly Subject<int> _onPlayerChanged = new Subject<int>();

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>スコア更新用 Subject</summary>
        private readonly Subject<ScoreEvent> _onScoreUpdated = new Subject<ScoreEvent>();

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>入力マッピング変更用 Subject</summary>
        private readonly Subject<int> _onMappingChanged = new Subject<int>();

        /// <summary>ゲームパッド検知用 Subject</summary>
        private readonly Subject<bool> _onGamepadUsed = new Subject<bool>();

        /// <summary>ポインター座標変更用 Subject</summary>
        private readonly Subject<Vector2> _onPointerPositionChanged = new Subject<Vector2>();

        /// <summary>スキップ入力用 Subject</summary>
        private readonly Subject<Unit> _onSkipRequested = new Subject<Unit>();

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>画面フェードイン開始通知用 Subject</summary>
        private readonly Subject<float> _fadeInTrigger = new Subject<float>();

        /// <summary>画面フェードアウト開始通知用 Subject</summary>
        private readonly Subject<float> _fadeOutTrigger = new Subject<float>();

        /// <summary>画面フェード完了通知用 Subject</summary>
        private readonly Subject<Unit> _onFadeCompleted = new Subject<Unit>();

        /// <summary>画面フェード完了ストリーム</summary>
        public IObservable<Unit> OnFadeCompleted => _onFadeCompleted;

        /// <summary>ダイアログ画面での入力用 Subject</summary>
        private readonly Subject<bool> _onDialogInputed = new Subject<bool>();

        // --------------------------------------------------
        // ボード
        // --------------------------------------------------
        /// <summary>駒配置入力用 Subject</summary>
        private readonly Subject<Unit> _onDropRequested = new Subject<Unit>();

        /// <summary>ボード回転入力用 Subject</summary>
        private readonly Subject<Unit> _onRotateRequested = new Subject<Unit>();

        /// <summary>ボード回転実行用 Subject</summary>
        private readonly Subject<RotationCommand> _onRotateExecuted = new Subject<RotationCommand>();

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // シーン
        // --------------------------------------------------
        /// <summary>タイトルシーン名</summary>
        private const string TITLE_SCENE_NAME = "TitleScene";

        /// <summary>3 x 3 ボードシーン名</summary>
        private const string THREE_SIZE_SCENE_NAME = "ThreeSizeScene";

        /// <summary>5 x 5 ボードシーン名</summary>
        private const string FIVE_SIZE_SCENE_NAME = "FiveSizeScene";

        /// <summary>リザルトシーン名</summary>
        private const string RESULT_SCENE_NAME = "ResultScene";

        // --------------------------------------------------
        // オプション
        // --------------------------------------------------
        /// <summary>3 x 3 ボードサイズ</summary>
        private const int BOARD_SIZE_THREE = 3;

        /// <summary>5 x 5 ボードサイズ</summary>
        private const int BOARD_SIZE_FIVE = 5;

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

            // ボタン A 押す
            _inputManager.ButtonA.OnDown
                .Subscribe(_ =>
                {
                    // スキップイベント発火
                    _onSkipRequested.OnNext(Unit.Default);
                })
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
            _inputManager.BindStreams(_onMappingChanged, _onPointerPositionChanged);

            // スタートボタン押す
            _inputManager.StartButton.OnDown
                .Subscribe(e => TogglePausePhase(_currentPhase.Value))
                .AddTo(_disposables);
            // アクティブコントローラー変更
            _inputManager.ActiveDeviceType
                .Subscribe(e => NotifyActiveControllerChanged(e))
                .AddTo(_disposables);

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            if (_titleUIPresenter != null)
            {
                _titleUIPresenter.BindStreams(
                    _currentPhase.Select(phase => phase != PhaseType.Title),
                    _onGamepadUsed);

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
                _titleUIPresenter.BindBaseStreams(_onDialogInputed, _fadeInTrigger, _fadeOutTrigger);

                _titleUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneChangeRequested())
                    .AddTo(_disposables);
                _titleUIPresenter.OnDialogVisibleChanged
                    .Subscribe(isVisible =>
                    {
                        // ダイアログ表示時
                        if (isVisible)
                        {
                            BindShowDialogInputCommands();
                        }
                        // ダイアログ非表示時
                        else
                        {
                            UnbindInputCommands();
                        }
                    })
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
                _mainUIPresenter.BindStreams(
                    _currentPhase,
                    _onPlayerChanged,
                    _onScoreUpdated,
                    _currentPhase.Select(phase => phase != PhaseType.Play),
                    _onRotateRequested,
                    _phaseMachine.LimitTime);

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _mainUIPresenter.BindBaseStreams(_onDialogInputed, _fadeInTrigger, _fadeOutTrigger);

                _mainUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneChangeRequested())
                    .AddTo(_disposables);
                _mainUIPresenter.OnDialogVisibleChanged
                    .Subscribe(isVisible =>
                    {
                        // ダイアログ表示時
                        if (isVisible)
                        {
                            BindShowDialogInputCommands();
                        }
                        // ダイアログ非表示時
                        else
                        {
                            UnbindInputCommands();
                        }
                    })
                    .AddTo(_disposables);
                _mainUIPresenter.OnFadeInCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _mainUIPresenter.OnFadeOutCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // カメラ
            // --------------------------------------------------
            if (_cameraPresenter != null)
            {
                _cameraPresenter.BindStreams(
                    _currentPhase.Select(phase => phase != PhaseType.Play),
                    _mainUIPresenter.OnSwitchProjection);
            }

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
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        public void Dispose()
        {
            // 購読解除
            _disposables.Dispose();

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
        }

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        public void BindStreams(
            IObservable<string> onPrepareStart,
            IObservable<float> onPrepareEnd,
            IObservable<float> onSceneChanged)
        {
            onPrepareStart
                .Subscribe(sceneName =>
                {
                    // ロード準備開始
                    HandleLoadPrepareStart(sceneName);
                })
                .AddTo(_disposables);

            onPrepareEnd
                .Subscribe(fadeTime =>
                {
                    // ロード準備完了
                    HandleLoadPrepareEnd(fadeTime);
                })
                .AddTo(_disposables);

            onSceneChanged
                .Subscribe(seconds =>
                {
                    // フェードアウト時間を通知
                    _fadeOutTrigger.OnNext(seconds);
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// シーンロード進捗ストリームを購読する
        /// </summary>
        public void BindSceneLoadProgressStream(IObservable<float> onSceneLoadProgress)
        {
            // 多重購読防止
            _sceneLoadSubscription?.Dispose();

            _sceneLoadSubscription = onSceneLoadProgress
                .Subscribe(progress =>
                {
                    // デバッグ用
                    Debug.Log(progress);
                });
        }

        /// <summary>
        /// シーン変更ストリームの購読を解除する
        /// </summary>
        public void UnbindSceneLoadProgressStream()
        {
            _sceneLoadSubscription?.Dispose();
            _sceneLoadSubscription = null;
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
        private void NotifySceneChangeRequested()
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
            switch (sceneName)
            {
                case TITLE_SCENE_NAME:
                    // スキップ入力
                    _onSkipRequested
                        .Subscribe(_ =>
                        {
                            // シーン遷移実行通知
                            _onSceneChangeExecuted.OnNext(Unit.Default);
                        })
                        .AddTo(_disposables);

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

        /// <summary>
        /// ポーズ用のフェーズトグル処理を行う
        /// </summary>
        private void TogglePausePhase(in PhaseType phase)
        {
            // フェーズ遷移先を決定する
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

            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            switch (phase)
            {
                // --------------------------------------------------
                // Title
                // --------------------------------------------------
                case PhaseType.Title:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

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
        /// Play フェーズ用の入力コマンド購読を登録する
        /// </summary>
        private void BindPlayPhaseInputCommands()
        {
            // ボタン A 離す
            _inputManager.ButtonA.OnUp
                .Subscribe(_ =>
                {
                    // 駒配置イベント発火
                    _onDropRequested.OnNext(Unit.Default);
                })
                .AddTo(_inputDisposables);

            // ボタン B 押す
            _inputManager.ButtonB.OnDown
                .Subscribe(_ =>
                {
                    // 回転準備イベント発火
                    _onRotateRequested.OnNext(Unit.Default);
                })
                .AddTo(_inputDisposables);
        }

        /// <summary>
        /// Event フェーズ用の入力コマンド購読を登録する
        /// </summary>
        private void BindEventPhaseInputCommands()
        {
            // 左スティック左押す
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

            // 左スティック右押す
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

            // 左スティック上押す
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

            // 左スティック下押す
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
        /// ダイアログ表示時のマッピング変更処理を行う
        /// </summary>
        private void BindShowDialogInputCommands()
        {
            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            // ボタン A 押す
            _inputManager.ButtonA.OnDown
                .Subscribe(_ =>
                {
                    // ダイアログ画面での決定入力として発火
                    _onDialogInputed.OnNext(true);
                })
                .AddTo(_inputDisposables);

            // ボタン B 押す
            _inputManager.ButtonB.OnDown
                .Subscribe(_ =>
                {
                    // ダイアログ画面でのキャンセル入力として発火
                    _onDialogInputed.OnNext(false);
                })
                .AddTo(_inputDisposables);

            // ボタン X 押す
            _inputManager.ButtonX.OnDown
                .Subscribe(_ =>
                {
                    // ダイアログ画面でのキャンセル入力として発火
                    _onDialogInputed.OnNext(false);
                })
                .AddTo(_inputDisposables);

            // ボタン Y 押す
            _inputManager.ButtonY.OnDown
                .Subscribe(_ =>
                {
                    // ダイアログ画面でのキャンセル入力として発火
                    _onDialogInputed.OnNext(false);
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