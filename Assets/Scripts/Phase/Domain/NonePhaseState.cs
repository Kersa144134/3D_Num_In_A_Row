// ======================================================
// NonePhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : 未初期化フェーズの振る舞い
// ======================================================

using System;
using UpdateSystem.Domain;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// Noneフェーズの処理
    /// </summary>
    public sealed class NonePhaseState : PhaseStateBase
    {
        // ======================================================
        // IPhaseUpdatableDefinition 実装
        // ======================================================

        /// <summary>
        /// このフェーズで更新対象となる Updatable 種別を返す
        /// </summary>
        public override UpdatableType[] GetUpdatableTypes()
        {
            return Array.Empty<UpdatableType>();
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