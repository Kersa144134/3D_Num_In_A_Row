// ======================================================
// UpdatableContexts.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-22
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
    public sealed class UpdatableContexts :
        IUpdatableReader,
        IUpdatableWriter,
        IUpdatableEnumerable
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 型ごとの Updatable リスト辞書
        /// </summary>
        private readonly Dictionary<Type, List<object>> _updatables = new Dictionary<Type, List<object>>();

        /// <summary>
        /// シーン内の全 IUpdatable 実体配列
        /// </summary>
        private readonly IUpdatable[] _updatablesArray;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="updatables">シーン内の IUpdatable 配列</param>
        public UpdatableContexts(in IUpdatable[] updatables)
        {
            if (updatables == null)
            {
                _updatablesArray = Array.Empty<IUpdatable>();
                return;
            }

            // 配列をそのまま保持する
            _updatablesArray = updatables;
        }

        // ======================================================
        // IUpdatableWriter 実装
        // ======================================================

        /// <summary>
        /// Updatable 登録（Writer 経由のみ許可）
        /// </summary>
        /// <param name="type">登録対象の型</param>
        /// <param name="instance">登録するインスタンス</param>
        void IUpdatableWriter.Register(Type type, object instance)
        {
            // 型に対応するリストが存在するか確認する
            if (!_updatables.TryGetValue(type, out List<object>? list))
            {
                // 存在しない場合は新規リストを生成する
                list = new List<object>();

                // 辞書に型とリストを登録する
                _updatables[type] = list;
            }

            // インスタンスをリストに追加する
            list.Add(instance);
        }

        // ======================================================
        // IUpdatableReader 実装
        // ======================================================

        /// <summary>
        /// 型に一致する Updatable 配列を取得
        /// </summary>
        /// <typeparam name="T">取得対象の型</typeparam>
        /// <returns>一致する配列（存在しない場合は空配列）</returns>
        public T[] GetAll<T>()
        {
            // 型に対応するリストが存在するか確認する
            if (_updatables.TryGetValue(typeof(T), out List<object>? list))
            {
                // 指定型へキャストして配列として返却する
                return list.Cast<T>().ToArray();
            }

            // 存在しない場合は空配列を返却する
            return Array.Empty<T>();
        }

        /// <summary>
        /// 型に一致する Updatable を1件取得
        /// </summary>
        /// <typeparam name="T">取得対象の型</typeparam>
        /// <returns>最初のインスタンス、存在しない場合は null</returns>
        public T? Get<T>() where T : class
        {
            // 全取得結果の先頭要素を返却する
            return GetAll<T>().FirstOrDefault();
        }

        // ======================================================
        // IUpdatableEnumerable 実装
        // ======================================================

        /// <summary>
        /// 全 IUpdatable を列挙する（Enumerable 経由のみ許可）
        /// </summary>
        /// <returns>全 IUpdatable 配列</returns>
        IUpdatable[] IUpdatableEnumerable.GetAllUpdatables()
        {
            // 内部配列をそのまま返却する
            return _updatablesArray;
        }
    }
}