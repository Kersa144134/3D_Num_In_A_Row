// ======================================================
// EventPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : イベントフェーズの振る舞い
// ======================================================

namespace PhaseSystem.Domain
{
    /// <summary>
    /// Event フェーズの処理
    /// </summary>
    public sealed class EventPhaseState : IPhaseState
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
        public void OnEnter()
        {

        }

        /// <summary>
        /// フェーズ終了時処理
        /// </summary>
        public void OnExit()
        {

        }

        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        public void OnUpdate(in float unscaledDeltaTime)
        {
            _elapsedTime += unscaledDeltaTime;
        }
    }
}