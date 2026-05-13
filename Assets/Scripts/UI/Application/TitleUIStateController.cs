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
    public sealed class TitleUIStateController
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

        /// <summary>ダイアログ UI キャンバス</summary>
        private readonly GameObject _dialogCanvas;

        // --------------------------------------------------
        // 初期選択ボタン
        // --------------------------------------------------
        /// <summary>スタートキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedStartCanvasButton;

        /// <summary>オプションキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedOptionCanvasButton;

        /// <summary>ダイアログキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedDialogCanvasButton;

        // --------------------------------------------------
        // 状態
        // --------------------------------------------------
        /// <summary>現在アクティブなキャンバス状態</summary>
        private CanvasType _activeCanvasType = CanvasType.None;

        /// <summary>キャンバス状態キャッシュ</summary>
        private CanvasType _cachedCanvasType = CanvasType.None;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在アクティブなキャンバス状態</summary>
        public CanvasType ActiveCanvasType => _activeCanvasType;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startCanvas">スタートキャンバス</param>
        /// <param name="optionCanvas">オプションキャンバス</param>
        /// <param name="dialogCanvas">ダイアログキャンバス</param>
        /// <param name="initialSelectedStartCanvasButton">スタートキャンバス初期選択ボタン</param>
        /// <param name="initialSelectedOptionCanvasButton">オプションキャンバス初期選択ボタン</param>
        /// <param name="initialSelectedDialogCanvasButton">ダイアログキャンバス初期選択ボタン/param>
        public TitleUIStateController(
            GameObject startCanvas,
            GameObject optionCanvas,
            GameObject dialogCanvas,
            BaseButtonEvent initialSelectedStartCanvasButton,
            BaseButtonEvent initialSelectedOptionCanvasButton,
            BaseButtonEvent initialSelectedDialogCanvasButton)
        {
            _startCanvas = startCanvas;
            _optionCanvas = optionCanvas;
            _dialogCanvas = dialogCanvas;
            _initialSelectedStartCanvasButton = initialSelectedStartCanvasButton;
            _initialSelectedOptionCanvasButton = initialSelectedOptionCanvasButton;
            _initialSelectedDialogCanvasButton = initialSelectedDialogCanvasButton;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// スタートキャンバスを表示する
        /// </summary>
        public void ShowStartCanvas()
        {
            _startCanvas.SetActive(true);
            _optionCanvas.SetActive(false);
            _dialogCanvas.SetActive(false);

            // 現在状態を更新
            _activeCanvasType = CanvasType.Start;
        }

        /// <summary>
        /// オプションキャンバスを表示する
        /// </summary>
        public void ShowOptionCanvas()
        {
            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(true);
            _dialogCanvas.SetActive(false);

            // 現在状態を更新
            _activeCanvasType = CanvasType.Option;
        }

        /// <summary>
        /// ダイアログキャンバスを表示する
        /// </summary>
        public void ShowDialogCanvas()
        {
            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(false);
            _dialogCanvas.SetActive(true);

            // ダイアログ表示前の状態をキャッシュ
            _cachedCanvasType = _activeCanvasType;

            // 現在状態を更新
            _activeCanvasType = CanvasType.Dialog;
        }

        /// <summary>
        /// ダイアログキャンバスを非表示にする
        /// </summary>
        public void HideDialogCanvas()
        {
            // キャッシュした状態に応じて復帰
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

        /// <summary>
        /// 現在アクティブなキャンバスに応じた初期選択ボタンを取得する
        /// </summary>
        /// <returns>初期選択ボタン</returns>
        public BaseButtonEvent GetInitialSelectedButton()
        {
            switch (_activeCanvasType)
            {
                case CanvasType.Start:
                    return _initialSelectedStartCanvasButton;

                case CanvasType.Option:
                    return _initialSelectedOptionCanvasButton;

                case CanvasType.Dialog:
                    return _initialSelectedDialogCanvasButton;
            }

            return null;
        }
    }
}