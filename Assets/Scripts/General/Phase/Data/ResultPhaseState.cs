// ======================================================
// ResultPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : リザルトフェーズの振る舞い
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// Resultフェーズの処理
    /// </summary>
    public sealed class ResultPhaseState : IPhaseState
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>リザルトシーン名</summary>
        private const string RESULT_SCENE_NAME = "ResultScene";

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
            // 現在シーン維持
            targetScene = currentScene;

            // フェーズ維持
            targetPhase = PhaseType.Result;

            // --------------------------------------------------
            // シーン整合性チェック
            // --------------------------------------------------

            // リザルトシーンでない場合
            if (currentScene != RESULT_SCENE_NAME)
            {
                // リザルトシーンへ遷移
                targetScene = RESULT_SCENE_NAME;
            }
        }
    }
}