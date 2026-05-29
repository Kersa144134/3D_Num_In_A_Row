// ======================================================
// ResultUIStateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : リザルト UI のキャンバス状態管理と初期選択制御を管理する
// ======================================================

using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// リザルト UI の状態制御を管理するクラス
    /// </summary>
    public sealed class ResultUIStateController : BaseUIStateController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>リザルトキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedResultCanvasButton;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dialogCanvasArray">ダイアログキャンバス配列</param>
        /// <param name="initialSelectedDialogCanvasButton">ダイアログキャンバス初期選択ボタン/param>
        /// <param name="initialSelectedPauseCanvasButton">ポーズキャンバス初期選択ボタン</param>
        public ResultUIStateController(
            DialogCanvasDefinition[] dialogCanvasArray,
            BaseButtonEvent initialSelectedResultCanvasButton)
            : base(dialogCanvasArray)
        {
            _initialSelectedResultCanvasButton = initialSelectedResultCanvasButton;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        /// <summary>
        /// リザルトキャンバスを表示する
        /// </summary>
        public void ShowResultCanvas()
        {
            // 現在状態を更新
            SetActiveCanvasType(CanvasType.Result);
        }

        /// <summary>
        /// リザルトキャンバスを非表示にする
        /// </summary>
        public void HidePauseCanvas()
        {
            // 現在状態を更新
            SetActiveCanvasType(CanvasType.None);
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
                case CanvasType.Result:
                    return _initialSelectedResultCanvasButton;
            }

            return null;
        }

        /// <summary>
        /// ダイアログ用初期選択ボタンを取得する
        /// </summary>
        /// <returns>ダイアログ初期選択ボタン</returns>
        protected override BaseButtonEvent GetDialogInitialSelectedButton()
        {
            // ダイアログ未使用なので null
            return null;
        }
    }
}