// ======================================================
// OptionButtonGroup.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : OptionType と GridLayoutGroup を紐づけるデータクラス
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using OptionSystem.Domain;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// OptionType と GridLayoutGroup を紐づけるデータ
    /// </summary>
    [Serializable]
    public sealed class OptionButtonGroup
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>
        /// 対応するオプション種別
        /// </summary>
        [SerializeField]
        private OptionType _type;

        /// <summary>
        /// 対象 GridLayoutGroup
        /// </summary>
        [SerializeField]
        private GridLayoutGroup _group;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// オプション種別取得
        /// </summary>
        public OptionType Type => _type;

        /// <summary>
        /// UIグループ取得
        /// </summary>
        public GridLayoutGroup Group => _group;
    }
}