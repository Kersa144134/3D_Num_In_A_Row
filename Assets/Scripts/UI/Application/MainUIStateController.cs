// ======================================================
// MainUIStateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : メイン UI のキャンバス状態管理と初期選択制御を管理する
// ======================================================

using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// メイン UI の状態制御を管理するクラス
    /// </summary>
    public sealed class MainUIStateController : BaseUIStateController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ポーズキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedPauseCanvasButton;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="uiActionButtonResolver">UI ボタンの参照解決クラス</param>
        /// <param name="dialogCanvasArray">ダイアログキャンバス配列</param>
        /// <param name="initialSelectedPauseCanvasButton">ポーズキャンバス初期選択ボタン</param>
        public MainUIStateController(
            in UIActionButtonResolver uiActionButtonResolver,
            in DialogCanvasDefinition[] dialogCanvasArray,
            in BaseButtonEvent initialSelectedPauseCanvasButton)
            : base(uiActionButtonResolver, dialogCanvasArray)
        {
            _initialSelectedPauseCanvasButton = initialSelectedPauseCanvasButton;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        /// <summary>
        /// ポーズキャンバスを表示する
        /// </summary>
        public void ShowPauseCanvas()
        {
            // 現在状態を更新
            SetActiveCanvasType(CanvasType.Pause);
        }

        /// <summary>
        /// ポーズキャンバスを非表示にする
        /// </summary>
        public void HidePauseCanvas()
        {
            // 現在状態を更新
            SetActiveCanvasType(CanvasType.None);
        }

        /// <summary>
        /// ダイアログキャンバスを非表示にする
        /// </summary>
        public override void HideDialogCanvas()
        {
            base.HideDialogCanvas();

            ShowPauseCanvas();
        }

        /// --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// 現在アクティブなキャンバスに応じた初期選択ボタンを取得する
        /// </summary>
        /// <returns>初期選択ボタン</returns>
        protected override BaseButtonEvent GetInitialSelectedButton()
        {
            switch (GetActiveCanvasType())
            {
                case CanvasType.Dialog:
                    return GetDialogInitialSelectedButton();

                case CanvasType.Pause:
                    return _initialSelectedPauseCanvasButton;
            }

            return null;
        }
    }
}