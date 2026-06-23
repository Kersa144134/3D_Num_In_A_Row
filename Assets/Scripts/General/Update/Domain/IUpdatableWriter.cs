// ======================================================
// IUpdatableWriter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : Updatable 登録専用インターフェース
// ======================================================

#nullable enable

namespace UpdateSystem.Domain
{
    /// <summary>
    /// Updatable 登録専用インターフェース
    /// </summary>
    public interface IUpdatableWriter
    {
        /// <summary>
        /// Updatable を識別子で登録する
        /// 同一識別子への複数登録を許可する
        /// </summary>
        /// <param name="type">Updatable種別</param>
        /// <param name="instance">登録するUpdatable</param>
        void Register(in UpdatableType type, in IUpdatable instance);
    }
}