// ======================================================
// UIClickEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-19
// 更新日時 : 2026-05-19
// 概要     : UI クリック通知データ
// ======================================================

using UISystem.Infrastructure;

namespace UISystem.Domain
{
    /// <summary>
    /// UI クリック通知データ
    /// </summary>
    public readonly struct UIClickEvent
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>クリック入力種別</summary>
        public readonly UIClickType ClickType;

        /// <summary>対象 UI イベント</summary>
        public readonly BaseUIEvent UIEvent;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// UI クリック通知データを初期化する
        /// </summary>
        /// <param name="clickType">クリック入力種別</param>
        /// <param name="uiEvent">対象 UI イベント</param>
        public UIClickEvent(
            UIClickType clickType,
            BaseUIEvent uiEvent)
        {
            ClickType = clickType;
            UIEvent = uiEvent;
        }
    }
}