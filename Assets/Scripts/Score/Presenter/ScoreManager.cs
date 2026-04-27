// ======================================================
// ScoreManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-03-08
// 概要     : スコア管理クラス
// ======================================================

using UnityEngine;
using UniRx;
using ScoreSystem.Application;
using ScoreSystem.Domain;

namespace ScoreSystem.Presentation
{
    /// <summary>
    /// ゲーム内スコアを管理するクラス
    /// </summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        // ======================================================
        // シングルトンインスタンス
        // ======================================================

        /// <summary>シングルトンインスタンス</summary>
        public static ScoreManager Instance { get; private set; }

        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("スコア設定")]
        /// <summary>最大スコア</summary>
        [SerializeField, Range(1, DEFAULT_MAX_SCORE)]
        private int _maxScore = 999999;

        /// <summary>駒 1 つあたりの基準スコア</summary>
        [SerializeField, Range(1, DEFAULT_MAX_SCORE)]
        private int _baseScore = 10;

        /// <summary>連続ライン成立時のボーナス倍率</summary>
        [SerializeField, Range(0f, 1f)]
        private float _lineComleteChainBonus = 0.1f;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>スコア計算サービス</summary>
        private ScoreCalculateService _calculateService;

        /// <summary>プレイヤースコア</summary>
        private ScoreData[] _playerScores;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>スコア上限のデフォルト値</summary>
        private const int DEFAULT_MAX_SCORE = 999999;
        
        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>
        /// プレイヤー別累計スコア
        /// </summary>
        private ReactiveProperty<int>[] _totalScores;

        /// <summary>
        /// プレイヤー別累計スコア
        /// </summary>
        public IReadOnlyReactiveProperty<int> GetTotalScore(int index)
        {
            if (_totalScores == null ||
                index < 0 ||
                index >= _totalScores.Length)
            {
                return null;
            }
            
            return _totalScores[index];
        }

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            Dispose();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// スコアマネージャーの初期化
        /// </summary>
        public void Initialize(int playerCount)
        {
            // 未生成
            if (_totalScores == null || _playerScores == null)
            {
                CreateScoresInternal(playerCount);
                return;
            }

            // プレイヤー数変更
            if (_totalScores.Length != playerCount)
            {
                Dispose();
                CreateScoresInternal(playerCount);
                return;
            }

            ResetScoresInternal();
        }

        /// <summary>
        /// ライン成立スコア加算
        /// </summary>
        public void AddLineScore(int playerId, int lineLength)
        {
            if (_totalScores == null)
            {
                return;
            }

            if (playerId < 0 || playerId >= _playerScores.Length)
            {
                return;
            }

            // サービスで累積スコア加算量を計算する
            int delta = _calculateService.AddCumulativeScore(
                ref _playerScores[playerId],
                _baseScore * lineLength,
                _lineComleteChainBonus
            );

            // 対象プレイヤーのスコアへ反映し通知する
            ApplyScore(playerId, delta);
        }

        /// <summary>
        /// 全プレイヤーの累積カウンターを加算する
        /// </summary>
        public void AddAllCumulativeCount()
        {
            if (_playerScores == null)
            {
                return;
            }

            // 全プレイヤーのカウントを進める
            for (int i = 0; i < _playerScores.Length; i++)
            {
                _calculateService.AddCumulativeCount(ref _playerScores[i]);
            }
        }

        /// <summary>
        /// 全プレイヤーの累積カウンターをリセットする
        /// </summary>
        public void ResetAllCumulativeCount()
        {
            if (_playerScores == null)
            {
                return;
            }

            // 全プレイヤーの累積カウントを初期化
            for (int i = 0; i < _playerScores.Length; i++)
            {
                _calculateService.ResetAddCounter(ref _playerScores[i]);
            }
        }

        /// <summary>
        /// スコアをリセットする
        /// </summary>
        public void ResetTotalScore()
        {
            if (_totalScores == null)
            {
                return;
            }

            // 全プレイヤーのスコア初期化
            for (int i = 0; i < _totalScores.Length; i++)
            {
                _totalScores[i].Value = 0;

                _calculateService.ResetScore(ref _playerScores[i]);
            }
        }

        /// <summary>
        /// スコア関連リソースを破棄する
        /// </summary>
        private void Dispose()
        {
            if (_totalScores != null)
            {
                for (int i = 0; i < _totalScores.Length; i++)
                {
                    _totalScores[i]?.Dispose();
                }
            }

            _totalScores = null;
            _playerScores = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 初回スコア生成
        /// </summary>
        private void CreateScoresInternal(int playerCount)
        {
            // プレイヤー数分のスコアを確保
            _totalScores = new ReactiveProperty<int>[playerCount];
            _playerScores = new ScoreData[playerCount];

            // スコア計算サービスを生成
            _calculateService = new ScoreCalculateService(_maxScore);

            for (int i = 0; i < playerCount; i++)
            {
                _totalScores[i] = new ReactiveProperty<int>(0);
                _playerScores[i] = new ScoreData();
            }
        }

        /// <summary>
        /// スコア再初期化
        /// </summary>
        private void ResetScoresInternal()
        {
            ResetTotalScore();
            ResetAllCumulativeCount();
        }

        /// <summary>
        /// スコア加算を総スコアへ反映し通知する
        /// </summary>
        private void ApplyScore(int playerIndex, int delta)
        {
            if (_totalScores == null)
            {
                return;
            }

            if (playerIndex < 0 || playerIndex >= _totalScores.Length)
            {
                return;
            }
            
            if (delta == 0)
            {
                return;
            }

            // 加算前スコアを保持
            int previousScore = _totalScores[playerIndex].Value;

            // 新しいスコアを計算
            int nextScore = previousScore + delta;

            // 上限制御
            if (nextScore > _maxScore)
            {
                nextScore = _maxScore;
            }

            // 確定値を代入
            _totalScores[playerIndex].Value = nextScore;
        }
    }
}