// ======================================================
// OptionButtonData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-08
// 概要     : オプションボタン情報保持クラス
// ======================================================

using UnityEngine.UI;

namespace OptionSystem.Domain
{
    /// <summary>
    /// オプションボタン情報
    /// </summary>
    public sealed class OptionButtonData
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>Button</summary>
        public Button Button { get; }

        /// <summary>オプション種別</summary>
        public OptionType Type { get; }

        /// <summary>int 値</summary>
        public int IntValue { get; }

        /// <summary>float 値</summary>
        public float FloatValue { get; }

        /// <summary>盤面サイズ</summary>
        public GameRules.BoardSizeType BoardSizeType { get; }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// int 値用コンストラクタ
        /// </summary>
        public OptionButtonData(
            in Button button,
            in OptionType type,
            in int value)
        {
            Button = button;
            Type = type;
            IntValue = value;
        }

        /// <summary>
        /// float 値用コンストラクタ
        /// </summary>
        public OptionButtonData(
            in Button button,
            in OptionType type,
            in float value)
        {
            Button = button;
            Type = type;
            FloatValue = value;
        }

        /// <summary>
        /// BoardSize 用コンストラクタ
        /// </summary>
        public OptionButtonData(
            in Button button,
            in GameRules.BoardSizeType boardSizeType)
        {
            Button = button;
            Type = OptionType.BoardSize;
            BoardSizeType = boardSizeType;
        }
    }
}