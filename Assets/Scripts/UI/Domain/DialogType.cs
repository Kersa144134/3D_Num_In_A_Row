// ======================================================
// DialogType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-19
// 更新日時 : 2026-05-19
// 概要     : ダイアログの種類
// ======================================================

namespace UISystem.Domain
{
    /// <summary>
    /// ダイアログ種別
    /// </summary>
    public enum DialogType
    {
        /// <summary>未指定</summary>
        None,

        /// <summary>ゲーム開始ダイアログ</summary>
        StartGame,

        /// <summary>ゲーム終了ダイアログ</summary>
        ExitGame,

        /// <summary>ポーズダイアログ</summary>
        Pause
    }
}