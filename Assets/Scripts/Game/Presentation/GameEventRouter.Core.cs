// ======================================================
// GameEventRouter.Core.cs
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
using OptionSystem.Presentation;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using ScoreSystem.Presentation;
using SoundSystem.Presentation;
using UISystem.Presentation;
using UpdateSystem.Domain;

namespace GameSystem.Presentation
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed partial class GameEventRouter
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

        /// <summary>ResultUIPresenter キャッシュ</summary>
        private readonly ResultUIPresenter _resultUIPresenter;

        /// <summary>SoundManager キャッシュ</summary>
        private readonly SoundManager _soundManager;

        /// <summary>CameraPresenter キャッシュ</summary>
        private readonly CameraPresenter _cameraPresenter;

        /// <summary>SceneObjectContainer キャッシュ配列</summary>
        private readonly BoardPresenter[] _boardPresenters;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // ゲーム
        // --------------------------------------------------
        /// <summary>ゲーム再起動長押し判定中かどうか</summary>
        private bool _isCheckingRestartGame;

        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>フェード待機中に来たフェーズキャッシュ</summary>
        private PhaseType? _pendingPhase;

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>スコア更新の保留フラグ</summary>
        private bool _hasPendingScoreEvent;

        /// <summary>保留中の更新スコアイベント</summary>
        private ScoreEvent _pendingUpdateScoreEvent;

        /// <summary>保留中の加算スコアイベントリスト</summary>
        private readonly Queue<ScoreEvent> _pendingAddScoreEvents = new Queue<ScoreEvent>();

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>現在の入力マッピング番号</summary>
        private int _currentMappingIndex = -1;

        /// <summary>現在アクティブな入力デバイスのキャッシュ</summary>
        private InputDeviceType _cachedActiveDevice = InputDeviceType.Gamepad;

        /// <summary>X ボタン が押下中かどうか</summary>
        private bool _isButtonXPressed;

        /// <summary>左トリガー が押下中かどうか</summary>
        private bool _isLeftTriggerPressed;

        /// <summary>右トリガー が押下中かどうか</summary>
        private bool _isRightTriggerPressed;

        /// <summary>DPad 上入力が押下中かどうか</summary>
        private bool _isDPadUpPressed;
        
        /// <summary>セレクトボタン が押下中かどうか</summary>
        private bool _isSelectButtonPressed;

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>フェード完了フラグ</summary>
        private bool _isFadeCompleted = false;

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

        /// <summary>フェーズ変更リクエスト通知用 Subject</summary>
        private readonly Subject<PhaseChangeEvent> _onPhaseChangeRequested = new Subject<PhaseChangeEvent>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseChangeEvent> OnPhaseChangeRequested => _onPhaseChangeRequested;

        /// <summary>プレイヤー変更用 Subject</summary>
        private readonly Subject<int> _onPlayerChanged = new Subject<int>();

        // --------------------------------------------------
        // システム
        // --------------------------------------------------
        /// <summary>ゲーム終了リクエスト用 Subject</summary>
        private readonly Subject<Unit> _onExitGameRequested = new Subject<Unit>();

        /// <summary>ゲーム終了リクエストストリーム</summary>
        public IObservable<Unit> OnExitGameRequested => _onExitGameRequested;

        /// <summary>ゲーム再起動リクエスト用 Subject</summary>
        private readonly Subject<Unit> _onRestartGameRequested = new Subject<Unit>();

        /// <summary>ゲーム再起動リクエストストリーム</summary>
        public IObservable<Unit> OnRestartGameRequested => _onRestartGameRequested;

        /// <summary>ゲームスピード変更用 Subject</summary>
        private readonly Subject<float> _onGameSpeedChangeRequested = new Subject<float>();

        /// <summary>スコア更新用 Subject</summary>
        private readonly Subject<ScoreEvent> _onScoreUpdated = new Subject<ScoreEvent>();

        /// <summary>スコア加算用 Subject</summary>
        private readonly Subject<ScoreEvent> _onScoreAdded = new Subject<ScoreEvent>();

        /// <summary>ターンカウント通知用 Subject</summary>
        private readonly Subject<int> _onTurnChanged = new Subject<int>();

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>入力マッピング変更用 Subject</summary>
        private readonly Subject<int> _onMappingChanged = new Subject<int>();

        /// <summary>ゲームパッド検知用 Subject</summary>
        private readonly Subject<bool> _onGamepadUsed = new Subject<bool>();

        /// <summary>ポインター座標変更用 Subject</summary>
        private readonly Subject<Vector2> _onPointerPositionChanged = new Subject<Vector2>();

        /// <summary>ゲーム終了入力用 Subject</summary>
        private readonly Subject<Unit> _onExitGameInput = new Subject<Unit>();
        
        /// <summary>スキップ入力用 Subject</summary>
        private readonly Subject<Unit> _onSkipInput = new Subject<Unit>();

        /// <summary>ポーズ入力用 Subject</summary>
        private readonly Subject<Unit> _onPauseInput = new Subject<Unit>();

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

        /// <summary>シーンスタートアニメーションスキップ通知用 Subject</summary>
        private readonly Subject<Unit> _onSceneStartAnimationSkiped = new Subject<Unit>();

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

        /// <summary>コンボ加算通知用 Subject</summary>
        private readonly Subject<int> _onComboAdded = new Subject<int>();

        /// <summary>ライン位置通知用 Subject</summary>
        private readonly Subject<LinePositionInfo> _onLinePositionNotified = new Subject<LinePositionInfo>();

        /// <summary>列選択表示の表示状態通知用 Subject</summary>
        private readonly Subject<bool> _onColumnSelectVisibleChanged = new Subject<bool>();

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // ゲーム
        // --------------------------------------------------
        /// <summary>ゲーム再起動判定時間（秒）</summary>
        private const int RESTART_GAME_HOLD_SECONDS = 5;

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
        private const float GAME_SPEED_FAST = 3.0f;

        /// <summary>3 x 3 ボードサイズ</summary>
        private const int BOARD_SIZE_THREE = 3;

        /// <summary>5 x 5 ボードサイズ</summary>
        private const int BOARD_SIZE_FIVE = 5;

        /// <summary>通常スコア倍率</summary>
        private const float NORMAL_SCORE_MULTIPLIER = 1.0f;

        /// <summary>回転入力スコア倍率</summary>
        private const float ROTATE_SCORE_MULTIPLIER = 3.0f;

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

            _resultUIPresenter = (ResultUIPresenter)
                updatableReader.Get(UpdatableType.ResultUIPresenter);

            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;
            _scoreManager = ScoreManager.Instance;
            _soundManager = SoundManager.Instance;

            if (_gameOptionManager == null ||
                _inputManager == null ||
                _scoreManager == null)
            {
                Debug.LogError("[GameEventRouter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }
        }
    }
}