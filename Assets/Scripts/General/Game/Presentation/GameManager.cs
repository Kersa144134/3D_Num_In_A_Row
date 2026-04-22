// ======================================================
// GameManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-04-08
// 概要     : シーン遷移、フェーズ管理、Update 管理を統括する
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using UniRx;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using SceneSystem.Application;
using SceneSystem.Domain;

namespace SceneSystem.Presentation
{
    /// <summary>
    /// シーン遷移・フェーズ遷移・Update 実行を統括する
    /// </summary>
    public sealed class GameManager : MonoBehaviour
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("ゲーム設定")]
        /// <summary>プレイヤー人数</summary>
        [SerializeField] private int _playerCount;

        /// <summary>1 プレイヤーあたりの制限時間</summary>
        [SerializeField, Min(0f)]
        private float _perPlayerLimitTime = 15.0f;

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

        /// <summary>Update を管理するサービス</summary>
        private UpdatableManagement _updatableManagement;

        /// <summary>IUpdatable を実装しているコンポーネントを取得するクラス</summary>
        private readonly UpdatableCollector _updatableCollector = new UpdatableCollector();

        /// <summary>IUpdatable の初期化を行うクラス</summary>
        private readonly UpdatableInitializer _updatableInitializer = new UpdatableInitializer();

        /// <summary>シーン内イベントを仲介するクラス</summary>
        private GameEventRouter _eventRouter;

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
        private readonly ReactiveProperty<PhaseType> _currentPhase =
            new ReactiveProperty<PhaseType>(PhaseType.None);

        /// <summary>遷移予定のフェーズ</summary>
        private PhaseType _targetPhase = PhaseType.None;

        // --------------------------------------------------
        // Updatables
        // --------------------------------------------------
        /// <summary>IUpdatable を保持している GameObject 群</summary>
        private GameObject[] _components;

        /// <summary>Updatable を保持するコンテキスト</summary>
        private UpdatableContext _updatableContexts;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>アプリケーション全体で固定する目標 FPS</summary>
        private const int TARGET_FRAME_RATE = 120;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposables =
            new CompositeDisposable();

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
            _currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            _targetScene = _currentScene;
            _isSceneChanged = true;
            _targetPhase = _startPhase;
        }

        private void Start()
        {
            // --------------------------------------------------
            // Updatable 初期化
            // --------------------------------------------------
            // インスペクタから IUpdatable を収集
            IUpdatable[] updatables = _updatableCollector.Collect(_components);

            // コンテキスト作成
            _updatableContexts = _updatableInitializer.InitializeUpdatables(updatables);

            // コンポーネント初期化
            _updatableManagement = new UpdatableManagement(updatables);
            _eventRouter = new GameEventRouter(_updatableContexts, _currentPhase);

            // --------------------------------------------------
            // フェーズ遷移マシン初期化
            // --------------------------------------------------
            // フェーズ遷移設定
            _phaseTransitionConfig = new PhaseTransitionConfig(
                _playerCount,
                _perPlayerLimitTime,
                _readyToChangePlayerWaitTime,
                _playToFinishWaitTime,
                _changePlayerToPlayWaitTime
            );

            // フェーズ遷移管理マシン生成
            _phaseMachine = new PhaseMachine(
                _startPhase,
                _phaseTransitionConfig
            );

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            _phaseMachine.OnPhaseChanged
                .Subscribe(e =>
                {
                    // 即時反映ではなくターゲットだけ更新
                    SetTargetPhase(e.NextPhaseType);
                })
                .AddTo(_disposables);

            _eventRouter.OnPhaseChanged
                .Subscribe(e =>
                {
                    // マシンだけ更新
                    SetPhaseMachine(e.NextPhaseType);
                })
                .AddTo(_disposables);

            _eventRouter.Subscribe(_phaseMachine);
        }

        private void Update()
        {
            // --------------------------------------------------
            // シーン遷移
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
            // Updatable 処理
            // --------------------------------------------------
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            _updatableManagement.Update(unscaledDeltaTime);

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------
            _phaseMachine.Update(unscaledDeltaTime);
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
            // Updatable 処理
            // --------------------------------------------------
            float unscaledDeltaTime = Time.unscaledDeltaTime;

            _updatableManagement.LateUpdate(unscaledDeltaTime);

            // --------------------------------------------------
            // フェーズ反映
            // --------------------------------------------------
            if (_currentPhase.Value != _targetPhase)
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

            // Updatable の終了時処理
            _updatableInitializer.FinalizeUpdatables(_updatableContexts);

            // シーンロード
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// フェーズ切替を行う
        /// </summary>
        private void ChangePhase(in PhaseType nextPhase)
        {
            if (_currentPhase.Value == nextPhase)
            {
                return;
            }

            // Updatable のフェーズ変更時処理
            _updatableManagement.ChangePhase(nextPhase);

            // 現在フェーズ更新
            _currentPhase.Value = nextPhase;
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
        
        /// <summary>
        /// 遷移先フェーズを設定する
        /// </summary>
        private void SetPhaseMachine(in PhaseType nextPhase)
        {
            // 同一フェーズなら更新しない
            if (_targetPhase == nextPhase)
            {
                return;
            }

            // PhaseMachine の遷移先フェーズを更新
            _phaseMachine.ChangePhase(nextPhase);
        }
    }
}