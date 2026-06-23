// ======================================================
// PausePhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : 一時停止フェーズの振る舞い
// ======================================================

using UpdateSystem.Domain;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// Pauseフェーズの処理
    /// </summary>
    public sealed class PausePhaseState : PhaseStateBase
    {
        // ======================================================
        // IPhaseStreamDefinition 実装
        // ======================================================

        /// <summary>
        /// フェーズごとに購読開始される Updatable 種別
        /// </summary>
        /// <returns>Updatable種別配列</returns>
        public override UpdatableType[] GetStreamTypes()
        {
            return new UpdatableType[]
            {
                UpdatableType.MainUIPresenter
            };
        }

        // ======================================================
        // IPhaseUpdatableDefinition 実装
        // ======================================================

        /// <summary>
        /// フェーズごとに有効化される Updatable 種別
        /// </summary>
        /// <returns>Updatable 種別配列</returns>
        public override UpdatableType[] GetUpdatableTypes()
        {
            return new UpdatableType[]
            {
                UpdatableType.MainUIPresenter
            };
        }

        // ======================================================
        // IPhaseState 実装
        // ======================================================

        /// <summary>
        /// フェーズ開始時処理
        /// </summary>
        protected override void OnEnterStateInternal() { }

        /// <summary>
        /// フェーズ終了時処理
        /// </summary>
        protected override void OnExitStateInternal() { }

        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        protected override void OnUpdateStateInternal(in float unscaledDeltaTime) { }

        /// <summary>
        /// フェーズ更新後処理
        /// </summary>
        protected override void OnLateUpdateStateInternal(in float unscaledDeltaTime) { }
    }
}