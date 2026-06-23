// ======================================================
// PlayPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-04-02
// 概要     : プレイフェーズの振る舞い
// ======================================================

using UpdateSystem.Domain;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// プレイフェーズの処理
    /// </summary>
    public sealed class PlayPhaseState : PhaseStateBase, IPhaseEnterHandler
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
                UpdatableType.BoardPresenter,
                UpdatableType.CameraPresenter,
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
                UpdatableType.BoardPresenter,
                UpdatableType.CameraPresenter,
                UpdatableType.MainUIPresenter
            };
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>プレイ専用経過時間</summary>
        private float _playElapsedTime = 0.0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>プレイ専用経過時間</summary>
        public float PlayElapsedTime => _playElapsedTime;

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
        protected override void OnUpdateStateInternal(in float unscaledDeltaTime)
        {
            _playElapsedTime += unscaledDeltaTime;
        }

        /// <summary>
        /// フェーズ更新後処理
        /// </summary>
        protected override void OnLateUpdateStateInternal(in float unscaledDeltaTime) { }

        // ======================================================
        // IPhaseEnterHandler 実装
        // ======================================================

        /// <summary>
        /// フェーズ開始時処理
        /// 遷移元フェーズ付きの例外処理
        /// </summary>
        /// <param name="previousPhase">遷移元のフェーズ種別</param>
        public void OnEnterState(in PhaseType previousPhase)
        {
            OnEnterState();

            // ChangePlayer → Play の場合のみプレイ専用経過時間をリセット
            if (previousPhase == PhaseType.ChangePlayer)
            {
                _playElapsedTime = 0.0f;
            }
        }
    }
}