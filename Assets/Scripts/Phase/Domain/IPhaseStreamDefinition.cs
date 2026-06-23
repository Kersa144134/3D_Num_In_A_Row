// ======================================================
// IPhaseStreamDefinition.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : フェーズごとに実行対象となる Updatable 群を明示的に定義するインターフェース
// ======================================================

using UpdateSystem.Domain;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// フェーズごとの Updatable 実行対象を識別子で定義する
    /// </summary>
    public interface IPhaseStreamDefinition
    {
        /// <summary>
        /// このフェーズで実行される Updatable 種別一覧
        /// </summary>
        UpdatableType[] GetStreamTypes();
    }
}