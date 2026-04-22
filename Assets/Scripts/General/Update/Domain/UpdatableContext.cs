// ======================================================
// UpdatableContexts.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-22
// 概要     : Updatableを識別子(enum)で管理するコンテキスト
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
            // --------------------------------------------------
            // null防御（外部呼び出し対策）
            // --------------------------------------------------
            if (instance == null)
            {
                return;
            }

            // --------------------------------------------------
            // 型ごとのリスト取得または生成
            // --------------------------------------------------
            if (!_map.TryGetValue(type, out List<IUpdatable>? list))
            {
                list = new List<IUpdatable>();
                _map[type] = list;
            }

            // --------------------------------------------------
            // 登録
            // --------------------------------------------------
            list.Add(instance);
        }

        // ======================================================
        // IUpdatableReader 実装
        // ======================================================

        /// <summary>
        /// 指定識別子の全Updatable取得
        /// </summary>
        public IUpdatable[] GetAll(in UpdatableType type)
        {
            // --------------------------------------------------
            // 存在しない場合は空配列
            // --------------------------------------------------
            if (!_map.TryGetValue(type, out List<IUpdatable>? list))
            {
                return Array.Empty<IUpdatable>();
            }

            // --------------------------------------------------
            // コピーして返却
            // --------------------------------------------------
            return list.ToArray();
        }

        /// <summary>
        /// 指定識別子の代表Updatable取得
        /// </summary>
        public IUpdatable? Get(in UpdatableType type)
        {
            // --------------------------------------------------
            // 存在確認
            // --------------------------------------------------
            if (_map.TryGetValue(type, out List<IUpdatable>? list))
            {
                // --------------------------------------------------
                // 先頭要素を返す
                // --------------------------------------------------
                if (list.Count > 0)
                {
                    return list[0];
                }
            }

            // --------------------------------------------------
            // 未登録または空
            // --------------------------------------------------
            return null;
        }

        // ======================================================
        // IUpdatableEnumerable 実装
        // ======================================================

        /// <summary>
        /// 全Updatable取得（デバッグ・管理用）
        /// </summary>
        IUpdatable[] IUpdatableEnumerable.GetAllUpdatables()
        {
            // --------------------------------------------------
            // 全リスト結合
            // --------------------------------------------------
            List<IUpdatable> result = new List<IUpdatable>();

            foreach (var pair in _map)
            {
                result.AddRange(pair.Value);
            }

            return result.ToArray();
        }
    }
}