// ======================================================
// ResultPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : リザルトフェーズの振る舞い
// ======================================================

using System;
using UpdateSystem.Domain;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// Resultフェーズの処理
    /// </summary>
    public sealed class ResultPhaseState : IPhaseState, IPhaseUpdatableDefinition
    {
        // ======================================================
        // IPhaseUpdatableDefinition 実装
        // ======================================================

        /// <summary>
        /// このフェーズで更新対象となる Updatable 種別を返す
        /// </summary>
        public UpdatableType[] GetUpdatableTypes()
        {
            return Array.Empty<UpdatableType>();
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズ経過時間</summary>
        private float _elapsedTime = 0.0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>フェーズ経過時間</summary>
        public float ElapsedTime => _elapsedTime;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ開始時処理
        /// </summary>
        public void OnEnterState()
        {

        }

        /// <summary>
        /// フェーズ終了時処理
        /// </summary>
        public void OnExitState()
        {

        }

        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        public void OnUpdateState(in float unscaledDeltaTime)
        {
            _elapsedTime += unscaledDeltaTime;
        }

        /// <summary>
        /// フェーズ更新後処理
        /// </summary>
        public void OnLateUpdateState(in float unscaledDeltaTime)
        {

        }
    }
}