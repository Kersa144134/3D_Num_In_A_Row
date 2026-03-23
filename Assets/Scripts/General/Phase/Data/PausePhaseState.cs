// ======================================================
// PausePhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : 一時停止フェーズの振る舞い
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// Pauseフェーズの処理
    /// </summary>
    public sealed class PausePhaseState : IPhaseState
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
            // 現在のシーンを維持する（ポーズ中は変更しない）
            targetScene = currentScene;

            // フェーズも維持する
            targetPhase = PhaseType.Pause;

            // --------------------------------------------------
            // 備考
            // --------------------------------------------------

            // ポーズ解除などの遷移は外部入力（UIなど）で制御する想定
        }
    }
}