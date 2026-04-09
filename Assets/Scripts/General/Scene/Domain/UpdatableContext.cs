// ======================================================
// UpdatableContext.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-24
// 概要     : Updatable をまとめて保持するコンテキスト
//            同一型の複数オブジェクト登録に対応
// ======================================================

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace SceneSystem.Domain
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
        /// 型ごとの Updatable リスト辞書
        /// </summary>
        private readonly Dictionary<Type, List<object>> _services
            = new Dictionary<Type, List<object>>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// シーン内の全 IUpdatable
        /// </summary>
        public IUpdatable[] Updatables { get; set; } = Array.Empty<IUpdatable>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Updatable 登録（同一型複数登録可能）
        /// </summary>
        /// <param name="type">型</param>
        /// <param name="instance">インスタンス</param>
        public void Register(Type type, object instance)
        {
            if (!_services.TryGetValue(type, out List<object>? list))
            {
                // 型リストが未作成なら新規作成
                list = new List<object>();
                _services[type] = list;
            }

            // リストに追加
            list.Add(instance);
        }

        /// <summary>
        /// 型に一致する Updatable 配列を取得
        /// 存在しなければ空配列を返す
        /// </summary>
        /// <typeparam name="T">取得対象の型</typeparam>
        /// <returns>型に一致する Updatable 配列</returns>
        public T[] GetAll<T>()
        {
            if (_services.TryGetValue(typeof(T), out List<object>? list))
            {
                // T にキャストして配列で返す
                return list.Cast<T>().ToArray();
            }

            return Array.Empty<T>();
        }

        /// <summary>
        /// 型に一致する Updatable 1件のみ取得
        /// 存在しなければ null を返す
        /// </summary>
        /// <typeparam name="T">取得対象の型</typeparam>
        /// <returns>最初のインスタンスまたは null</returns>
        public T? Get<T>() where T : class
        {
            // GetAll から最初の要素を取得
            return GetAll<T>().FirstOrDefault();
        }
    }
}