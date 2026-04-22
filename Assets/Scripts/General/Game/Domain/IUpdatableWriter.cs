// ======================================================
// IUpdatableWriter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : Updatable の登録専用インターフェース
// ======================================================

#nullable enable

using System;

namespace SceneSystem.Domain
{
    /// <summary>
    /// Updatable の登録専用インターフェース
    /// </summary>
    public interface IUpdatableWriter
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Updatable を型ごとに登録する
        /// 同一型の複数登録を許可する
        /// </summary>
        /// <param name="type">登録対象の型</param>
        /// <param name="instance">登録するインスタンス</param>
        void Register(Type type, object instance);
    }
}