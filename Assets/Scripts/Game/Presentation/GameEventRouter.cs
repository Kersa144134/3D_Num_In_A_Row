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
using Cysharp.Threading.Tasks;
using InputSystem.Domain;
using InputSystem.Presentation;
using OptionSystem.Domain;
using OptionSystem.Presentation;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using ScoreSystem.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using UISystem.Presentation;
using UniRx;
using Unity.Multiplayer.PlayMode;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UpdateSystem.Domain;

namespace GameSystem.Presentation
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed class GameEventRouter
    {
        // ======================================================
        // 列挙型
        // ======================================================

        /// <summary>
        /// ボード操作入力種別
        /// </summary>
        private enum BoardInputType
        {
            /// <summary>駒落下入力</summary>
            Drop = 0,

            /// <summary>ボード回転入力</summary>
            Rotate = 1
        }
        
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

        /// <summary>フェード待機中に来たフェーズキャッシュ</summary>
        private PhaseType? _pendingPhase;

        /// <summary>スコア更新の保留フラグ</summary>
        private bool _hasPendingScoreEvent;

        /// <summary>保留中のスコアイベント</summary>
        private ScoreEvent _pendingScoreEvent;

        /// <summary>現在の入力マッピング番号</summary>
        private int _currentMappingIndex = -1;

        /// <summary>フェード完了フラグ</summary>
        private bool _isFadeCompleted;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>ボードごとの位置をキャッシュする辞書</summary>
        private readonly Dictionary<BoardPresenter, Vector3> _boardPosition =
            new Dictionary<BoardPresenter, Vector3>();

        // ======================================================
        // UniRx 変数
        // ======================================================

        // --------------------------------------------------
        // 購読管理
        // --------------------------------------------------
        /// <summary>共通購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>入力用購読管理</summary>
        private CompositeDisposable _inputDisposables = new CompositeDisposable();

        /// <summary>シーンロード用購読管理</summary>
        private IDisposable _sceneLoadSubscription;

        /// <summary>スキップ入力用購読管理</summary>
        private IDisposable _skipInputSubscription;

        /// <summary>ゲームスピード変更用購読管理</summary>
        private CompositeDisposable _gameSpeedChangeDisposables = new CompositeDisposable();

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

        /// <summary>現在ボード入力種別ストリーム</summary>
        private readonly ReactiveProperty<BoardInputType> _currentBoardInputType
            = new ReactiveProperty<BoardInputType>(BoardInputType.Drop);

        /// <summary>フェーズ変更通知用 Subject</summary>
        private readonly Subject<PhaseChangeEvent> _onPhaseChanged = new Subject<PhaseChangeEvent>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseChangeEvent> OnPhaseChanged => _onPhaseChanged;

        /// <summary>プレイヤー変更用 Subject</summary>
        private readonly Subject<int> _onPlayerChanged = new Subject<int>();

        // --------------------------------------------------
        // オプション
        // --------------------------------------------------
        /// <summary>ゲームスピード変更用 Subject</summary>
        private readonly Subject<float> _onGameSpeedChangeRequested = new Subject<float>();

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

        // --------------------------------------------------
        // ボード
        // --------------------------------------------------
        /// <summary>駒落下入力用 Subject</summary>
        private readonly Subject<Unit> _onDropRequested = new Subject<Unit>();

        /// <summary>ボード回転入力用 Subject</summary>
        private readonly Subject<Unit> _onRotateRequested = new Subject<Unit>();

        /// <summary>駒落下入力用 Subject</summary>
        private readonly Subject<Unit> _onDropExecuted = new Subject<Unit>();

        /// <summary>ボード回転実行用 Subject</summary>
        private readonly Subject<RotationCommand> _onRotateExecuted = new Subject<RotationCommand>();

        /// <summary>中心座標算出ストリーム</summary>
        private readonly Subject<Vector3> _onCenterPositionCalculated = new Subject<Vector3>();

        /// <summary>成立ライン中心差分ベクトル算出通知用 Subject</summary>
        private readonly Subject<Vector3> _onCenterOffsetVectorCalculated = new Subject<Vector3>();

        /// <summary>列選択表示の表示状態通知用 Subject</summary>
        private readonly Subject<bool> _onColumnSelectVisibleChanged = new Subject<bool>();

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
        /// <summary>通常ゲームスピード</summary>
        private const float GAME_SPEED_NORMAL = 1.0f;

        /// <summary>高速ゲームスピード</summary>
        private const float GAME_SPEED_FAST = 2.0f;

        /// <summary>3 x 3 ボードサイズ</summary>
        private const int BOARD_SIZE_THREE = 3;

        /// <summary>5 x 5 ボードサイズ</summary>
        private const int BOARD_SIZE_FIVE = 5;

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>入力バインド切替時の遅延時間（秒）</summary>
        private const float INPUT_BIND_DELAY_SECONDS = 0.5f;

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
            // ボタン A 押す
            _inputManager.ButtonA.OnDown
                .Subscribe(_ =>
                {
                    // スキップイベント発火
                    _onSkipRequested.OnNext(Unit.Default);
                })
                .AddTo(_disposables);
            _onFadeCompleted
                .Subscribe(_ =>
                {
                    _isFadeCompleted = true;

                    // 入力購読処理の保留があれば即実行
                    if (_pendingPhase.HasValue)
                    {
                        HandlePhaseInputSwitch(_pendingPhase.Value);
                        _pendingPhase = null;
                    }
                })
                .AddTo(_disposables);

            // --------------------------------------------------
            // フェーズ
            // --------------------------------------------------
            _currentPhase
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(phase =>
                {
                    if (phase == PhaseType.Ready)
                    {
                        // スコア計算クラス初期化
                        _scoreManager.Initialize(_gameOptionManager.PlayerCount);
                    }

                    if (phase == PhaseType.ChangePlayer)
                    {
                        // スコア累積カウントリセット
                        _scoreManager.ResetAllCumulativeCount();
                    }

                    // フェード未完了なら入力購読処理を保留
                    if (!_isFadeCompleted)
                    {
                        _pendingPhase = phase;

                        return;
                    }

                    HandlePhaseInputSwitch(phase);
                })
                .AddTo(_disposables);
            _phaseMachine.CurrentPlayerIndex
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(player => _onPlayerChanged.OnNext(player))
                .AddTo(_disposables);
            _phaseMachine.PlayEnterCount
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(turn => Debug.Log(turn))
                .AddTo(_disposables);

            // --------------------------------------------------
            // オプション
            // --------------------------------------------------
            if (_gameOptionManager != null)
            {
                _gameOptionManager.BindStream(_onGameSpeedChangeRequested);
            }

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            if (_inputManager != null)
            {
                _inputManager.BindStreams(_onMappingChanged, _onPointerPositionChanged);

                // スタートボタン押す
                _inputManager.StartButton.OnDown
                    .Subscribe(e => TogglePausePhase(_currentPhase.Value))
                    .AddTo(_disposables);
                _inputManager.ActiveDeviceType
                    .Subscribe(e => NotifyActiveControllerChanged(e))
                    .AddTo(_disposables);
            }

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
                _titleUIPresenter.BindBaseStreams(_fadeInTrigger, _fadeOutTrigger, _onFadeCompleted);

                _titleUIPresenter.OnDialogVisibleChanged
                    .Subscribe(_ =>
                    {
                        UnbindInputCommands();
                    })
                    .AddTo(_disposables);
                _titleUIPresenter.OnFadeInCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _titleUIPresenter.OnFadeOutCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _titleUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneChangeRequested())
                    .AddTo(_disposables);
            }

            if (_mainUIPresenter != null)
            {
                _mainUIPresenter.BindStreams(
                    _currentPhase,
                    _onPlayerChanged,
                    _onScoreUpdated,
                    _currentPhase.Select(phase => phase != PhaseType.Play),
                    _onGamepadUsed,
                    _onColumnSelectVisibleChanged,
                    _onDropRequested,
                    _onRotateRequested,
                    _phaseMachine.LimitTime);

                _mainUIPresenter.OnChangePlayerAnimationEnd
                    .Subscribe(_ => NotifyPhaseChanged(PhaseType.Play))
                    .AddTo(_disposables);

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _mainUIPresenter.BindBaseStreams(_fadeInTrigger, _fadeOutTrigger, _onFadeCompleted);

                _mainUIPresenter.OnDialogVisibleChanged
                    .Subscribe(_ =>
                    {
                        UnbindInputCommands();
                    })
                    .AddTo(_disposables);
                _mainUIPresenter.OnFadeInCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _mainUIPresenter.OnFadeOutCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _mainUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneChangeRequested())
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // カメラ
            // --------------------------------------------------
            if (_cameraPresenter != null)
            {
                _cameraPresenter.BindStreams(
                    Observable.CombineLatest(
                        _currentPhase,
                        _currentBoardInputType,
                        (phase, inputType) =>
                        {
                            bool isPlay = phase == PhaseType.Play;
                            bool isRotate = inputType == BoardInputType.Rotate;

                            // Play フェーズでないまたは Rotate 中に有効
                            return !isPlay || isRotate;
                        }),
                    _onGamepadUsed,
                    _mainUIPresenter.OnSwitchProjection,
                    _onCenterPositionCalculated,
                    _onCenterOffsetVectorCalculated
                );
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

                    boardPresenter.OnPlayerEnd
                        .Subscribe(_ => NotifyPhaseChanged(PhaseType.ChangePlayer))
                        .AddTo(_disposables);
                    boardPresenter.OnDropInputted
                        .Subscribe(_ => NotifyPhaseChanged(PhaseType.Event))
                        .AddTo(_disposables);
                    boardPresenter.OnRotateInputted
                        .Subscribe(_ => NotifyPhaseChanged(PhaseType.Event))
                        .AddTo(_disposables);
                    boardPresenter.OnLineComplete
                        .Subscribe(e => HandleLineCompleted(e))
                        .AddTo(_disposables);
                    boardPresenter.OnLineDelete
                        .Subscribe(_ =>
                        {
                            if (!_hasPendingScoreEvent)
                            {
                                return;
                            }

                            // スコア更新通知
                            _onScoreUpdated.OnNext(_pendingScoreEvent);

                            // リセット
                            _hasPendingScoreEvent = false;
                        })
                        .AddTo(_disposables);
                    boardPresenter.OnCenterPositionCalculated
                        .Subscribe(linePosition => ProcessCenterOffset(boardPresenter, linePosition))
                        .AddTo(_disposables);
                    boardPresenter.IsColumnSelectVisible
                        .DistinctUntilChanged()
                        .Subscribe(isVisible => _onColumnSelectVisibleChanged.OnNext(isVisible))
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

            UnbindSceneLoadProgressStream();
            UnbindEventSkipStream();
        }

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        public void BindStreams(
            in IObservable<string> onPrepareStart,
            in IObservable<float> onPrepareEnd,
            in IObservable<float> onSceneChanged)
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
        public void BindSceneLoadProgressStream(in IObservable<float> onSceneLoadProgress)
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
        /// シーンロード進捗ストリームの購読を解除する
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
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// イベントスキップストリームを購読する
        /// </summary>
        /// <typeparam name="TEvent">発火イベント型</typeparam>
        /// <param name="subject">発火対象 Subject</param>
        /// <param name="eventValue">発火時に送信する値</param>
        private void BindEventSkipStream<TEvent>(Subject<TEvent> subject, TEvent eventValue)
        {
            // 多重購読防止
            _skipInputSubscription?.Dispose();

            _skipInputSubscription = _onSkipRequested
                .Subscribe(_ =>
                {
                    // スキップ入力時に指定イベントを発火
                    subject.OnNext(eventValue);

                    // 即時購読解除
                    UnbindEventSkipStream();
                });
        }

        /// <summary>
        /// イベントスキップストリームの購読を解除する
        /// </summary>
        private void UnbindEventSkipStream()
        {
            _skipInputSubscription?.Dispose();
            _skipInputSubscription = null;
        }

        /// <summary>
        /// ゲームスピード変更ストリームを購読する
        /// </summary>
        private void BindGameSpeedChangeStream()
        {
            // 多重購読防止
            _gameSpeedChangeDisposables?.Dispose();

            // CompositeDisposable 生成
            _gameSpeedChangeDisposables = new CompositeDisposable();

            // R ショルダー 押す
            _inputManager.RightShoulder.OnDown
                .Subscribe(_ =>
                {
                    // 高速状態へ変更
                    _onGameSpeedChangeRequested.OnNext(GAME_SPEED_FAST);
                })
                .AddTo(_gameSpeedChangeDisposables);

            // R ショルダー 離す
            _inputManager.RightShoulder.OnUp
                .Subscribe(_ =>
                {
                    // 通常状態へ変更
                    _onGameSpeedChangeRequested.OnNext(GAME_SPEED_NORMAL);
                })
                .AddTo(_gameSpeedChangeDisposables);
        }

        /// <summary>
        /// ゲームスピード変更ストリームの購読を解除する
        /// </summary>
        private void UnbindGameSpeedChangeStream()
        {
            // 通常速度へリセット
            _onGameSpeedChangeRequested.OnNext(1.0f);

            // イベント購読解除
            _gameSpeedChangeDisposables?.Dispose();
            _gameSpeedChangeDisposables = null;
        }

        /// <summary>
        /// スコア更新ストリームを購読する
        /// </summary>
        private void BindScoreUpdateStream()
        {
            if (_scoreManager == null)
            {
                return;
            }

            // プレイヤー人数取得
            int playerCount = _gameOptionManager.PlayerCount;

            // 1 ベースでループ
            for (int playerId = 1; playerId <= playerCount; playerId++)
            {
                int currentPlayerId = playerId;

                _scoreManager.GetTotalScore(currentPlayerId)
                    .Skip(1)
                    .Subscribe(score =>
                    {
                        // 最新スコアを保留イベントとして保持する
                        _pendingScoreEvent = new ScoreEvent(currentPlayerId, score);

                        _hasPendingScoreEvent = true;
                    })
                    .AddTo(_disposables);
            }
        }

        /// <summary>
        /// 駒落下用の入力コマンド購読を登録する
        /// </summary>
        private void BindDropInputCommands()
        {
            // 入力購読解除
            UnbindInputCommands();

            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            // ボタン A 離す
            _inputManager.ButtonA.OnUp
                .Subscribe(_ =>
                {
                    // 駒落下イベント発火
                    _onDropExecuted.OnNext(Unit.Default);
                })
                .AddTo(_inputDisposables);

            // ボタン B 押す
            _inputManager.ButtonB.OnDown
                .Subscribe(async _ =>
                {
                    // ボード回転準備イベント発火
                    _onRotateRequested.OnNext(Unit.Default);

                    // 入力購読解除
                    UnbindInputCommands();

                    // CompositeDisposable 生成
                    _inputDisposables = new CompositeDisposable();

                    // 入力切替遅延
                    // タイムスケールを無視する
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(INPUT_BIND_DELAY_SECONDS),
                        DelayType.UnscaledDeltaTime
                    );

                    // 回転入力コマンド再登録
                    BindRotateInputCommands();
                })
                .AddTo(_inputDisposables);

            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.BindDropInputStream(_onDropExecuted);
                boardPresenter.UnbindRotateInputStream();
            }

            // ボード入力更新
            _currentBoardInputType.Value = BoardInputType.Drop;
        }

        /// <summary>
        /// ボード回転用の入力コマンド購読を登録する
        /// </summary>
        private void BindRotateInputCommands()
        {
            // 入力購読解除
            UnbindInputCommands();

            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            // 左スティック初回入力
            _inputManager.LeftStick.OnDown
                .Subscribe(direction =>
                {
                    // 左方向
                    if (direction == Vector2.left)
                    {
                        // Z- 回転実行イベント発火
                        _onRotateExecuted.OnNext(
                            new RotationCommand(
                                RotationAxis.Z,
                                RotationDirection.Negative
                            )
                        );

                        return;
                    }

                    // 右方向
                    if (direction == Vector2.right)
                    {
                        // Z+ 回転実行イベント発火
                        _onRotateExecuted.OnNext(
                            new RotationCommand(
                                RotationAxis.Z,
                                RotationDirection.Positive
                            )
                        );

                        return;
                    }

                    // 上方向
                    if (direction == Vector2.up)
                    {
                        // X- 回転実行イベント発火
                        _onRotateExecuted.OnNext(
                            new RotationCommand(
                                RotationAxis.X,
                                RotationDirection.Negative
                            )
                        );

                        return;
                    }

                    // 下方向
                    if (direction == Vector2.down)
                    {
                        // X+ 回転実行イベント発火
                        _onRotateExecuted.OnNext(
                            new RotationCommand(
                                RotationAxis.X,
                                RotationDirection.Positive
                            )
                        );
                    }
                })
                .AddTo(_inputDisposables);

            // ボタン B 押す
            _inputManager.ButtonB.OnDown
                .Subscribe(async _ =>
                {
                    // 駒落下準備イベント発火
                    _onDropRequested.OnNext(Unit.Default);

                    // 入力購読解除
                    UnbindInputCommands();

                    // CompositeDisposable 生成
                    _inputDisposables = new CompositeDisposable();

                    // 入力切替遅延
                    // タイムスケールを無視する
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(INPUT_BIND_DELAY_SECONDS),
                        DelayType.UnscaledDeltaTime
                    );

                    // 駒落下時の入力コマンド登録
                    BindDropInputCommands();
                })
                .AddTo(_inputDisposables);

            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.UnbindDropInputStream();
                boardPresenter.BindRotateInputStream(_onRotateExecuted);
            }

            // ボード入力更新
            _currentBoardInputType.Value = BoardInputType.Rotate;
        }

        /// <summary>
        /// 入力コマンド購読を解除する
        /// </summary>
        private void UnbindInputCommands()
        {
            _inputDisposables?.Dispose();
            _inputDisposables = null;

            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.UnbindDropInputStream();
                boardPresenter.UnbindRotateInputStream();
            }
        }

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
                    // シーン遷移実行通知
                    BindEventSkipStream(_onSceneChangeExecuted, Unit.Default);

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

                case OptionType.CameraSpeed:
                    _gameOptionManager.SetCameraSpeed(data.FloatValue);
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
                _onGamepadUsed.OnNext(true);
            }
            else
            {
                _onGamepadUsed.OnNext(false);
            }
        }

        /// <summary>
        /// フェーズ変更時の入力購読切替処理を行う
        /// </summary>
        /// <param name="phase">変更後のフェーズ</param>
        private void HandlePhaseInputSwitch(in PhaseType phase)
        {
            // 入力購読解除
            UnbindInputCommands();

            switch (phase)
            {
                // --------------------------------------------------
                // Title, Result
                // --------------------------------------------------
                case PhaseType.Title:
                case PhaseType.Result:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    break;

                // --------------------------------------------------
                // Event, Pause
                // --------------------------------------------------
                case PhaseType.Event:
                case PhaseType.Pause:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    break;

                // --------------------------------------------------
                // Ready
                // --------------------------------------------------
                case PhaseType.Ready:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // スキップ入力
                    // ChangePlayer へフェーズ遷移通知
                    BindEventSkipStream(
                        _onPhaseChanged,
                        new PhaseChangeEvent(
                            _currentPhase.Value,
                            PhaseType.ChangePlayer
                        )
                    );

                    // ゲームスピード変更購読
                    BindGameSpeedChangeStream();

                    // スコア更新購読
                    BindScoreUpdateStream();

                    break;

                // --------------------------------------------------
                // Play
                // --------------------------------------------------
                case PhaseType.Play:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // --------------------------------------------------
                    // 直前の入力種別に応じて入力コマンドを登録
                    // --------------------------------------------------
                    switch (_currentBoardInputType.Value)
                    {
                        // 駒落下入力コマンドを登録
                        case BoardInputType.Drop:
                            BindDropInputCommands();
                            break;

                        // ボード回転入力コマンドを登録
                        case BoardInputType.Rotate:
                            BindRotateInputCommands();
                            break;
                    }

                    break;

                // --------------------------------------------------
                // ChangePlayer
                // --------------------------------------------------
                case PhaseType.ChangePlayer:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // ボード入力種別キャッシュをリセット
                    _currentBoardInputType.Value = BoardInputType.Drop;

                    // ボード回転準備イベント発火
                    _onDropRequested.OnNext(Unit.Default);

                    // スキップ入力購読解除
                    UnbindEventSkipStream();

                    break;

                // --------------------------------------------------
                // Finish
                // --------------------------------------------------
                case PhaseType.Finish:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    // ゲームスピード変更購読解除
                    UnbindGameSpeedChangeStream();

                    break;

                default:
                    break;
            }
        }

        // --------------------------------------------------
        // ボード
        // --------------------------------------------------
        /// <summary>
        /// ライン成立時の処理を行う
        /// </summary>
        private void HandleLineCompleted(in IReadOnlyList<LineCompleteEvent> events)
        {
            // --------------------------------------------------
            // 各イベントを個別に処理
            // --------------------------------------------------
            for (int i = 0; i < events.Count; i++)
            {
                // 現在処理中のライン成立イベント
                LineCompleteEvent lineEvent = events[i];

                // プレイヤー ID
                int playerId = lineEvent.Player;

                // ライン座標リスト
                IReadOnlyList<BoardIndex> line = lineEvent.LinePositions;

                if (line == null)
                {
                    continue;
                }

                // スコア累積カウント加算
                _scoreManager.AddAllCumulativeCount();

                // 1 ライン分のスコア加算
                _scoreManager.AddLineScore(playerId, line.Count);
            }
        }

        /// <summary>
        /// ボード位置とライン中心座標から差分ベクトルを算出し、イベント通知する
        /// </summary>
        /// <param name="boardPresenter">対象ボード</param>
        /// <param name="linePosition">成立ライン中心座標</param>
        private void ProcessCenterOffset(in BoardPresenter boardPresenter, in Vector3 linePosition)
        {
            // --------------------------------------------------
            // ボード位置取得
            // --------------------------------------------------
            Vector3 boardPosition;

            // 辞書に登録済かどうか
            if (_boardPosition.ContainsKey(boardPresenter))
            {
                boardPosition = _boardPosition[boardPresenter];
            }
            else
            {
                boardPosition = boardPresenter.gameObject.transform.position;

                // 辞書登録
                _boardPosition.Add(boardPresenter, boardPosition);
            }

            // --------------------------------------------------
            // 中心差分ベクトル算出
            // --------------------------------------------------
            Vector3 centerOffset = linePosition - boardPosition;

            // 正規化
            Vector3 normalizedCenterOffset = centerOffset.normalized;

            // --------------------------------------------------
            // イベント通知
            // --------------------------------------------------
            _onCenterPositionCalculated.OnNext(linePosition);
            _onCenterOffsetVectorCalculated.OnNext(normalizedCenterOffset);
        }
    }
}