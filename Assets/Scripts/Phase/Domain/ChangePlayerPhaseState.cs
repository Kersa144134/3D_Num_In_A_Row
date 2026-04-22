// ======================================================
// ChangePlayerPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : プレイヤー切り替えフェーズの振る舞い
// ======================================================

using UpdateSystem.Domain;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// ChangePlayer フェーズの処理
    /// </summary>
    public sealed class ChangePlayerPhaseState : IPhaseState
    {
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
            _elapsedTime = 0.0f;
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