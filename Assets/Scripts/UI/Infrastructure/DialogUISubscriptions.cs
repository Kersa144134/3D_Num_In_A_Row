// ======================================================
// DialogUISubscriptions.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-20
// 更新日時 : 2026-05-20
// 概要     : ダイアログ UI のイベント購読対象をまとめるクラス
// ======================================================


namespace UISystem.Infrastructure
{
    /// <summary>
    /// ダイアログ UI のイベント購読対象クラス
    /// </summary>
    public sealed class DialogUISubscriptions
    {
        /// <summary>ダイアログイベント</summary>
        public DialogEvent[] Events;

        /// <summary>ボタン</summary>
        public NormalButton[] Buttons;

        /// <summary>パネル</summary>
        public BasePanelEvent[] Panels;
    }
}