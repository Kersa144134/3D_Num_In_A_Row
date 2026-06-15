// ======================================================
// GameManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-04-08
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using GameSystem.Application;
using OptionSystem.Presentation;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using SoundSystem.Presentation;
using UpdateSystem.Application;
using UpdateSystem.Domain;

namespace GameSystem.Presentation
{
    /// <summary>
    /// シーン遷移・フェーズ遷移・Update 実行を統括する
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("初期フェーズ")]
        /// <summary>シーン読み込み時の初期フェーズ</summary>
        [SerializeField] private PhaseType _startPhase;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン遷移管理クラス</summary>
        private SceneLoader _sceneLoader = new SceneLoader();

        /// <summary>フェーズ管理マシン</summary>
        private PhaseMachine _phaseMachine;

        /// <summary>フェーズ遷移およびプレイ進行設定</summary>
        private PhaseTransitionConfig _phaseTransitionConfig;

        /// <summary>UpdatableBindAttribute を走査し UpdatableContexts へ自動登録を行うクラス</summary>
        private readonly UpdatableAttributeScanner _updatableAttributeScanner = new UpdatableAttributeScanner();

        /// <summary>IUpdatable を実装しているコンポーネントを取得するクラス</summary>
        private readonly UpdatableCollector _updatableCollector = new UpdatableCollector();

        /// <summary>IUpdatable のライフサイクル実行クラス</summary>
        private readonly UpdatableLifecycleRunner _updatableLifecycleRunner = new UpdatableLifecycleRunner();

        /// <summary>シーン内イベントを仲介するクラス</summary>
        private GameEventRouter _eventRouter;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>SoundManager キャッシュ</summary>
        private SoundManager _soundManager;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // シーン
        // --------------------------------------------------
        /// <summary>現在のシーン名</summary>
        private string _currentScene = string.Empty;

        /// <summary>遷移先シーン名</summary>
        private string _targetScene = string.Empty;

        /// <summary>シーン切り替え直後かどうかを示すフラグ</summary>
        private bool _isAfterSceneChange = true;

        /// <summary>シーン遷移中かどうかを示すフラグ</summary>
        private bool _isSceneTransitioning = false;

        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>遷移予定のフェーズ</summary>
        private PhaseType _targetPhase = PhaseType.None;

        // --------------------------------------------------
        // Updatables
        // --------------------------------------------------
        /// <summary>IUpdatable を保持している GameObject 群</summary>
        private GameObject[] _components;

        /// <summary>Updatable を保持するコンテキスト</summary>
        private UpdatableContexts _updatableContexts;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>アプリケーション全体で固定する目標 FPS</summary>
        private const int TARGET_FRAME_RATE = 120;

        // --------------------------------------------------
        // シーン
        // --------------------------------------------------
        /// <summary>シーンロードの最低保証時間</summary>
        private const float MIN_LOAD_TIME_SECONDS = 0.25f;

        // --------------------------------------------------
        // アニメーション
        // --------------------------------------------------
        /// <summary>ゲーム終了待機時間</summary>
        private const float GAME_END_WAIT_TIME_SECONDS = 1.0f;

        /// <summary>画面フェード時間</summary>
        private const float SCREEN_FADE_DURATION_SECONDS = 0.5f;

        /// <summary>画面フェードの最低待機時間</summary>
        private const float SCREEN_FADE_HOLD_TIME_SECONDS = 0.5f;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>シーンロード準備開始通知用 Subject</summary>
        private readonly Subject<string> _onLoadPrepareStart = new Subject<string>();

        /// <summary>シーンロード準備完了通知用 Subject</summary>
        private readonly Subject<Unit> _onLoadPrepareEnd = new Subject<Unit>();

        /// <summary>画面フェード実行通知用 Subject</summary>
        private readonly Subject<float> _onFadeStarted = new Subject<float>();

        /// <summary>シーン変更通知用 Subject</summary>
        private readonly Subject<float> _onSceneChanged = new Subject<float>();

        /// <summary>現在のフェーズ</summary>
        public IReadOnlyReactiveProperty<PhaseType> CurrentPhase => _phaseMachine.CurrentPhaseType;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            // フレームレート設定
            UnityEngine.Application.targetFrameRate = TARGET_FRAME_RATE;

            // コンポーネント配列初期化
            _components = new GameObject[transform.childCount];

            for (int i = 0; i < _components.Length; i++)
            {
                // 指定インデックスの子 Transform 取得
                Transform child = transform.GetChild(i);

                // GameObject として配列に格納
                _components[i] = child.gameObject;
            }

            // シーン、フェーズ初期設定
            _currentScene = SceneManager.GetActiveScene().name;
            _targetScene = _currentScene;
            _targetPhase = _startPhase;
            _isAfterSceneChange = true;
        }

        private void Start()
        {
            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _soundManager = SoundManager.Instance;

            if (_gameOptionManager == null ||
                _soundManager == null)
            {
                Debug.LogError("[GameManager] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit();
#endif

                return;
            }

            // --------------------------------------------------
            // Updatable 初期化
            // --------------------------------------------------
            // インスペクタから IUpdatable を収集
            IUpdatable[] updatables = _updatableCollector.Collect(_components);

            // コンテキスト作成
            _updatableContexts = new UpdatableContexts();

            // 書き込み専用として扱う
            IUpdatableWriter updatableWriter = _updatableContexts;

            // Attribute ベース自動登録
            _updatableAttributeScanner.RegisterFromAssembly(
                writer: updatableWriter,
                instances: updatables
            );

            // 列挙専用として扱う
            IUpdatableEnumerable updatableEnumerable = _updatableContexts;

            // Updatable の開始時処理
            _updatableLifecycleRunner.RunEnter(updatableEnumerable);

            // --------------------------------------------------
            // フェーズ管理初期化
            // --------------------------------------------------
            // フェーズ遷移設定
            _phaseTransitionConfig = new PhaseTransitionConfig(
                _gameOptionManager.PlayerCount,
                _gameOptionManager.TurnCount,
                _gameOptionManager.LimitTime
            );

            // フェーズ管理マシン初期化
            _phaseMachine = new PhaseMachine(
                _phaseTransitionConfig,
                _updatableContexts
            );

            // --------------------------------------------------
            // 非同期初期化処理
            // --------------------------------------------------
            InitializeAsync().Forget();
        }
        
        private void Update()
        {
            // --------------------------------------------------
            // 再起動判定
            // --------------------------------------------------
            // --------------------------------------------------
            // シーン遷移判定
            // --------------------------------------------------
            // シーン遷移が必要かつ未実行の場合のみ実行
            if (_currentScene != _targetScene && !_isSceneTransitioning)
            {
                // 非同期シーン遷移を開始する
                ChangeScene(_targetScene).Forget();

                // 多重実行防止フラグを有効化
                _isSceneTransitioning = true;

                return;
            }

            // シーン切替直後は 1 フレーム停止
            if (_isAfterSceneChange)
            {
                return;
            }

            // --------------------------------------------------
            // フェーズ処理
            // --------------------------------------------------
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            _phaseMachine?.OnUpdate(unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            // シーン切替直後は 1 フレーム停止
            if (_isAfterSceneChange)
            {
                _isAfterSceneChange = false;
                return;
            }

            // --------------------------------------------------
            // フェーズ処理
            // --------------------------------------------------
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            _phaseMachine?.OnLateUpdate(unscaledDeltaTime);

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------
            if (CurrentPhase.Value != _targetPhase)
            {
                RequestChangePhase(_targetPhase);
            }
        }

        private void OnDestroy()
        {
            // イベント購読解除
            Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // ゲーム
        // --------------------------------------------------
        /// <summary>
        /// 非同期初期化処理を実行する
        /// </summary>
        private async UniTask InitializeAsync()
        {
            // サウンド初期化
            await _soundManager.InitializeAudioAsync();

            // イベント購読
            Subscribe();

            // シーン変更後イベント
            TriggerSceneChangedEventAsync().Forget();
        }

        /// <summary>
        /// アプリケーションを終了する
        /// </summary>
        private async UniTask ExitGameAsync()
        {
            // 画面フェード開始イベント
            _onFadeStarted.OnNext(SCREEN_FADE_DURATION_SECONDS);

            await _eventRouter.OnFadeCompleted.ToUniTask(useFirstValue: true);

            // ゲーム終了待機時間
            await UniTask.Delay(TimeSpan.FromSeconds(GAME_END_WAIT_TIME_SECONDS), DelayType.UnscaledDeltaTime);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }

        /// <summary>
        /// アプリケーションを再起動する
        /// </summary>
        private async UniTask RestartGameAsync()
        {
            // 画面フェード開始イベント
            _onFadeStarted.OnNext(SCREEN_FADE_DURATION_SECONDS);

            await _eventRouter.OnFadeCompleted.ToUniTask(useFirstValue: true);

            // ゲーム終了待機時間
            await UniTask.Delay(TimeSpan.FromSeconds(GAME_END_WAIT_TIME_SECONDS), DelayType.UnscaledDeltaTime);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;

#else
            // 現在実行中のプロセス情報を取得
            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();

            // 実行ファイルパスを取得
            string executablePath = currentProcess.MainModule?.FileName;

            // 実行ファイルが取得できた場合のみ再起動する
            if (!string.IsNullOrEmpty(executablePath))
            {
                // 新しいプロセスとして起動
                System.Diagnostics.Process.Start(executablePath);
            }

            // 現在のアプリケーションを終了
            UnityEngine.Application.Quit();
#endif
        }

        // ---------------------------------------------
        // イベント購読
        // ---------------------------------------------
        /// <summary>
        /// イベント購読
        /// </summary>
        private void Subscribe()
        {
            // ---------------------------------------------
            // ルーター初期化
            // ---------------------------------------------
            // 読み込み専用として扱う
            IUpdatableReader updatableReader = _updatableContexts;

            // ルーター生成
            _eventRouter = new GameEventRouter(updatableReader, CurrentPhase);

            // イベント購読
            _eventRouter.Subscribe(_phaseMachine);
            _eventRouter.BindStreams(_onLoadPrepareStart, _onLoadPrepareEnd, _onSceneChanged, _onFadeStarted);

            // ---------------------------------------------
            // ルーター
            // ---------------------------------------------
            _eventRouter.OnSceneChangeRequested
                .Subscribe(scene => SetTargetScene(scene))
                .AddTo(_disposables);
            _eventRouter.OnPhaseChangeRequested
                .Subscribe(e => SetTargetPhase(e.NextPhaseType))
                .AddTo(_disposables);
            _eventRouter.OnExitGameRequested
                .Subscribe(_ => ExitGameAsync().Forget())
                .AddTo(_disposables);
            _eventRouter.OnRestartGameRequested
                .Subscribe(_ => RestartGameAsync().Forget())
                .AddTo(_disposables);

            // ---------------------------------------------
            // フェーズ
            // ---------------------------------------------
            _phaseMachine.CurrentPhaseType
                .Skip(1)
                .Subscribe(phase => SetTargetPhase(phase))
                .AddTo(_disposables);
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void Dispose()
        {
            _disposables?.Dispose();
            _onLoadPrepareStart?.Dispose();
            _onLoadPrepareEnd?.Dispose();
            _onFadeStarted?.Dispose();
            _onSceneChanged?.Dispose();

            _eventRouter?.Dispose();
        }
        
        // --------------------------------------------------
        // シーン
        // --------------------------------------------------
        /// <summary>
        /// 遷移先シーンを設定する
        /// </summary>
        private void SetTargetScene(in string nextScene)
        {
            // 同一シーンなら処理なし
            if (_targetScene == nextScene)
            {
                return;
            }

            _targetScene = nextScene;
        }

        /// <summary>
        /// シーン遷移フローを実行する
        /// </summary>
        private async UniTask ChangeScene(string nextScene)
        {
            if (string.IsNullOrEmpty(nextScene))
            {
                _isSceneTransitioning = false;

                return;
            }

            // --------------------------------------------------
            // ロード処理
            // --------------------------------------------------
            // ロード処理
            UniTask loadTask = BeginSceneLoad(nextScene);

            // 最低保証時間
            UniTask minWaitTask = UniTask.Delay(TimeSpan.FromSeconds(MIN_LOAD_TIME_SECONDS));

            // シーン切替完了イベント待機
            UniTask executeTask = _eventRouter.OnSceneChangeExecuted.ToUniTask(true);

            // ロード準備開始イベント
            _onLoadPrepareStart.OnNext(nextScene);

            // 完了まで待機
            await UniTask.WhenAll(
                loadTask,
                minWaitTask,
                executeTask
            );

            // ロード準備完了イベント
            _onLoadPrepareEnd.OnNext(Unit.Default);

            // --------------------------------------------------
            // 画面フェード処理
            // --------------------------------------------------
            // 画面フェード開始イベント
            _onFadeStarted.OnNext(SCREEN_FADE_DURATION_SECONDS);

            await _eventRouter.OnFadeCompleted.ToUniTask(useFirstValue: true);

            // フェード完了後待機時間
            await UniTask.Delay(TimeSpan.FromSeconds(SCREEN_FADE_HOLD_TIME_SECONDS), DelayType.UnscaledDeltaTime);

            // --------------------------------------------------
            // Updatable 終了処理
            // --------------------------------------------------
            // 列挙専用として扱う
            IUpdatableEnumerable updatableEnumerable = _updatableContexts;

            // 終了処理実行
            _updatableLifecycleRunner.RunExit(updatableEnumerable);

            // --------------------------------------------------
            // シーン遷移処理
            // --------------------------------------------------
            await CommitSceneLoad();
        }

        /// <summary>
        /// シーンのロードを開始する
        /// </summary>
        private async UniTask BeginSceneLoad(string nextScene)
        {
            _sceneLoader ??= new SceneLoader();

            // イベント購読
            _eventRouter.BindSceneLoadProgressStream(_sceneLoader.OnLoadProgress);

            await _sceneLoader.BeginLoadSceneAsync(nextScene);
        }

        /// <summary>
        /// ロード済みシーンを確定する
        /// </summary>
        private async UniTask CommitSceneLoad()
        {
            if (_sceneLoader == null)
            {
                return;
            }

            // イベント購読
            _eventRouter.UnbindSceneLoadProgressStream();

            await _sceneLoader.CommitSceneChangeAsync();
        }

        /// <summary>
        /// シーン遷移完了イベントを実行する
        /// </summary>
        private async UniTask TriggerSceneChangedEventAsync()
        {
            // フェード開始後待機時間
            await UniTask.Delay(TimeSpan.FromSeconds(SCREEN_FADE_HOLD_TIME_SECONDS), DelayType.UnscaledDeltaTime);

            // フェードアウト時間を通知
            _onSceneChanged.OnNext(SCREEN_FADE_DURATION_SECONDS);
        }

        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>
        /// 遷移先フェーズを設定する
        /// </summary>
        private void SetTargetPhase(in PhaseType nextPhase)
        {
            // 同一フェーズなら更新しない
            if (_targetPhase == nextPhase)
            {
                return;
            }

            // 遷移先フェーズを更新
            _targetPhase = nextPhase;
        }

        /// <summary>
        /// フェーズ遷移リクエストをする
        /// </summary>
        private void RequestChangePhase(in PhaseType nextPhase)
        {
            if (CurrentPhase.Value == nextPhase)
            {
                return;
            }

            _phaseMachine.RequestChangePhase(nextPhase);
        }
    }
}