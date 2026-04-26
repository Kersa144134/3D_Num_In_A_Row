// ======================================================
// ScoreManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-03-08
// 概要     : スコア管理クラス
// ======================================================

using System;
using UnityEngine;
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
        // コンポーネント参照
        // ======================================================

        /// <summary>スコア計算サービス</summary>
        private ScoreCalculateService _calculateService;

        /// <summary>アイテム取得スコア</summary>
        private ScoreData _itemScore = new ScoreData();

        /// <summary>戦車撃破スコア</summary>
        private ScoreData _tankScore = new ScoreData();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>累計スコア</summary>
        private int _totalScore;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>累計スコア</summary>
        public int TotalScore => _totalScore;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>スコア最大値</summary>
        public const int SCORE_MAX = 99999999;

        /// <summary>アイテム取得時のスコア加算量</summary>
        private const int ITEM_SCORE = 10;

        /// <summary>戦車撃破時のスコア加算量</summary>
        private const int TANK_SCORE = 1000;

        /// <summary>アイテムボーナス指数係数</summary>
        private const double ITEM_BONUS_EXPONENT = 1.5;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// スコア変動通知イベント
        /// 引数は加算されたスコア量
        /// </summary>
        public event Action<int> OnScoreChanged;

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

            _calculateService = new ScoreCalculateService(SCORE_MAX);

            // 初期化
            _totalScore = 0;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// アイテム取得スコア加算
        /// </summary>
        public void AddItemScore()
        {
            // サービスでスコア加算量を計算する
            int delta = _calculateService.AddFixedScore(ref _itemScore, ITEM_SCORE);

            // 総スコアへ反映し通知する
            ApplyScore(delta);
        }

        /// <summary>
        /// アイテム取得ボーナススコア加算
        /// アイテム取得回数に応じたボーナススコアを加算する
        /// </summary>
        public void AddItemBonusScore()
        {
            int count = _itemScore.AddCount;

            if (count <= 0)
            {
                return;
            }

            // 加算前のスコア保存
            int previousScore = _totalScore;

            // ボーナス係数計算
            int multiplier = (int)Math.Pow(count, ITEM_BONUS_EXPONENT);

            // ボーナススコア算出
            int bonusScore = ITEM_SCORE * multiplier;

            // 総スコアへ加算
            _totalScore += bonusScore;

            // スコア上限補正
            if (_totalScore > SCORE_MAX)
            {
                _totalScore = SCORE_MAX;
            }

            // 実際の増加量算出
            int delta = _totalScore - previousScore;

            // スコア変動通知
            OnScoreChanged?.Invoke(delta);
        }

        /// <summary>
        /// 敵戦車撃破スコア加算
        /// </summary>
        public void AddTankScore()
        {
            // サービスで累積スコア加算量を計算する
            int delta = _calculateService.AddCumulativeScore(ref _tankScore, TANK_SCORE);

            // 総スコアへ反映し通知する
            ApplyScore(delta);
        }

        /// <summary>
        /// スコアをリセットする
        /// シーン開始時などに呼び出す
        /// </summary>
        public void ResetScore()
        {
            // スコア初期化
            _totalScore = 0;
            _calculateService.ResetScore(ref _itemScore);
            _calculateService.ResetScore(ref _tankScore);

            // スコアリセット通知
            OnScoreChanged?.Invoke(0);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// スコア加算を総スコアへ反映し通知する
        /// </summary>
        /// <param name="delta">加算スコア量</param>
        private void ApplyScore(int delta)
        {
            if (delta == 0)
            {
                return;
            }

            // 加算前スコアを保持
            int previousScore = _totalScore;

            // 総スコアへ加算
            _totalScore += delta;

            // 上限補正
            if (_totalScore > SCORE_MAX)
            {
                _totalScore = SCORE_MAX;
            }

            // 増加量を算出
            int appliedDelta = _totalScore - previousScore;

            // スコア変動通知
            OnScoreChanged?.Invoke(appliedDelta);
        }
    }
}