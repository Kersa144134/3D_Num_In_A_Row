// ======================================================
// SeType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : SE タイプ定義
// ======================================================

namespace SoundSystem.Domain
{
    /// <summary>
    /// SE タイプ
    /// </summary>
    public enum SeType
    {
        /// <summary>未設定</summary>
        None = 0,

        /// <summary>UI 決定</summary>
        UIDecide,

        /// <summary>UI キャンセル</summary>
        UICancel
    }
}