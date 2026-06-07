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
using BoardSystem.Presentation;
using ScoreSystem.Domain;

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
                int addedScore = _scoreManager.AddLineScore(playerId, line.Count);

                // 発光開始時に通知するため保持
                _pendingAddScoreEvents.Enqueue(new ScoreEvent(playerId, addedScore));
            }
        }

        /// <summary>
        /// ボード位置とライン中心座標から差分ベクトルを算出し、イベント通知する
        /// </summary>
        /// <param name="boardPresenter">対象ボード</param>
        /// <param name="linePosition">成立ライン中心座標</param>
        private void ProcessCenterOffset(in BoardPresenter boardPresenter, in Vector3 linePosition)
        {
            // --------------------------------------------------
            // ボード位置取得
            // --------------------------------------------------
            Vector3 boardPosition;

            // 辞書に登録済かどうか
            if (_boardPosition.ContainsKey(boardPresenter))
            {
                boardPosition = _boardPosition[boardPresenter];
            }
            else
            {
                boardPosition = boardPresenter.gameObject.transform.position;

                // 辞書登録
                _boardPosition.Add(boardPresenter, boardPosition);
            }

            // --------------------------------------------------
            // 中心差分ベクトル算出
            // --------------------------------------------------
            Vector3 centerOffset = linePosition - boardPosition;

            // 正規化
            Vector3 normalizedCenterOffset = centerOffset.normalized;

            // --------------------------------------------------
            // イベント通知
            // --------------------------------------------------
            _onCenterPositionCalculated.OnNext(linePosition);
            _onCenterOffsetVectorCalculated.OnNext(normalizedCenterOffset);
        }
    }
}