// ======================================================
// RankingData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-01
// 更新日時 : 2026-06-01
// 概要     : ランキング情報を保持するデータ
// ======================================================

namespace ScoreSystem.Domain
{
    /// <summary>
    /// ランキング情報を表すデータ
    /// </summary>
    public readonly struct RankingData
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>プレイヤー ID（1 ベース）</summary>
        public readonly int PlayerId;

        /// <summary>プレイヤーのスコア</summary>
        public readonly int Score;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// RankingData を生成する
        /// </summary>
        /// <param name="playerId">プレイヤー ID（1 ベース）</param>
        /// <param name="score">プレイヤースコア</param>
        public RankingData(int playerId, int score)
        {
            PlayerId = playerId;
            Score = score;
        }
    }
}