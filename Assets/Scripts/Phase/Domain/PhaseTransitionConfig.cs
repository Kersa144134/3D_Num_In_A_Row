// ======================================================
// PhaseTransitionConfig.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-17
// 概要     : フェーズ遷移およびプレイ進行設定
// ======================================================

using UnityEngine;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// フェーズ遷移設定データ
    /// </summary>
    public sealed class PhaseTransitionConfig
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>プレイヤー人数</summary>
        public int PlayerCount { get; }

        /// <summary>総ターン数</summary>
        public int TurnCount { get; }

        /// <summary>1 プレイヤーあたりの制限時間</summary>
        public float PerPlayerLimitTime { get; }

        /// <summary>Ready → ChangePlayer 遷移時間</summary>
        public float ReadyToChangePlayerWaitTime { get; }

        /// <summary>ChangePlayer → Play 遷移時間</summary>
        public float ChangePlayerToPlayWaitTime { get; }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PhaseTransitionConfig(
            in int playerCount,
            in int turnCount,
            in float perPlayerLimitTime,
            in float readyToChangePlayerWaitTime,
            in float changePlayerToPlayWaitTime)
        {
            PlayerCount = playerCount;
            TurnCount = turnCount;
            PerPlayerLimitTime = perPlayerLimitTime;
            ReadyToChangePlayerWaitTime = readyToChangePlayerWaitTime;
            ChangePlayerToPlayWaitTime = changePlayerToPlayWaitTime;
        }
    }
}