// ======================================================
// ScoreCalculateService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-09
// 更新日時 : 2026-03-09
// 概要     : スコア計算ロジックを提供するサービスクラス
// ======================================================

using UnityEngine;
using ScoreSystem.Domain;

namespace ScoreSystem.Application
{
    /// <summary>
    /// スコア計算サービス
    /// </summary>
    public sealed class ScoreCalculateService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// スコア最大値
        /// </summary>
        private readonly int _maxScore;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ScoreCalculateService を生成する
        /// </summary>
        /// <param name="maxScore">スコア最大値</param>
        public ScoreCalculateService(int maxScore)
        {
            _maxScore = maxScore;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 固定値をスコアに加算する
        /// </summary>
        /// <param name="scoreData">更新対象のスコアデータ</param>
        /// <param name="score">加算スコア</param>
        /// <returns>実際に増加したスコア量</returns>
        public int AddFixedScore(ref ScoreData scoreData, in int score)
        {
            if (score == 0)
            {
                return 0;
            }

            // 加算前スコアを保持
            int previousScore = scoreData.TotalScore;

            // スコア加算カウンターを増加
            scoreData.AddCount++;

            // 指定された固定スコアを累計スコアへ加算する
            scoreData.TotalScore += score;

            // スコアが上限値を超えた場合は最大値に補正する
            if (scoreData.TotalScore > _maxScore)
            {
                scoreData.TotalScore = _maxScore;
            }

            // 加算後スコアとの差分から実際の増加量を算出する
            int delta = scoreData.TotalScore - previousScore;

            // 呼び出し元に増加量を返す
            return delta;
        }

        /// <summary>
        /// 累積カウントに応じたスコア加算を行う
        /// </summary>
        /// <param name="scoreData">更新対象のスコアデータ</param>
        /// <param name="baseScore">基準スコア</param>
        /// <param name="bonusMultiplier">累積ごとの倍率補正値</param>
        /// <returns>実際に増加したスコア量</returns>
        public int AddCumulativeScore(
            ref ScoreData scoreData,
            in int baseScore,
            in float bonusMultiplier
        )
        {
            // 加算前スコアを保持
            int previousScore = scoreData.TotalScore;

            // 基準スコアが 0 なら加算しない
            if (baseScore == 0)
            {
                return 0;
            }

            // 累積倍率を計算
            float multiplier = 1f + (scoreData.AddCount - 1) * bonusMultiplier;

            // スコア加算量を算出
            int scoreToAdd = Mathf.FloorToInt(baseScore * multiplier);

            // 累計スコアへ加算
            scoreData.TotalScore += scoreToAdd;

            // 上限制御
            if (scoreData.TotalScore > _maxScore)
            {
                scoreData.TotalScore = _maxScore;
            }

            // 実際の増加量
            int delta = scoreData.TotalScore - previousScore;

            return delta;
        }

        /// <summary>
        /// 累積カウンターのみ加算する
        /// </summary>
        public void AddCumulativeCount(ref ScoreData scoreData)
        {
            // 累積回数のみ進める
            scoreData.AddCount++;
        }

        /// <summary>
        /// スコアを初期化する
        /// </summary>
        /// <param name="scoreData">更新対象のスコアデータ</param>
        public void ResetScore(ref ScoreData scoreData)
        {
            scoreData.TotalScore = 0;
        }

        /// <summary>
        /// 加算カウントを初期化する
        /// </summary>
        /// <param name="scoreData">更新対象のスコアデータ</param>
        public void ResetAddCounter(ref ScoreData scoreData)
        {
            scoreData.AddCount = 0;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// スコアの上限制御を行う
        /// </summary>
        private int ClampScore(in int score)
        {
            // 上限を超えないように補正する
            if (score > _maxScore)
            {
                return _maxScore;
            }

            // 下限は0固定（必要なら追加）
            if (score < 0)
            {
                return 0;
            }

            // 範囲内ならそのまま返す
            return score;
        }
    }
}