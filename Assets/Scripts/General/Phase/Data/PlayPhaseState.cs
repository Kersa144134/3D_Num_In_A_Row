// ======================================================
// PlayPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : プレイフェーズの振る舞い
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// プレイフェーズの処理
    /// </summary>
    public sealed class PlayPhaseState : IPhaseState
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>PlayからFinishへ遷移するまでの待機時間（秒）</summary>
        private const float PLAY_TO_FINISH_WAIT_TIME = 120.0f;

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
            // 現在のシーンを維持する（デフォルト状態）
            targetScene = currentScene;

            // 現在のフェーズを維持する
            targetPhase = PhaseType.Play;

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------

            // 経過時間が規定時間を超えたかを判定する
            if (elapsedTime > PLAY_TO_FINISH_WAIT_TIME)
            {
                // プレイ終了としてFinishフェーズへ遷移する
                targetPhase = PhaseType.Finish;
            }
        }
    }
}