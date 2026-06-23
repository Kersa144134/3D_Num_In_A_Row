// ======================================================
// PhaseStateBase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-23
// 更新日時 : 2026-04-23
// 概要     : フェーズ共通の基底クラス
// ======================================================

using UpdateSystem.Domain;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// フェーズ共通基底クラス
    /// </summary>
    public abstract class PhaseStateBase : IPhaseState, IPhaseStreamDefinition, IPhaseUpdatableDefinition
    {
        // ======================================================
        // IPhaseStreamDefinition 実装
        // ======================================================

        /// <summary>
        /// フェーズごとに購読開始される Updatable 種別
        /// </summary>
        /// <returns>Updatable 種別配列</returns>
        public abstract UpdatableType[] GetStreamTypes();

        // ======================================================
        // IPhaseUpdatableDefinition 実装
        // ======================================================

        /// <summary>
        /// フェーズごとに有効化される Updatable 種別
        /// </summary>
        /// <returns>Updatable 種別配列</returns>
        public abstract UpdatableType[] GetUpdatableTypes();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズ経過時間</summary>
        protected float _elapsedTime = 0.0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>フェーズ経過時間</summary>
        public float ElapsedTime => _elapsedTime;

        // ======================================================
        // IPhaseState 実装
        // ======================================================

        /// <summary>
        /// フェーズ開始処理
        /// </summary>
        public void OnEnterState()
        {
            // 共通処理
            _elapsedTime = 0.0f;

            // 派生処理
            OnEnterStateInternal();
        }

        /// <summary>
        /// フェーズ終了処理
        /// </summary>
        public void OnExitState()
        {
            OnExitStateInternal();
        }

        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        public void OnUpdateState(in float unscaledDeltaTime)
        {
            // 共通処理
            _elapsedTime += unscaledDeltaTime;

            // 派生処理
            OnUpdateStateInternal(unscaledDeltaTime);
        }

        /// <summary>
        /// フェーズ更新後処理
        /// </summary>
        public void OnLateUpdateState(in float unscaledDeltaTime)
        {
            OnLateUpdateStateInternal(unscaledDeltaTime);
        }

        // ======================================================
        // 抽象メソッド
        // ======================================================

        protected abstract void OnEnterStateInternal();

        protected abstract void OnExitStateInternal();

        protected abstract void OnUpdateStateInternal(in float unscaledDeltaTime);

        protected abstract void OnLateUpdateStateInternal(in float unscaledDeltaTime);
    }
}