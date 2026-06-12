// ======================================================
// GameEventRouter.Board.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            ボード関連処理をまとめたファイル
// ======================================================

using System.Collections.Generic;
using BoardSystem.Domain;
using ScoreSystem.Domain;
using SoundSystem.Domain;

namespace GameSystem.Presentation
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed partial class GameEventRouter
    {
        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ライン成立時の処理を行う
        /// </summary>
        private void HandleLineCompleted(in IReadOnlyList<LineCompleteEvent> events)
        {
            // 前回分の保留データを破棄
            _pendingAddScoreEvents.Clear();

            // スコア累積カウント加算
            _scoreManager.AddAllCumulativeCount();

            // SE 再生
            SeType comboSE = _scoreManager.CumulativeCount.Value
                switch
                {
                    1 => SeType.Combo_1,
                    2 => SeType.Combo_2,
                    _ => SeType.Combo_3
                };

            _soundManager?.PlaySE(comboSE);

            // --------------------------------------------------
            // 各イベントを個別に処理
            // --------------------------------------------------
            for (int i = 0; i < events.Count; i++)
            {
                // 現在処理中のライン成立イベント
                LineCompleteEvent lineEvent = events[i];

                // プレイヤー ID
                int playerId = lineEvent.Player;

                // ライン座標リスト
                IReadOnlyList<BoardIndex> line = lineEvent.LinePositions;

                if (line == null)
                {
                    continue;
                }

                // 1 ライン分のスコア加算
                int addScore = _scoreManager.AddLineScore(playerId, line.Count);

                // 発光開始時に通知するため保持
                _pendingAddScoreEvents.Enqueue(new ScoreEvent(playerId, addScore));
            }
        }
    }
}