// ======================================================
// IPhaseTransitionRule.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-16
// 概要     : フェーズ遷移判断ロジックを外部化するためのインターフェース
// ======================================================

namespace PhaseSystem.Domain
{
    /// <summary>
    /// フェーズ遷移の判断責務を定義するインターフェース
    /// </summary>
    public interface IPhaseTransitionRule
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在の状態および経過時間をもとに次の状態を決定する
        /// </summary>
        /// <param name="currentState">現在のフェーズ状態</param>
        /// <param name="deltaTime">フレーム経過時間</param>
        /// <returns>
        /// 次のフェーズ状態
        /// </returns>
        IPhaseState Resolve(IPhaseState currentState, float deltaTime);
    }
}