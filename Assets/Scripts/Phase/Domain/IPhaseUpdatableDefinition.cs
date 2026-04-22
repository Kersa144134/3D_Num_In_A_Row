// ======================================================
// IPhaseUpdatableDefinition.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : フェーズごとに実行対象となるUpdatable群を明示的に定義するインターフェース
//            フェーズ単位で更新対象を完全に固定し、動的検索処理を排除する設計
// ======================================================

#nullable enable

using UpdateSystem.Domain;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// フェーズごとのUpdatable実行対象を識別子で定義する
    /// </summary>
    public interface IPhaseUpdatableDefinition
    {
        /// <summary>
        /// このフェーズで実行されるUpdatable種別一覧
        /// </summary>
        UpdatableType[] GetUpdatableTypes();
    }
}