// ======================================================
// GameManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-04-08
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using UnityEngine;
using UniRx;
using OptionSystem.Application;
using PhaseSystem.Application;
using PhaseSystem.Domain;
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

        [Header("フェーズ遷移設定")]
        /// <summary>Ready フェーズから Play フェーズへ遷移するまでの時間（秒）</summary>
        [SerializeField, Min(0f)] private float _readyToChangePlayerWaitTime = 3.0f;

        /// <summary>Play フェーズから Finish フェーズへ遷移するまでの時間（秒）</summary>
        [SerializeField, Min(0f)] private float _playToFinishWaitTime = 120.0f;

        /// <summary>ChangePlayer フェーズから Play フェーズへ遷移するまでの時間（秒）</summary>
        [SerializeField, Min(0f)] private float _changePlayerToPlayWaitTime = 2.0f;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ管理マシン</summary>
        private PhaseMachine _phaseMachine;

        /// <summary>フェーズ遷移およびプレイ進行設定</summary>
        private PhaseTransitionConfig _phaseTransitionConfig;

        /// <summary>UpdatableBindAttribute を走査し UpdatableContexts へ自動登録を行うクラス</summary>
        private readonly UpdatableAttributeScanner _updatableAttributeScanner = new UpdatableAttributeScanner();

        /// <summary>IUpdatable を実装しているコンポーネントを取得するクラス</summary>
        private readonly UpdatableCollector _updatableCollector = new UpdatableCollector();

        /// <summary>IUpdatable を実装しているコンポーネントを取得するクラス</summary>
        private readonly UpdatableContextFactory _updatableContextFactory = new UpdatableContextFactory();

        /// <summary>IUpdatable のライフサイクル実行クラス</summary>
        private readonly UpdatableLifecycleRunner _updatableLifecycleRunner = new UpdatableLifecycleRunner();

        /// <summary>シーン内イベントを仲介するクラス</summary>
        private GameEventRouter _eventRouter;

        /// <summary>GameOptionService キャッシュ</summary>
        private GameOptionService _gameOptionService;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // オプション
        // --------------------------------------------------
        /// <summary>プレイヤー人数</summary>
        private int _playerCount;

        /// <summary>1 プレイヤーあたりの制限時間</summary>
        private float _perPlayerLimitTime;

        // --------------------------------------------------
        // シーン
        // --------------------------------------------------
        /// <summary>現在のシーン名</summary>
        private string _currentScene = string.Empty;

        /// <summary>遷移先シーン名</summary>
        private string _targetScene = string.Empty;

        /// <summary>シーン切り替え直後かどうかを示すフラグ</summary>
        private bool _isSceneChanged = true;

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

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>現在のフェーズストリーム</summary>
        public IReadOnlyReactiveProperty<PhaseType> CurrentPhase => _phaseMachine.CurrentPhaseType;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            // フレームレート設定
            Application.targetFrameRate = TARGET_FRAME_RATE;

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
            _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            _targetScene = _currentScene;
            _isSceneChanged = true;
            _targetPhase = _startPhase;
        }

        private void Start()
        {
            // --------------------------------------------------
            // オプション読み込み
            // --------------------------------------------------
            // インスタンスからコンポーネント取得
            _gameOptionService = GameOptionService.Instance;

            if (_gameOptionService == null)
            {
                Debug.LogError("[GameManager] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            _playerCount = _gameOptionService.PlayerCount;
            _perPlayerLimitTime = _gameOptionService.LimitTime;
            
            // --------------------------------------------------
            // Updatable 初期化
            // --------------------------------------------------
            // インスペクタから IUpdatable を収集
            IUpdatable[] updatables = _updatableCollector.Collect(_components);

            // コンテキスト作成
            _updatableContexts = _updatableContextFactory.Create(updatables);

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
                _playerCount,
                _perPlayerLimitTime,
                _readyToChangePlayerWaitTime,
                _playToFinishWaitTime,
                _changePlayerToPlayWaitTime
            );

            // フェーズ管理マシン初期化
            _phaseMachine = new PhaseMachine(
                _startPhase,
                _phaseTransitionConfig,
                _updatableContexts
            );

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            IUpdatableReader updatableReader = _updatableContexts;
            _eventRouter = new GameEventRouter(updatableReader, CurrentPhase);
            
            _eventRouter.OnPhaseChanged
                .Subscribe(e =>
                {
                    SetTargetPhase(e.NextPhaseType);
                })
                .AddTo(_disposables);

            _phaseMachine.CurrentPhaseType
                .Skip(1)
                .Subscribe(e =>
                {
                    SetTargetPhase(e);
                })
                .AddTo(_disposables);

            _eventRouter.Subscribe(_phaseMachine);
        }

        private void Update()
        {
            // --------------------------------------------------
            // シーン遷移判定
            // --------------------------------------------------
            if (_currentScene != _targetScene)
            {
                ChangeScene(_targetScene);
                return;
            }

            // シーン切替直後は 1 フレーム停止
            if (_isSceneChanged)
            {
                return;
            }

            // --------------------------------------------------
            // フェーズ処理
            // --------------------------------------------------
            float unscaledDeltaTime = Time.unscaledDeltaTime;
            
            _phaseMachine.OnUpdate(unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            // シーン切替直後は 1 フレーム停止
            if (_isSceneChanged)
            {
                _isSceneChanged = false;
                return;
            }

            // --------------------------------------------------
            // フェーズ処理
            // --------------------------------------------------
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            _phaseMachine.OnLateUpdate(unscaledDeltaTime);

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------
            if (CurrentPhase.Value != _targetPhase)
            {
                ChangePhase(_targetPhase);
            }
        }

        private void OnDestroy()
        {
            // イベント購読解除
            _disposables.Dispose();
            _eventRouter.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// シーン遷移を行う
        /// </summary>
        private void ChangeScene(in string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            // 列挙専用として扱う
            IUpdatableEnumerable updatableEnumerable = _updatableContexts;

            // Updatable の終了時処理
            _updatableLifecycleRunner.RunExit(updatableEnumerable);

            // シーンロード
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// フェーズ遷移を行う
        /// </summary>
        private void ChangePhase(in PhaseType nextPhase)
        {
            // 同一フェーズでないなら実行しない
            if (CurrentPhase.Value == nextPhase)
            {
                return;
            }

            // PhaseMachine の遷移先フェーズを更新
            _phaseMachine.ChangePhase(nextPhase);
        }

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
    }
}