// ======================================================
// IUpdatableEnumerable.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : Updatable の列挙専用インターフェース
// ======================================================

#nullable enable

namespace UpdateSystem.Domain
{
    /// <summary>
    /// Updatable の列挙専用インターフェース
    /// </summary>
    public interface IUpdatableEnumerable
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 登録されている全ての IUpdatable を取得する
        /// </summary>
        /// <returns>全 IUpdatable 配列</returns>
        IUpdatable[] GetAllUpdatables();
    }
}