// ======================================================
// OptionSelectionIndexTable.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-10
// 更新日時 : 2026-05-10
// 概要     : 各オプション種別に対応する選択インデックスを管理するテーブル
// ======================================================

using System;
using System.Collections.Generic;

namespace OptionSystem.Domain
{
    /// <summary>
    /// オプションごとの選択インデックスを管理するテーブルクラス
    /// </summary>
    public sealed class OptionSelectionIndexTable
    {
        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// オプション種別と選択インデックスの対応テーブル
        /// </summary>
        private readonly Dictionary<OptionType, int> _indexTable
            = new Dictionary<OptionType, int>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 初期値を設定する
        /// </summary>
        public void Initialize(in OptionType type, in int index)
        {
            _indexTable[type] = index;
        }

        /// <summary>
        /// インデックスを取得する
        /// </summary>
        public int Get(in OptionType type)
        {
            // 存在しない場合は安全に0を返す
            if (_indexTable.TryGetValue(type, out int index))
            {
                return index;
            }

            return 0;
        }

        /// <summary>
        /// インデックスを更新する
        /// </summary>
        public void Set(in OptionType type, in int index)
        {
            // 選択結果を保存する
            _indexTable[type] = index;
        }

        /// <summary>
        /// 全データを取得する
        /// </summary>
        public IReadOnlyDictionary<OptionType, int> GetAll()
        {
            return _indexTable;
        }
    }
}