// ======================================================
// GameOptionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-27
// 更新日時 : 2026-04-27
// 概要     : ゲームオプション制御・適用サービス
// ======================================================

using UnityEngine;
using OptionSystem.Domain;
using OptionSystem.Infrastructure;

namespace OptionSystem.Application
{
    /// <summary>
    /// ゲーム設定の制御クラス
    /// </summary>
    public sealed class GameOptionService : MonoBehaviour
    {
        // ======================================================
        // シングルトンインスタンス
        // ======================================================

        /// <summary>
        /// インスタンス
        /// </summary>
        public static GameOptionService Instance { get; private set; }

        // ======================================================
        // インスペクタ設定
        // ======================================================

        // --------------------------------------------------
        // プレイヤー
        // --------------------------------------------------
        [Header("プレイヤー人数")]
        [SerializeField, Range(2, 4)]
        private int _playerCount = 2;

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        [Header("制限時間")]
        [SerializeField, Range(0f, 60f)]
        private float _limitTime = 15f;

        // --------------------------------------------------
        // ボード
        // --------------------------------------------------
        [Header("盤面サイズ")]
        [SerializeField]
        private GameRules.BoardSizeType _boardSizeType;

        [Header("ライン成立条件")]
        [SerializeField, Range(3, 5)]
        private int _connectCount = 3;

        // --------------------------------------------------
        // 操作
        // --------------------------------------------------
        [Header("カメラ回転速度")]
        [SerializeField, Range(180f, 1800f)]
        private float _cameraRotationSpeed = 360f;

        [Header("ポインター感度")]
        [SerializeField, Range(500f, 5000f)]
        private float _pointerSpeed = 1000f;
        
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// ゲーム設定の永続化を担当するリポジトリ
        /// </summary>
        private IGameOptionRepository _repository;

        /// <summary>
        /// 現在のゲームルール
        /// </summary>
        private GameRules _currentRules;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// プレイヤー人数
        /// </summary>
        public int PlayerCount => _currentRules.PlayerCount;

        /// <summary>
        /// 1 プレイヤーあたりの制限時間
        /// </summary>
        public float LimitTime => _currentRules.PerPlayerLimitTime;

        /// <summary>
        /// 盤面サイズ
        /// int に変換
        /// </summary>
        public int BoardSize => (int)_currentRules.BoardSize;

        /// <summary>
        /// ライン成立条件
        /// </summary>
        public int ConnectCount => _currentRules.ConnectCount;

        /// <summary>
        /// カメラ回転速度
        /// </summary>
        public float CameraRotationSpeed => _currentRules.CameraRotationSpeed;

        /// <summary>
        /// ポインター速度
        /// </summary>
        public float PointerSpeed => _currentRules.PointerSpeed;

        // ======================================================
        // Unity イベント
        // ======================================================

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            // リポジトリ初期化
            _repository = new PlayerPrefsGameOptionRepository();

            // インスペクタ設定値で初期化
            _currentRules = new GameRules
            {
                PlayerCount = _playerCount,
                PerPlayerLimitTime = _limitTime,
                BoardSize = _boardSizeType,
                ConnectCount = _connectCount,
                CameraRotationSpeed = _cameraRotationSpeed,
                PointerSpeed = _pointerSpeed
            };

            // セーブデータがあるか
            if (_repository.Exists())
            {
                _currentRules = _repository.Load();
            }
            else
            {
                _repository.Save(_currentRules);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ゲームルールを更新する
        /// </summary>
        public void UpdateRules(GameRules rules)
        {
            _currentRules = rules;
        }

        /// <summary>
        /// 現在のゲームルールを保存する
        /// </summary>
        public void Save()
        {
            _repository.Save(_currentRules);
        }

        /// <summary>
        /// 保存されているゲームルールを読み込む
        /// </summary>
        public void Load()
        {
            _currentRules = _repository.Load();
        }
    }
}