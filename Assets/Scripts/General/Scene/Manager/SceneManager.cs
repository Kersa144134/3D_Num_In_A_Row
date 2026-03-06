// ======================================================
// SceneManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-01-23
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using UnityEngine;
using SceneSystem.Controller;
using SceneSystem.Data;
using SceneSystem.Utility;

namespace SceneSystem.Manager
{
    /// <summary>
    /// シーン遷移・フェーズ遷移・Update 実行を統括する
    /// </summary>
    public sealed class SceneManager : MonoBehaviour
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("IUpdatable 保持オブジェクト")]
        /// <summary>IUpdatable を保持している GameObject 群</summary>
        [SerializeField] private GameObject[] _components;

        [Header("初期フェーズ")]
        /// <summary>シーン読み込み時の初期フェーズ</summary>
        [SerializeField] private PhaseType _startPhase;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ遷移条件管理クラス</summary>
        private PhaseManager _phaseManager = new PhaseManager();

        /// <summary>フェーズ切替制御クラス</summary>
        private PhaseController _phaseController;

        /// <summary>フェーズの初期化を行うクラス</summary>
        private PhaseInitializer _phaseInitializer = new PhaseInitializer();

        /// <summary>Update を管理するクラス</summary>
        private UpdateManager _updateManager;

        /// <summary>Update を実行する対象を保持し、実行するクラス</summary>
        private UpdateController _updateController = new UpdateController();

        /// <summary>IUpdatable を実装しているコンポーネントを取得するクラス</summary>
        private UpdatableCollector _updatableCollector = new UpdatableCollector();

        /// <summary>IUpdatable の初期化を行うクラス</summary>
        private UpdatableInitializer _initializer;

        /// <summary>シーン内イベントを仲介するクラス</summary>
        private SceneEventRouter _sceneEventRouter;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // シーン管理
        // --------------------------------------------------
        /// <summary>現在のシーン名</summary>
        private string _currentScene = string.Empty;

        /// <summary>遷移先シーン名</summary>
        private string _targetScene = string.Empty;

        /// <summary>シーン切り替え直後かどうかを示すフラグ</summary>
        private bool _isSceneChanged = true;

        // --------------------------------------------------
        // フェーズ管理
        // --------------------------------------------------
        /// <summary>現在のフェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        /// <summary>遷移先フェーズ/summary>
        private PhaseType _targetPhase = PhaseType.None;

        /// <summary>ゲームの経過時間</summary>
        private float _elapsedTime = 0.0f;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>アプリケーション全体で固定する目標 FPS</summary>
        private const int TARGET_FRAME_RATE = 120;
        
        /// <summary>PhaseData を配置している Resources フォルダパス</summary>
        private const string PHASE_DATA_RESOURCES_PATH = "Phase";

        // ======================================================
        // Unityイベント
        // ======================================================

        private void Awake()
        {
            // フレームレート固定
            Application.targetFrameRate = TARGET_FRAME_RATE;

            // 初期状態設定
            _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            _targetScene = _currentScene;
            _targetPhase = _startPhase;
            _isSceneChanged = true;
            _elapsedTime = 0.0f;

            // フェーズデータ読み込み
            PhaseData[] phaseDataList = Resources.LoadAll<PhaseData>(PHASE_DATA_RESOURCES_PATH);

            // IUpdatable 初期化
            _updatableCollector = new UpdatableCollector();
            _initializer = new UpdatableInitializer(_updatableCollector);

            // Context 作成
            UpdatableContext context = _initializer.Initialize(_components);

            // Context から取得
            IUpdatable[] allUpdatables = context.Updatables;

            // Update 制御初期化
            _phaseController = new PhaseController(_updateController);
            _updateManager = new UpdateManager(_updateController);

            // フェーズごと登録
            _phaseInitializer.Initialize(_phaseController, allUpdatables, phaseDataList);

            // シーンイベント初期化
            _sceneEventRouter = new SceneEventRouter(context);
            _sceneEventRouter.Subscribe();
            _phaseManager.OnOptionButtonPressed += HandleOptionButtonPressed;
            _sceneEventRouter.OnPhaseChanged += SetTargetPhase;
        }

        private void Update()
        {
            // シーン遷移判定
            if (_currentScene != _targetScene)
            {
                ChangeScene(_targetScene);
                return;
            }

            // フェーズ遷移判定
            if (_currentPhase != _targetPhase)
            {
                ChangePhase(_targetPhase);
            }

            // シーン切り替え直後のフレーム判定
            if (_isSceneChanged)
            {
                return;
            }

            float unscaledDeltaTime = Time.unscaledDeltaTime;

            // Play フェーズ中のみタイマーを進行
            if (_currentPhase == PhaseType.Play)
            {
                // timeScaleに影響されない経過時間で加算
                _elapsedTime += unscaledDeltaTime;
            }

            // Update 実行
            _updateManager.Update(unscaledDeltaTime, _elapsedTime);
        }

        private void LateUpdate()
        {
            // シーン切り替え直後のフレームスキップ判定
            if (_isSceneChanged)
            {
                _isSceneChanged = false;
                return;
            }

            float unscaledDeltaTime = Time.unscaledDeltaTime;

            // Play フェーズ中のみタイマー表示更新
            if (_currentPhase == PhaseType.Play)
            {
                float limitTime = PhaseManager.PLAY_TO_FINISH_WAIT_TIME;
                _sceneEventRouter.UpdateLimitTimeDisplay(_elapsedTime, limitTime);
            }
            
            // LateUpdate 実行
            _updateManager.LateUpdate(unscaledDeltaTime);

            // フェーズ遷移判定
            if (_currentPhase == _targetPhase)
            {
                _phaseManager.Update(
                    unscaledDeltaTime,
                    _elapsedTime,
                    _currentScene,
                    _currentPhase,
                    out _targetScene,
                    out _targetPhase
                );
            }
        }

        private void OnDestroy()
        {
            // イベント購読解除
            _sceneEventRouter.Dispose();
            _phaseManager.OnOptionButtonPressed -= HandleOptionButtonPressed;
            _sceneEventRouter.OnPhaseChanged -= SetTargetPhase;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 外部から遷移先フェーズを設定する
        /// </summary>
        public void SetTargetPhase(PhaseType nextPhase)
        {
            // 遷移先フェーズを更新
            _targetPhase = nextPhase;
        }
        
        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// シーン遷移を行う
        /// </summary>
        private void ChangeScene(in string sceneName)
        {
            // 無効なシーン名なら処理なし
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            // 現在シーンを更新
            _currentScene = sceneName;

            _isSceneChanged = true;

            // シーンロード
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// フェーズ切替を行う
        /// </summary>
        public void ChangePhase(in PhaseType nextPhase)
        {
            if (_currentPhase == nextPhase)
            {
                return;
            }

            // UpdateController をリセット
            _updateController.Clear();

            // PhaseInitializer に登録された Updatable を PhaseController に通知
            if (_phaseInitializer.TryGetUpdatablesForPhase(nextPhase, out IUpdatable[] updatables))
            {
                // PhaseController に登録して UpdateController に追加
                _phaseController.AssignPhaseUpdatables(nextPhase, updatables);
            }

            _updateManager.ChangePhase(nextPhase);

            // 現在フェーズ更新
            _currentPhase = nextPhase;
        }

        /// <summary>
        /// オプションボタン押下時の処理を行うハンドラ
        /// </summary>
        private void HandleOptionButtonPressed()
        {
            _sceneEventRouter.HandleOptionButtonPressed();
        }
    }
}