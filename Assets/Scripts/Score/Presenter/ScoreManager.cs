// ======================================================
// ScoreManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-03-08
// 概要     : スコア管理クラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using UniRx;
using ScoreSystem.Application;
using ScoreSystem.Domain;
using Unity.Collections.LowLevel.Unsafe;

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

        [Header("スコア基準設定")]
        /// <summary>最大スコア</summary>
        [SerializeField, Range(1, DEFAULT_MAX_SCORE)]
        private int _maxScore = 999999;

        /// <summary>駒 1 つあたりの基準スコア</summary>
        [SerializeField, Range(1, DEFAULT_MAX_SCORE)]
        private int _baseScore = 10;

        [Header("スコア加算設定")]
        /// <summary>累積ボーナス計算方式</summary>
        [SerializeField]
        private ScoreCalculateService.BonusCalcType _bonusType;

        /// <summary>連続ライン成立時のボーナス倍率</summary>
        [SerializeField, Range(0f, INSPECTOR_MAX_BONUS)]
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

        /// <summary>連続ライン成立時ボーナス倍率の上限値</summary>
        private const float INSPECTOR_MAX_BONUS = 100f;

        /// <summary>スコア倍率計算の基準となるライン長</summary>
        private const int LINE_LENGTH_BASE = 3;

        /// <summary>ライン長に応じたスコア倍率を指数的に増加させるための基準値</summary>
        private const int LINE_LENGTH_SCALE_BASE = 2;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>プレイヤー別累計スコア配列</summary>
        private ReactiveProperty<int>[] _totalScores;

        /// <summary>累積カウンター</summary>
        private ReactiveProperty<int> _cumulativeCount = new ReactiveProperty<int>(0);

        /// <summary>累積カウンター</summary>
        public IReadOnlyReactiveProperty<int> CumulativeCount => _cumulativeCount;

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

            // インデックス変換
            if (!TryConvertPlayerIndex(playerId, out int playerIndex))
            {
                return;
            }

            // ライン長をスケーリング値に変換
            int scaledLineLength = ConvertLineLengthToScale(lineLength);

            // 累積スコア加算量を計算する
            int delta = _calculateService.AddCumulativeScore(
                ref _playerScores[playerIndex],
                _baseScore * scaledLineLength,
                _lineComleteChainBonus
            );

            // 対象プレイヤーのスコアへ反映し通知する
            ApplyScore(playerIndex, delta);
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

            // 累積カウンターを進める
            _cumulativeCount.Value++;

            // 全プレイヤーへ適用する
            for (int i = 0; i < _playerScores.Length; i++)
            {
                _calculateService.ApplyCumulativeCount(
                    ref _playerScores[i],
                    _cumulativeCount.Value);
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

            // 累積カウンターをリセット
            _cumulativeCount.Value = 0;

            // 全プレイヤーの累積カウントを初期化
            for (int i = 0; i < _playerScores.Length; i++)
            {
                _calculateService.ApplyCumulativeCount(ref _playerScores[i], _cumulativeCount.Value);
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

        /// <summary>
        /// プレイヤー別累計スコアを取得する
        /// </summary>
        public IReadOnlyReactiveProperty<int> GetTotalScore(int playerId)
        {
            if (_totalScores == null)
            {
                return null;
            }

            // インデックス変換
            if (!TryConvertPlayerIndex(playerId, out int playerIndex))
            {
                return null;
            }

            return _totalScores[playerIndex];
        }

        /// <summary>
        /// スコア順に並んだプレイヤーIDランキングを取得する
        /// </summary>
        /// <returns>
        /// 上位から順にランキング情報を格納したリスト
        /// </returns>
        public List<RankingData> GetRanking()
        {
            if (_totalScores == null)
            {
                return new List<RankingData>();
            }

            List<RankingData> result =
                new List<RankingData>();

            for (int i = 0; i < _totalScores.Length; i++)
            {
                // 1 ベースへ変換
                int playerId = i + 1;

                // スコア取得
                int score = _totalScores[i].Value;

                result.Add(new RankingData(playerId, score));
            }

            result.Sort((a, b) =>
            {
                // スコア比較
                int scoreCompare = b.Score.CompareTo(a.Score);

                // スコアが異なる場合はその結果を返す
                if (scoreCompare != 0)
                {
                    return scoreCompare;
                }

                // スコアが同じ場合はプレイヤー ID 昇順
                return a.PlayerId.CompareTo(b.PlayerId);
            });

            return result;
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
            _calculateService = new ScoreCalculateService(_maxScore, _bonusType);

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
        /// 1 ベースの ID を 0 ベースのインデックスへ変換する
        /// </summary>
        /// <param name="playerId">プレイヤー ID</param>
        /// <param name="playerIndex">プレイヤーインデックス</param>
        /// <returns>変換成功時:true / 範囲外:false</returns>
        private bool TryConvertPlayerIndex(int playerId, out int playerIndex)
        {
            // 1 ベース ID を0 ベースインデックスへ変換
            playerIndex = playerId - 1;

            // スコア配列未生成の場合は失敗
            if (_totalScores == null)
            {
                return false;
            }

            // 範囲外チェック
            if (playerIndex < 0 || playerIndex >= _totalScores.Length)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// ライン長をスケーリング値に変換する
        /// </summary>
        private int ConvertLineLengthToScale(int lineLength)
        {
            // 基準値未満は最低倍率を返す
            if (lineLength <= LINE_LENGTH_BASE)
            {
                return 1;
            }

            // 指数計算によりライン長をスケーリング値へ変換
            int scaledLineLength = (int)Mathf.Pow(LINE_LENGTH_SCALE_BASE, lineLength - LINE_LENGTH_BASE);

            return scaledLineLength;
        }

        /// <summary>
        /// スコア加算を総スコアへ反映し通知する
        /// </summary>
        /// <param name="playerIndex">プレイヤー Index</param>
        /// <param name="delta">スコア加算量</param>
        private void ApplyScore(int playerIndex, int delta)
        {
            if (_totalScores == null)
            {
                return;
            }

            // 加算値が 0 なら処理なし
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

            _totalScores[playerIndex].Value = nextScore;
        }
    }
}