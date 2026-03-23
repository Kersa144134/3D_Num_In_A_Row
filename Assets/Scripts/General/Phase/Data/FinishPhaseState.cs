// ======================================================
// FinishPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : ゲーム終了フェーズの振る舞い
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// Finishフェーズの処理
    /// </summary>
    public sealed class FinishPhaseState : IPhaseState
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>FinishからResultへ遷移する待機時間（秒）</summary>
        private const float FINISH_TO_RESULT_WAIT_TIME = 3.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズ経過時間</summary>
        private float _elapsedTime = 0.0f;

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

            // 現在フェーズ維持
            targetPhase = PhaseType.Finish;

            // --------------------------------------------------
            // 経過時間加算
            // --------------------------------------------------

            // フレーム毎に経過時間を加算
            _elapsedTime += unscaledDeltaTime;

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------

            // 指定時間経過した場合
            if (_elapsedTime >= FINISH_TO_RESULT_WAIT_TIME)
            {
                // Resultへ遷移
                targetPhase = PhaseType.Result;

                // 経過時間リセット
                _elapsedTime = 0.0f;
            }
        }
    }
}