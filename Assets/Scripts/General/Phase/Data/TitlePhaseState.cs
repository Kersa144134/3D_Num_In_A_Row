// ======================================================
// TitlePhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : タイトルフェーズの振る舞い
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// タイトルフェーズの処理
    /// </summary>
    public sealed class TitlePhaseState : IPhaseState
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>タイトルシーン名</summary>
        private const string TITLE_SCENE_NAME = "TitleScene";

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
            // 現在のシーンを初期値として設定する（遷移なし状態）
            targetScene = currentScene;

            // 現在のフェーズを維持する
            targetPhase = PhaseType.Title;

            // --------------------------------------------------
            // シーン整合性チェック
            // --------------------------------------------------

            // 現在のシーンがタイトルシーンと一致しない場合
            if (currentScene != TITLE_SCENE_NAME)
            {
                // タイトルシーンへ強制遷移する
                targetScene = TITLE_SCENE_NAME;
            }
        }
    }
}