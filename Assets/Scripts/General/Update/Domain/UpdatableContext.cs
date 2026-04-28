// ======================================================
// UpdatableContexts.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-22
// 概要     : Updatable を enum で管理するコンテキスト
//          : 同一識別子に複数登録可能
// ======================================================

#nullable enable

using System;
using System.Collections.Generic;

namespace UpdateSystem.Domain
{
    /// <summary>
    /// Updatableを識別子ベースで管理するコンテキスト
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
        /// UpdatableType → Updatableリスト
        /// </summary>
        private readonly Dictionary<UpdatableType, List<IUpdatable>> _map
            = new Dictionary<UpdatableType, List<IUpdatable>>();

        // ======================================================
        // IUpdatableWriter 実装
        // ======================================================

        /// <summary>
        /// Updatable登録
        /// </summary>
        /// <param name="type">識別子</param>
        /// <param name="instance">登録インスタンス</param>
        void IUpdatableWriter.Register(in UpdatableType type, in IUpdatable instance)
        {
            if (instance == null)
            {
                return;
            }

            // 型ごとのリストを取得または生成
            if (!_map.TryGetValue(type, out List<IUpdatable>? list))
            {
                list = new List<IUpdatable>();
                _map[type] = list;
            }

            // 登録
            list.Add(instance);
        }

        // ======================================================
        // IUpdatableReader 実装
        // ======================================================

        /// <summary>
        /// 指定識別子の全 Updatable 取得
        /// </summary>
        public IUpdatable[] GetAll(in UpdatableType type)
        {
            // 存在しない場合は空配列
            if (!_map.TryGetValue(type, out List<IUpdatable>? list))
            {
                return Array.Empty<IUpdatable>();
            }

            return list.ToArray();
        }

        /// <summary>
        /// 指定識別子の Updatable 取得
        /// </summary>
        public IUpdatable? Get(in UpdatableType type)
        {
            // 存在する場合は先頭要素を返す
            if (_map.TryGetValue(type, out List<IUpdatable>? list))
            {
                if (list.Count > 0)
                {
                    return list[0];
                }
            }

            return null;
        }

        // ======================================================
        // IUpdatableEnumerable 実装
        // ======================================================

        /// <summary>
        /// 全 Updatable 取得
        /// </summary>
        IUpdatable[] IUpdatableEnumerable.GetAllUpdatables()
        {
            // 合計要素数をカウント
            int totalCount = 0;

            // 各リストの要素数を加算する
            foreach (KeyValuePair<UpdatableType, List<IUpdatable>> pair in _map)
            {
                totalCount += pair.Value.Count;
            }

            // 配列を確保
            IUpdatable[] result = new IUpdatable[totalCount];

            int index = 0;

            // 配列へ直接コピーする
            foreach (KeyValuePair<UpdatableType, List<IUpdatable>> pair in _map)
            {
                List<IUpdatable> list = pair.Value;

                for (int i = 0; i < list.Count; i++)
                {
                    result[index] = list[i];
                    index++;
                }
            }

            return result;
        }
    }
}