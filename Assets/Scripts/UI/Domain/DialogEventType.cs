// ======================================================
// DialogEventType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-20
// 更新日時 : 2026-05-20
// 概要     : ダイアログから発火されるイベント種別定義
// ======================================================

namespace UISystem.Domain
{
    /// <summary>
    /// ダイアログから通知されるイベントの種類
    /// </summary>
    public enum DialogEventType
    {
        /// <summary>未設定</summary>
        None,

        /// <summary>シーン遷移要求</summary>
        RequestSceneChange,
    }
}