// ======================================================
// OptionSelectionIndexData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : オプション種別と初期選択インデックスを保持する
// ======================================================

using System;

namespace OptionSystem.Domain
{
    /// <summary>
    /// オプション選択インデックス設定データ
    /// </summary>
    [Serializable]
    public sealed class OptionSelectionIndexData
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>
        /// オプション種別
        /// </summary>
        public OptionType Type;

        /// <summary>
        /// 初期選択インデックス
        /// </summary>
        public int SelectedIndex;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期値付きコンストラクタ
        /// </summary>
        /// <param name="type">オプション種別</param>
        /// <param name="selectedIndex">初期選択インデックス</param>
        public OptionSelectionIndexData(OptionType type, int selectedIndex)
        {
            Type = type;
            SelectedIndex = selectedIndex;
        }
    }
}