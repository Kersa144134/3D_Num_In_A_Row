// ======================================================
// TitleUIStateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : タイトル UI のキャンバス状態管理と初期選択制御を管理する
// ======================================================

using System;
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
        /// <summary>ダイアログ UI キャンバス配列</summary>
        private readonly DialogCanvasDefinition[] _dialogCanvasArray;

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
        /// <summary>現在アクティブなキャンバス状態</summary>
        private CanvasType _activeCanvasType = CanvasType.None;

        /// <summary>キャンバス状態キャッシュ</summary>
        private CanvasType _cachedCanvasType = CanvasType.None;

        /// <summary>キャンバスごとの最後に選択したボタンイベント</summary>
        private readonly BaseButtonEvent[] _lastSelectedButtonEvents =
            new BaseButtonEvent[Enum.GetValues(typeof(CanvasType)).Length];

        /// <summary>キャンバスごとの最後にホバー中のボタンイベント</summary>
        private readonly BaseButtonEvent[] _lastHoveredButtonEvents =
            new BaseButtonEvent[Enum.GetValues(typeof(CanvasType)).Length];

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dialogCanvas">ダイアログキャンバス</param>
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
        {
            _dialogCanvasArray = dialogCanvasArray;
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
        /// 現在アクティブなキャンバス状態を取得する
        /// </summary>
        /// <returns>現在アクティブなキャンバス種別</returns>
        public CanvasType GetActiveCanvasType()
        {
            return _activeCanvasType;
        }
        
        /// <summary>
        /// スタートキャンバスを表示する
        /// </summary>
        public void ShowStartCanvas()
        {
            for (int i = 0; i < _dialogCanvasArray.Length; i++)
            {
                if (_dialogCanvasArray[i] == null)
                {
                    continue;
                }

                _dialogCanvasArray[i].Canvas.SetActive(false);
            }
            
            _startCanvas.SetActive(true);
            _optionCanvas.SetActive(false);

            // 現在状態を更新
            _activeCanvasType = CanvasType.Start;
        }

        /// <summary>
        /// オプションキャンバスを表示する
        /// </summary>
        public void ShowOptionCanvas()
        {
            for (int i = 0; i < _dialogCanvasArray.Length; i++)
            {
                if (_dialogCanvasArray[i] == null)
                {
                    continue;
                }

                _dialogCanvasArray[i].Canvas.SetActive(false);
            }

            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(true);

            // 現在状態を更新
            _activeCanvasType = CanvasType.Option;
        }

        /// <summary>
        /// ダイアログキャンバスを表示する
        /// </summary>
        /// <param name="dialogType">表示するダイアログ種別</param>
        public void ShowDialogCanvas(in DialogType dialogType)
        {
            // 一度全ダイアログを非表示にする
            for (int i = 0; i < _dialogCanvasArray.Length; i++)
            {
                if (_dialogCanvasArray[i] == null)
                {
                    continue;
                }

                _dialogCanvasArray[i].Canvas.SetActive(false);
            }

            // 指定されたダイアログのみ表示
            for (int i = 0; i < _dialogCanvasArray.Length; i++)
            {
                if (_dialogCanvasArray[i] == null)
                {
                    continue;
                }

                if (_dialogCanvasArray[i].Type != dialogType)
                {
                    continue;
                }

                _dialogCanvasArray[i].Canvas.SetActive(true);
            }

            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(false);

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

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// キャンバス状態と入力状態に応じて選択対象ボタンを解決する
        /// </summary>
        /// <param name="canvasType">現在のキャンバス種別</param>
        /// <param name="isGamePadInput">ゲームパッド入力中かどうか</param>
        /// <param name="overrideButtonEvent">外部から指定されたボタン</param>
        /// <returns>選択対象のボタンイベント。該当なしの場合は null</returns>
        public BaseButtonEvent ResolveSelection(
            in CanvasType canvasType,
            in bool isGamePadInput,
            in BaseButtonEvent overrideButtonEvent = null)
        {
            // --------------------------------------------------
            // ダイアログキャンバス優先処理
            // --------------------------------------------------
            if (canvasType == CanvasType.Dialog)
            {
                // ダイアログは常に初期選択ボタンを返す
                return _initialSelectedDialogCanvasButton;
            }

            // --------------------------------------------------
            // ゲームパッド入力時
            // --------------------------------------------------
            if (isGamePadInput)
            {
                // 明示的に指定されたボタンがある場合
                if (overrideButtonEvent != null)
                {
                    return overrideButtonEvent;
                }

                // 初期選択ボタンを返す
                return GetInitialSelectedButton();
            }

            // --------------------------------------------------
            // マウス入力時
            // --------------------------------------------------
            BaseButtonEvent hoverButtonEvent =
                GetLastHoveredButtonEvent(canvasType);

            // ホバー入力状態が存在する場合
            if (hoverButtonEvent != null)
            {
                return hoverButtonEvent;
            }

            // --------------------------------------------------
            // フォールバック
            // --------------------------------------------------
            return null;
        }

        /// <summary>
        /// 現在アクティブなキャンバスに応じた初期選択ボタンを取得する
        /// </summary>
        /// <returns>初期選択ボタン</returns>
        public BaseButtonEvent GetInitialSelectedButton()
        {
            switch (_activeCanvasType)
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
        /// 指定キャンバスの最後に選択したボタンイベントを設定する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <param name="buttonEvent">設定するボタンイベント</param>
        public void SetLastSelectedButtonEvent(in CanvasType canvasType, in BaseButtonEvent buttonEvent)
        {
            int canvasIndex = (int)canvasType;

            _lastSelectedButtonEvents[canvasIndex] = buttonEvent;
        }

        /// <summary>
        /// 指定キャンバスの最後にホバー中のボタンイベントを設定する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <param name="buttonEvent">設定するボタンイベント</param>
        public void SetLastHoveredButtonEvent(in CanvasType canvasType, in BaseButtonEvent buttonEvent)
        {
            int canvasIndex = (int)canvasType;

            _lastHoveredButtonEvents[canvasIndex] = buttonEvent;
        }

        /// <summary>
        /// 指定キャンバスの最後に選択したボタンイベントを取得する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <returns>キャッシュ済みボタンイベント</returns>
        public BaseButtonEvent GetLastSelectedButtonEvent(in CanvasType canvasType)
        {
            int canvasIndex = (int)canvasType;

            return _lastSelectedButtonEvents[canvasIndex];
        }

        /// <summary>
        /// 指定キャンバスの最後にホバー中のボタンイベントを取得する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <returns>キャッシュ済みボタンイベント</returns>
        public BaseButtonEvent GetLastHoveredButtonEvent(in CanvasType canvasType)
        {
            int canvasIndex = (int)canvasType;

            return _lastHoveredButtonEvents[canvasIndex];
        }

        /// <summary>
        /// 指定キャンバスの最後に選択したボタンイベントをクリアする
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        public void ClearLastSelectedButtonEvent(in CanvasType canvasType)
        {
            int canvasIndex = (int)canvasType;

            _lastSelectedButtonEvents[canvasIndex] = null;
        }

        /// <summary>
        /// 最後にホバー中のボタンイベントをクリアする
        /// </summary>
        public void ClearLastHoveredButtonEvent(in CanvasType canvasType)
        {
            int canvasIndex = (int)canvasType;

            _lastHoveredButtonEvents[canvasIndex] = null;
        }
    }
}