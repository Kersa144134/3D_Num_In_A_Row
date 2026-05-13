// ======================================================
// GameOptionManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-27
// 更新日時 : 2026-04-27
// 概要     : ゲームオプション制御・適用クラス
// ======================================================

using UnityEngine;
using OptionSystem.Domain;
using OptionSystem.Infrastructure;

namespace OptionSystem.Presentation
{
    /// <summary>
    /// ゲーム設定の制御クラス
    /// </summary>
    public sealed class GameOptionManager : MonoBehaviour
    {
        // ======================================================
        // シングルトンインスタンス
        // ======================================================

        /// <summary>
        /// インスタンス
        /// </summary>
        public static GameOptionManager Instance { get; private set; }

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
        /// 公開時に int へ変換する
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

            // リポジトリ初期化
            _repository = new PlayerPrefsGameOptionRepository();

            // デバッグ用
            _repository.Delete();

            // オプションセーブデータがあるか
            if (_repository.HasSavedData())
            {
                Load();
            }
            else
            {
                Save();
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // セーブ・ロード
        // --------------------------------------------------
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

        // --------------------------------------------------
        // 更新
        // --------------------------------------------------
        /// <summary>
        /// プレイヤー人数を更新する
        /// </summary>
        /// <param name="playerCount">プレイヤー人数</param>
        public void SetPlayerCount(in int playerCount)
        {
            _currentRules.PlayerCount = playerCount;
        }

        /// <summary>
        /// 制限時間を更新する
        /// </summary>
        /// <param name="limitTime">制限時間</param>
        public void SetLimitTime(in float limitTime)
        {
            _currentRules.PerPlayerLimitTime = limitTime;
        }

        /// <summary>
        /// 盤面サイズを更新する
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        public void SetBoardSize(
            in GameRules.BoardSizeType boardSize)
        {
            _currentRules.BoardSize = boardSize;
        }

        /// <summary>
        /// ライン成立条件を更新する
        /// </summary>
        /// <param name="connectCount">ライン成立条件</param>
        public void SetConnectCount(in int connectCount)
        {
            _currentRules.ConnectCount = connectCount;
        }

        /// <summary>
        /// カメラ回転速度を更新する
        /// </summary>
        /// <param name="speed">回転速度</param>
        public void SetCameraRotationSpeed(in float speed)
        {
            _currentRules.CameraRotationSpeed = speed;
        }

        /// <summary>
        /// ポインター速度を更新する
        /// </summary>
        /// <param name="speed">ポインター速度</param>
        public void SetPointerSpeed(in float speed)
        {
            _currentRules.PointerSpeed = speed;
        }
    }
}