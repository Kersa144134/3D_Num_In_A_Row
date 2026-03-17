// ======================================================
// UpdatableContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-17
// 概要     : Updatable をまとめて保持するコンテキスト
// ======================================================

using System;
using System.Collections.Generic;

namespace SceneSystem.Data
{
    /// <summary>
    /// Updatable を保持するデータコンテキスト
    /// </summary>
    public sealed class UpdatableContext
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// Updatable キャッシュ
        /// </summary>
        private readonly Dictionary<Type, object> _services
            = new Dictionary<Type, object>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// シーン内の全 IUpdatable
        /// </summary>
        public IUpdatable[] Updatables;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Updatable 登録
        /// </summary>
        public void Register(Type type, object instance)
        {
            _services[type] = instance;
        }

        /// <summary>
        /// Updatable 取得
        /// </summary>
        public T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out object value))
            {
                return (T)value;
            }

            return default;
        }
    }
}