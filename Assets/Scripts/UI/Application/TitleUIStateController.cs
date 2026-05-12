// ======================================================
// TitleUIStateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : タイトル UI のキャンバス状態管理と初期選択制御を管理する
// ======================================================

using UISystem.Infrastructure;
using UnityEngine;

namespace UISystem.Application
{
    /// <summary>
    /// タイトル UI の状態制御を管理するクラス
    /// </summary>
    public sealed class TitleUIStateController
    {
        // ======================================================
        // 列挙型
        // ======================================================

        /// <summary>
        /// 現在アクティブなキャンバス種別
        /// </summary>
        public enum ActiveCanvasType
        {
            /// <summary>未選択</summary>
            None,

            /// <summary>スタートキャンバス</summary>
            Start,

            /// <summary>オプションキャンバス</summary>
            Option,

            /// <summary>ダイアログキャンバス</summary>
            Dialogue
        }

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
        private readonly GameObject _dialogueCanvas;

        // --------------------------------------------------
        // 初期選択ボタン
        // --------------------------------------------------
        /// <summary>スタートキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedStartCanvasButton;

        /// <summary>オプションキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedOptionCanvasButton;

        /// <summary>ダイアログキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedDialogueCanvasButton;

        // --------------------------------------------------
        // 状態
        // --------------------------------------------------
        /// <summary>現在アクティブなキャンバス状態</summary>
        private ActiveCanvasType _activeCanvasType = ActiveCanvasType.None;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在アクティブなキャンバス状態</summary>
        public ActiveCanvasType ActiveCanvas => _activeCanvasType;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="startCanvas">スタートキャンバス</param>
        /// <param name="optionCanvas">オプションキャンバス</param>
        /// <param name="dialogueCanvas">ダイアログキャンバス</param>
        /// <param name="initialSelectedStartCanvasButton">スタートキャンバス初期選択ボタン</param>
        /// <param name="initialSelectedOptionCanvasButton">オプションキャンバス初期選択ボタン</param>
        /// <param name="initialSelectedDialogueCanvasButton">ダイアログキャンバス初期選択ボタン/param>
        public TitleUIStateController(
            GameObject startCanvas,
            GameObject optionCanvas,
            GameObject dialogueCanvas,
            BaseButtonEvent initialSelectedStartCanvasButton,
            BaseButtonEvent initialSelectedOptionCanvasButton,
            BaseButtonEvent initialSelectedDialogueCanvasButton)
        {
            _startCanvas = startCanvas;
            _optionCanvas = optionCanvas;
            _dialogueCanvas = dialogueCanvas;
            _initialSelectedStartCanvasButton = initialSelectedStartCanvasButton;
            _initialSelectedOptionCanvasButton = initialSelectedOptionCanvasButton;
            _initialSelectedDialogueCanvasButton = initialSelectedDialogueCanvasButton;
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
            _dialogueCanvas.SetActive(false);

            // 現在状態を更新
            _activeCanvasType = ActiveCanvasType.Start;
        }

        /// <summary>
        /// オプションキャンバスを表示する
        /// </summary>
        public void ShowOptionCanvas()
        {
            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(true);
            _dialogueCanvas.SetActive(false);

            // 現在状態を更新
            _activeCanvasType = ActiveCanvasType.Option;
        }

        /// <summary>
        /// ダイアログキャンバスを表示する
        /// </summary>
        public void ShowDialogueCanvas()
        {
            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(false);
            _dialogueCanvas.SetActive(true);

            // 現在状態を更新
            _activeCanvasType = ActiveCanvasType.Dialogue;
        }

        /// <summary>
        /// 現在アクティブなキャンバスに応じた初期選択ボタンを取得する
        /// </summary>
        /// <returns>初期選択ボタン</returns>
        public BaseButtonEvent GetInitialSelectedButton()
        {
            switch (_activeCanvasType)
            {
                case ActiveCanvasType.Start:
                    return _initialSelectedStartCanvasButton;

                case ActiveCanvasType.Option:
                    return _initialSelectedOptionCanvasButton;

                case ActiveCanvasType.Dialogue:
                    return _initialSelectedDialogueCanvasButton;
            }

            return null;
        }
    }
}