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
        // 列挙体
        // ======================================================

        /// <summary>
        /// 累積ボーナス計算方式
        /// </summary>
        public enum BonusCalcType
        {
            /// <summary>
            /// 線形加算
            /// </summary>
            Linear,

            /// <summary>
            /// 加速加算
            /// </summary>
            Accelerate,

            /// <summary>
            /// 減速加算
            /// </summary>
            Decelerate
        }
        
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// スコア最大値
        /// </summary>
        private readonly int _maxScore;

        /// <summary>
        /// 累積ボーナス計算方式
        /// </summary>
        private readonly BonusCalcType _bonusType;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ScoreCalculateService を生成する
        /// </summary>
        /// <param name="maxScore">スコア最大値</param>
        public ScoreCalculateService(int maxScore, in BonusCalcType bonusType)
        {
            _maxScore = maxScore;
            _bonusType = bonusType;
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

            // 範囲補正
            ClampScore(scoreData.TotalScore);

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

            // 基準スコアが 0 なら処理なし
            if (baseScore == 0)
            {
                return 0;
            }

            // 累積倍率を計算
            float multiplier = CalculateMultiplier(
                scoreData.AddCount,
                bonusMultiplier,
                _bonusType);

            Debug.Log(multiplier);

            // スコア加算量を算出
            int scoreToAdd = Mathf.FloorToInt(baseScore * multiplier);

            // 累計スコアへ加算
            scoreData.TotalScore += scoreToAdd;

            // 範囲補正
            ClampScore(scoreData.TotalScore);

            // 実際の増加量
            int delta = scoreData.TotalScore - previousScore;

            return delta;
        }

        /// <summary>
        /// 累積カウンターのみ加算する
        /// </summary>
        public void AddCumulativeCount(ref ScoreData scoreData)
        {
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
        /// スコアの範囲制御を行う
        /// </summary>
        private int ClampScore(in int score)
        {
            // 上限補正
            if (score > _maxScore)
            {
                return _maxScore;
            }

            // 下限補正
            if (score < 0)
            {
                return 0;
            }

            return score;
        }

        /// <summary>
        /// 累積倍率を計算する
        /// </summary>
        private float CalculateMultiplier(
            in int addCount,
            in float bonusMultiplier,
            in BonusCalcType bonusType)
        {
            float result;

            switch (bonusType)
            {
                // --------------------------------------------------
                // 線形加算
                // --------------------------------------------------
                case BonusCalcType.Linear:
                    {
                        // 1 回目は必ず 1
                        if (addCount <= 1)
                        {
                            result = 1f;
                            break;
                        }

                        result = bonusMultiplier * (addCount - 1);

                        break;
                    }

                // --------------------------------------------------
                // 加速加算
                // --------------------------------------------------
                case BonusCalcType.Accelerate:
                    {
                        // 1 回目は必ず 1
                        if (addCount <= 1)
                        {
                            result = 1f;
                            break;
                        }

                        float n = addCount - 1;

                        // 1 から n までの整数の合計を使用した累積加算
                        float triangular = n * (n + 1f) * 0.5f;

                        result = 1f + bonusMultiplier * triangular;

                        break;
                    }

                // --------------------------------------------------
                // 減速加算
                // --------------------------------------------------
                case BonusCalcType.Decelerate:
                    {
                        // 1 回目は初期値
                        if (addCount <= 1)
                        {
                            result = bonusMultiplier;
                            break;
                        }

                        // addCount が増えるほど値が小さくなる逆数減衰係数
                        float decay = 1f / (1f + (addCount - 1f));

                        result = 1f + (bonusMultiplier - 1f) * decay;

                        break;
                    }

                default:
                    {
                        result = 1f;
                        break;
                    }
            }

            // --------------------------------------------------
            // 小数第4位以降を切り捨て
            // --------------------------------------------------
            return Mathf.Floor(result * 1000f) / 1000f;
        }
    }
}