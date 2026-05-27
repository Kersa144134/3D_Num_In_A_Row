// ======================================================
// TitleUIStateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : タイトル UI のキャンバス状態管理と初期選択制御を管理する
// ======================================================

using UnityEngine;
using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// タイトル UI の状態制御を管理するクラス
    /// </summary>
    public sealed class TitleUIStateController : BaseUIStateController
    {
        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        /// <summary>スタート UI キャンバス</summary>
        private readonly GameObject _startCanvas;

        /// <summary>オプション UI キャンバス</summary>
        private readonly GameObject _optionCanvas;

        // --------------------------------------------------
        // 初期選択ボタン
        // --------------------------------------------------
        /// <summary>ダイアログキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedDialogCanvasButton;

        /// <summary>スタートキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedStartCanvasButton;

        /// <summary>オプションキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedOptionCanvasButton;

        // --------------------------------------------------
        // 状態
        // --------------------------------------------------
        /// <summary>キャンバス状態キャッシュ</summary>
        private CanvasType _cachedCanvasType = CanvasType.None;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dialogCanvasArray">ダイアログキャンバス配列</param>
        /// <param name="startCanvas">スタートキャンバス</param>
        /// <param name="optionCanvas">オプションキャンバス</param>
        /// <param name="initialSelectedDialogCanvasButton">ダイアログキャンバス初期選択ボタン/param>
        /// <param name="initialSelectedStartCanvasButton">スタートキャンバス初期選択ボタン</param>
        /// <param name="initialSelectedOptionCanvasButton">オプションキャンバス初期選択ボタン</param>
        public TitleUIStateController(
            DialogCanvasDefinition[] dialogCanvasArray,
            GameObject startCanvas,
            GameObject optionCanvas,
            BaseButtonEvent initialSelectedDialogCanvasButton,
            BaseButtonEvent initialSelectedStartCanvasButton,
            BaseButtonEvent initialSelectedOptionCanvasButton)
            : base(dialogCanvasArray)
        {
            _startCanvas = startCanvas;
            _optionCanvas = optionCanvas;
            _initialSelectedDialogCanvasButton = initialSelectedDialogCanvasButton;
            _initialSelectedStartCanvasButton = initialSelectedStartCanvasButton;
            _initialSelectedOptionCanvasButton = initialSelectedOptionCanvasButton;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        /// <summary>
        /// スタートキャンバスを表示する
        /// </summary>
        public void ShowStartCanvas()
        {
            HideDialogCanvas();

            _startCanvas.SetActive(true);
            _optionCanvas.SetActive(false);

            // 現在状態を更新
            SetActiveCanvasType(CanvasType.Start);
        }

        /// <summary>
        /// オプションキャンバスを表示する
        /// </summary>
        public void ShowOptionCanvas()
        {
            HideDialogCanvas();

            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(true);

            // 現在状態を更新
            SetActiveCanvasType(CanvasType.Option);
        }

        /// <summary>
        /// ダイアログキャンバスを表示する
        /// </summary>
        /// <param name="dialogType">表示するダイアログ種別</param>
        public override void ShowDialogCanvas(in DialogType dialogType)
        {
            // ダイアログ表示前の状態をキャッシュ
            _cachedCanvasType = GetActiveCanvasType();

            base.ShowDialogCanvas(dialogType);

            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(false);
        }

        /// <summary>
        /// ダイアログキャンバスを非表示にする
        /// </summary>
        public override void HideDialogCanvas()
        {
            base.HideDialogCanvas();
            
            // キャッシュした状態に応じてキャンバスを表示
            switch (_cachedCanvasType)
            {
                case CanvasType.Start:
                    ShowStartCanvas();
                    break;

                case CanvasType.Option:
                    ShowOptionCanvas();
                    break;
            }
        }

        // --------------------------------------------------
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
                    return _initialSelectedDialogCanvasButton;

                case CanvasType.Start:
                    return _initialSelectedStartCanvasButton;

                case CanvasType.Option:
                    return _initialSelectedOptionCanvasButton;
            }

            return null;
        }

        /// <summary>
        /// ダイアログ用初期選択ボタンを取得する
        /// </summary>
        /// <returns>ダイアログ初期選択ボタン</returns>
        protected override BaseButtonEvent GetDialogInitialSelectedButton()
        {
            return _initialSelectedDialogCanvasButton;
        }
    }
}