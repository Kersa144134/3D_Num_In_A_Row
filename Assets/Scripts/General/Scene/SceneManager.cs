// ======================================================
// SceneManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-01-23
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using UniRx;
using PhaseSystem.Data;
using PhaseSystem.Manager;
using PhaseSystem.Utility;
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
        private readonly PhaseManager _phaseManager = new();

        /// <summary>フェーズの初期化を行うクラス</summary>
        private readonly PhaseInitializer _phaseInitializer = new();

        /// <summary>Update を管理するクラス</summary>
        private UpdateManager _updateManager;

        /// <summary>Update を実行する対象を保持し、実行するクラス</summary>
        private readonly UpdateController _updateController = new();

        /// <summary>IUpdatable を実装しているコンポーネントを取得するクラス</summary>
        private readonly UpdatableCollector _updatableCollector = new();

        /// <summary>IUpdatable の初期化を行うクラス</summary>
        private readonly UpdatableInitializer _updatableInitializer = new();

        /// <summary>シーン内イベントを仲介するクラス</summary>
        private SceneEventRouter _sceneEventRouter;

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
        private bool _isSceneChanged = true;

        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>現在のフェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        /// <summary>遷移先フェーズ/summary>
        private PhaseType _targetPhase = PhaseType.None;

        // --------------------------------------------------
        // Updatables
        // --------------------------------------------------
        /// <summary>Updatable を保持するコンテキスト</summary>
        private UpdatableContext _updatableContexts;

        /// <summary>フェーズごとの IUpdatable 配列を保持する辞書</summary>
        private Dictionary<PhaseType, IUpdatable[]> _phaseUpdatablesMap;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>アプリケーション全体で固定する目標 FPS</summary>
        private const int TARGET_FRAME_RATE = 120;
        
        /// <summary>PhaseData を配置している Resources フォルダパス</summary>
        private const string PHASE_DATA_RESOURCES_PATH = "Phase";

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposables =
            new CompositeDisposable();

        // ======================================================
        // Unityイベント
        // ======================================================

        private void Awake()
        {
            // フレームレート固定
            Application.targetFrameRate = TARGET_FRAME_RATE;
        }

        private void Start()
        {
            // フレームレート固定
            Application.targetFrameRate = TARGET_FRAME_RATE;

            // 初期状態設定
            _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            _targetScene = _currentScene;
            _targetPhase = _startPhase;
            _isSceneChanged = true;

            // フェーズデータ読み込み
            PhaseData[] phaseDataList = Resources.LoadAll<PhaseData>(PHASE_DATA_RESOURCES_PATH);

            // インスペクタから IUpdatable を収集
            IUpdatable[] updatables = _updatableCollector.Collect(_components);

            // コンテキスト作成
            _updatableContexts = _updatableInitializer.InitializeUpdatables(updatables);

            // コンテキストから取得
            IUpdatable[] allUpdatables = _updatableContexts.Updatables;

            // フェーズごと登録
            _phaseUpdatablesMap = _phaseInitializer.CreatePhaseMap(allUpdatables, phaseDataList);

            // コンポーネント初期化
            _updateManager = new UpdateManager(_updateController, _phaseUpdatablesMap);
            _sceneEventRouter = new SceneEventRouter(_updatableContexts);

            // イベント購読
            _sceneEventRouter.OnPhaseChanged
                .Subscribe(phase =>
                {
                    SetTargetPhase(phase);
                })
                .AddTo(_disposables);

            _sceneEventRouter.Subscribe();
        }

        private void Update()
        {
            // シーン遷移
            if (_currentScene != _targetScene)
            {
                ChangeScene(_targetScene);
                return;
            }

            // フェーズ遷移
            if (_currentPhase != _targetPhase)
            {
                ChangePhase(_targetPhase);
            }

            // シーン切り替え直後のフレームスキップ判定
            if (_isSceneChanged)
            {
                return;
            }

            float unscaledDeltaTime = Time.unscaledDeltaTime;

            // Update 実行
            _updateManager.Update(unscaledDeltaTime, _phaseManager.GamePlayElapsedTime);
        }

        private void LateUpdate()
        {
            // シーン切り替え直後のフレームスキップ判定
            if (_isSceneChanged)
            {
                _isSceneChanged = false;
                return;
            }

            // Play フェーズ中のみタイマー表示更新
            if (_currentPhase == PhaseType.Play)
            {
                float limitTime = PhaseManager.PLAY_TO_FINISH_WAIT_TIME;
                _sceneEventRouter.HandleLimitTimeUpdated(_phaseManager.GamePlayElapsedTime, limitTime);
            }

            float unscaledDeltaTime = Time.unscaledDeltaTime;

            // LateUpdate 実行
            _updateManager.LateUpdate(unscaledDeltaTime);

            // フェーズ遷移判定実行
            if (_currentPhase == _targetPhase)
            {
                _phaseManager.Update(
                    unscaledDeltaTime,
                    _currentPhase,
                    out _targetPhase
                );
            }
        }

        private void OnDestroy()
        {
            // イベント購読解除
            _disposables.Dispose();
            _sceneEventRouter.Dispose();
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

            // Updatable の終了時処理
            _updatableInitializer.FinalizeUpdatables(_updatableContexts);

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

            // フェーズ変更時処理
            _updateManager.ChangePhase(nextPhase);

            // 現在フェーズ更新
            _currentPhase = nextPhase;
        }
    }
}