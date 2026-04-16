// ======================================================
// PhaseTransitionConfig.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-16
// 概要     : フェーズ遷移およびプレイ進行設定
// ======================================================

namespace PhaseSystem.Domain
{
    /// <summary>
    /// フェーズ遷移設定データ
    /// </summary>
    public sealed class PhaseTransitionConfig
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>プレイヤー人数</summary>
        private readonly int _playerCount;

        /// <summary>1 プレイヤーあたりの制限時間</summary>
        private readonly float _perPlayerLimitTime;

        /// <summary>Ready → Play 遷移時間</summary>
        private readonly float _readyToPlayWaitTime;

        /// <summary>Play → Finish 遷移時間</summary>
        private readonly float _playToFinishWaitTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>プレイヤー人数</summary>
        public int PlayerCount => _playerCount;

        /// <summary>1 プレイヤーあたりの制限時間</summary>
        public float PerPlayerLimitTime => _perPlayerLimitTime;

        /// <summary>Ready → Play 遷移時間</summary>
        public float ReadyToPlayWaitTime => _readyToPlayWaitTime;

        /// <summary>Play → Finish 遷移時間</summary>
        public float PlayToFinishWaitTime => _playToFinishWaitTime;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PhaseTransitionConfig(
            int playerCount,
            float perPlayerLimitTime,
            float readyToPlayWaitTime,
            float playToFinishWaitTime)
        {
            _playerCount = playerCount;
            _perPlayerLimitTime = perPlayerLimitTime;
            _readyToPlayWaitTime = readyToPlayWaitTime;
            _playToFinishWaitTime = playToFinishWaitTime;
        }
    }
}