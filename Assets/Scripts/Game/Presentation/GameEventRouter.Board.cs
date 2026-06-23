// ======================================================
// GameEventRouter.Board.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            ボード関連処理をまとめたファイル
// ======================================================

using System.Collections.Generic;
using UnityEngine;
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
            _scoreManager?.AddAllCumulativeCount();

            // --------------------------------------------------
            // SE 再生
            // --------------------------------------------------
            // SE ピッチ算出用コンボ数
            int comboCount = 0;

            if (_scoreManager != null)
            {
                comboCount = _scoreManager.CumulativeCount.Value;
            }

            // コンボ補正
            int adjustedCombo = Mathf.Max(comboCount - 1, 0);

            // ピッチ計算
            float comboPitch = Mathf.Min(
                COMBO_SE_BASE_PITCH + adjustedCombo * COMBO_SE_PITCH_STEP,
                COMBO_SE_MAX_PITCH);

            _soundManager?.PlayPitchSE(SeType.Combo, comboPitch, 0.5f);

            // --------------------------------------------------
            // 各イベントを個別に処理
            // --------------------------------------------------
            if (_scoreManager == null)
            {
                return;
            }

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

                // 入力タイプに応じたスコア倍率を設定
                float multiplier = _currentBoardInputType.Value == BoardInputType.Rotate
                    ? ROTATE_SCORE_MULTIPLIER
                    : NORMAL_SCORE_MULTIPLIER;

                // 1 ライン分のスコア加算
                int addScore = _scoreManager.AddLineScore(playerId, line.Count, multiplier);

                // 発光開始時に通知するため保持
                _pendingAddScoreEvents.Enqueue(new ScoreEvent(playerId, addScore));
            }
        }
    }
}