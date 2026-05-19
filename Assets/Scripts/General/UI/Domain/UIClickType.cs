// ======================================================
// UIClickType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-19
// 更新日時 : 2026-05-19
// 概要     : クリック入力種別定義
// ======================================================

namespace UISystem.Domain
{
    /// <summary>
    /// クリック入力種別
    /// </summary>
    public enum UIClickType
    {
        /// <summary>未設定</summary>
        None,

        /// <summary>左クリック</summary>
        Left,

        /// <summary>右クリック</summary>
        Right,

        /// <summary>中クリック</summary>
        Middle
    }
}