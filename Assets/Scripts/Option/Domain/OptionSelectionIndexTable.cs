// ======================================================
// OptionSelectionIndexTable.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : オプション選択インデックスを管理する
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OptionSystem.Domain
{
    /// <summary>
    /// オプション選択インデックス管理テーブル
    /// </summary>
    [CreateAssetMenu(
        fileName = "OptionSelectionIndexTable",
        menuName = "OptionSystem/OptionSelectionIndexTable")]
    public sealed class OptionSelectionIndexTable : ScriptableObject
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>
        /// オプション初期選択インデックス設定データ
        /// </summary>
        [SerializeField]
        private List<OptionSelectionIndexData> _datas = new List<OptionSelectionIndexData>();

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// OptionType をキーとして、各オプションの選択インデックスへアクセスする辞書
        /// </summary>
        private readonly Dictionary<OptionType, int> _indexTable
            = new Dictionary<OptionType, int>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 選択インデックス辞書を構築する
        /// </summary>
        public void Initialize()
        {
            _indexTable.Clear();

            for (int i = 0; i < _datas.Count; i++)
            {
                OptionSelectionIndexData data = _datas[i];

                if (data == null)
                {
                    continue;
                }

                if (_indexTable.ContainsKey(data.Type))
                {
                    continue;
                }

                _indexTable.Add(data.Type, data.SelectedIndex);
            }
        }

        /// <summary>
        /// 選択インデックス取得
        /// </summary>
        public int Get(in OptionType type)
        {
            if (_indexTable.TryGetValue(type, out int index))
            {
                return index;
            }

            return 0;
        }

        /// <summary>
        /// 選択インデックス更新
        /// </summary>
        public void Set(in OptionType type, in int index)
        {
            _indexTable[type] = index;
        }

#if UNITY_EDITOR

        // ======================================================
        // エディタ補助
        // ======================================================

        private void OnValidate()
        {
            // 全 OptionType を取得
            OptionType[] types = (OptionType[])Enum.GetValues(typeof(OptionType));

            // インスペクタデータを一時保持する辞書
            Dictionary<OptionType, int> temp = new Dictionary<OptionType, int>();

            // --------------------------------------------------
            // インスペクタデータを走査
            // --------------------------------------------------
            for (int i = 0; i < _datas.Count; i++)
            {
                OptionSelectionIndexData data = _datas[i];

                if (data == null)
                {
                    continue;
                }

                // 同一タイプがすでに登録されている場合はスキップ
                if (temp.ContainsKey(data.Type))
                {
                    continue;
                }

                // インスペクタデータを一時辞書へ退避
                temp.Add(data.Type, data.SelectedIndex);
            }

            // --------------------------------------------------
            // インスペクタデータを再構築
            // --------------------------------------------------
            _datas.Clear();

            // OptionType 順に基づいてデータを再生成
            for (int i = 0; i < types.Length; i++)
            {
                OptionType type = types[i];

                int index = 0;

                // 既存データがある場合は復元
                if (temp.TryGetValue(type, out int saved))
                {
                    index = saved;
                }

                // インスペクタ用データを再生成
                _datas.Add(new OptionSelectionIndexData(type, index));
            }

            // 再構築
            Initialize();
        }

#endif
    }
}