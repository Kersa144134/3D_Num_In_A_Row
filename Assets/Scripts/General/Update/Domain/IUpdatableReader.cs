// ======================================================
// IUpdatableReader.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : Updatable 取得専用インターフェース
// ======================================================

#nullable enable

namespace UpdateSystem.Domain
{
    /// <summary>
    /// Updatable 取得専用インターフェース
    /// </summary>
    public interface IUpdatableReader
    {
        /// <summary>
        /// 指定識別子に対応する Updatabl e一覧を取得する
        /// </summary>
        IUpdatable[] GetAll(in UpdatableType type);

        /// <summary>
        /// 指定識別子の先頭要素を取得する
        /// </summary>
        IUpdatable? Get(in UpdatableType type);
    }
}