// ======================================================
// NonePhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : 未初期化フェーズの振る舞い
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// Noneフェーズの処理
    /// </summary>
    public sealed class NonePhaseState : IPhaseState
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 更新処理
        /// </summary>
        public void Update(
            in float unscaledDeltaTime,
            in float elapsedTime,
            in string currentScene,
            out string targetScene,
            out PhaseType targetPhase
        )
        {
            targetScene = currentScene;
            targetPhase = PhaseType.None;
        }
    }
}