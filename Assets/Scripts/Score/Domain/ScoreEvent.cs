// ======================================================
// ScoreEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-26
// 更新日時 : 2026-04-26
// 概要     : スコア通知用イベントデータ
// ======================================================

namespace ScoreSystem.Domain
{
    /// <summary>
    /// スコア更新イベントデータ
    /// </summary>
    public readonly struct ScoreEvent
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>プレイヤーID</summary>
        public readonly int PlayerId;

        /// <summary>スコア</summary>
        public readonly int Score;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playerId">プレイヤーID</param>
        /// <param name="score">スコア</param>
        public ScoreEvent(in int playerId, in int score)
        {
            PlayerId = playerId;
            Score = score;
        }
    }
}