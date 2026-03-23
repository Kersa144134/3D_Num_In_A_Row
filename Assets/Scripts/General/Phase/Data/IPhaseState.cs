// ======================================================
// IPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-03-23
// 概要     : フェーズごとの振る舞いを定義するインターフェース
// ======================================================

namespace PhaseSystem.Data
{
    /// <summary>
    /// フェーズごとの振る舞いを定義するインターフェース
    /// </summary>
    public interface IPhaseState
    {
        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        void Update(
            in float unscaledDeltaTime,
            in float elapsedTime,
            in string currentScene,
            out string targetScene,
            out PhaseType targetPhase
        );
    }
}