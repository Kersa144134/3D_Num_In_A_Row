// ======================================================
// ReadyPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : ゲーム開始前フェーズの振る舞い
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// Readyフェーズの処理
    /// </summary>
    public sealed class ReadyPhaseState : IPhaseState
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>メインシーン名</summary>
        private const string MAIN_SCENE_NAME = "MainScene";

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
            // 現在のシーンを維持する
            targetScene = currentScene;

            // 現在のフェーズを維持する
            targetPhase = PhaseType.Ready;

            // --------------------------------------------------
            // シーン整合性チェック
            // --------------------------------------------------

            // メインシーンでなければ遷移
            if (currentScene != MAIN_SCENE_NAME)
            {
                // メインシーンへ遷移
                targetScene = MAIN_SCENE_NAME;
            }
        }
    }
}