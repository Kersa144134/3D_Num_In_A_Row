// ======================================================
// MainUIStateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : メイン UI のキャンバス状態管理と初期選択制御を管理する
// ======================================================

using System;
using UnityEngine;
using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// メイン UI の状態制御を管理するクラス
    /// </summary>
    public sealed class MainUIStateController
    {
        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        /// <summary>ダイアログ UI キャンバス配列</summary>
        private readonly DialogCanvasDefinition[] _dialogCanvasArray;

        // --------------------------------------------------
        // 初期選択ボタン
        // --------------------------------------------------
        /// <summary>ダイアログキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedDialogCanvasButton;

        /// <summary>ポーズキャンバス初期選択ボタン</summary>
        private readonly BaseButtonEvent _initialSelectedPauseCanvasButton;

        // --------------------------------------------------
        // 状態
        // --------------------------------------------------
        /// <summary>現在アクティブなキャンバス状態</summary>
        private CanvasType _activeCanvasType = CanvasType.None;

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
        /// <param name="dialogCanvasArray">ダイアログキャンバス配列</param>
        /// <param name="initialSelectedDialogCanvasButton">ダイアログキャンバス初期選択ボタン/param>
        /// <param name="initialSelectedPauseCanvasButton">ポーズキャンバス初期選択ボタン</param>
        public MainUIStateController(
            DialogCanvasDefinition[] dialogCanvasArray,
            BaseButtonEvent initialSelectedDialogCanvasButton,
            BaseButtonEvent initialSelectedPauseCanvasButton)
        {
            _dialogCanvasArray = dialogCanvasArray;
            _initialSelectedDialogCanvasButton = initialSelectedDialogCanvasButton;
            _initialSelectedPauseCanvasButton = initialSelectedPauseCanvasButton;
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

            // 現在状態を更新
            _activeCanvasType = CanvasType.Dialog;
        }

        /// <summary>
        /// ダイアログキャンバスを非表示にする
        /// </summary>
        public void HideDialogCanvas()
        {
            // 全ダイアログを非表示にする
            for (int i = 0; i < _dialogCanvasArray.Length; i++)
            {
                if (_dialogCanvasArray[i] == null)
                {
                    continue;
                }

                _dialogCanvasArray[i].Canvas.SetActive(false);
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

                case CanvasType.Pause:
                    return _initialSelectedPauseCanvasButton;
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