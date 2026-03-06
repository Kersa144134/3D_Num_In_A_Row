// ======================================================
// UpdatableContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2025-12-17
// 概要     : 初期化済み参照をまとめて保持するコンテキスト
// ======================================================

using System;
using System.Collections.Generic;

namespace SceneSystem.Data
{
    /// <summary>
    /// 初期化済み参照を保持するデータコンテキスト
    /// </summary>
    public sealed class UpdatableContext
    {
        /// <summary>
        /// シーン内の全 IUpdatable
        /// </summary>
        public IUpdatable[] Updatables;

        /// <summary>
        /// 型キャッシュ
        /// </summary>
        private readonly Dictionary<Type, object> _services
            = new Dictionary<Type, object>();

        // ======================================================
        // 登録
        // ======================================================

        /// <summary>
        /// 型登録
        /// </summary>
        public void Register<T>(T instance)
        {
            _services[typeof(T)] = instance;
        }

        /// <summary>
        /// 型登録（object）
        /// </summary>
        public void Register(Type type, object instance)
        {
            _services[type] = instance;
        }

        // ======================================================
        // 取得
        // ======================================================

        /// <summary>
        /// 型取得
        /// </summary>
        public T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out object value))
            {
                return (T)value;
            }

            return default;
        }

        /// <summary>
        /// 型取得（Try）
        /// </summary>
        public bool TryGet<T>(out T value)
        {
            if (_services.TryGetValue(typeof(T), out object obj))
            {
                value = (T)obj;
                return true;
            }

            value = default;
            return false;
        }
    }
}