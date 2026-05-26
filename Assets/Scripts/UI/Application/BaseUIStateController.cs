// ======================================================
// BaseUIStateController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : UI ステート管理共通処理を提供する基底クラス
// ======================================================

using System;
using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// UI ステート管理共通処理を提供する基底クラス
    /// </summary>
    public abstract class BaseUIStateController
    {
        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        /// <summary>ダイアログ UI キャンバス配列</summary>
        protected readonly DialogCanvasDefinition[] DialogCanvasArray;

        // --------------------------------------------------
        // 状態
        // --------------------------------------------------
        /// <summary>現在アクティブなキャンバス状態</summary>
        protected CanvasType _activeCanvasType = CanvasType.None;

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
        protected BaseUIStateController(DialogCanvasDefinition[] dialogCanvasArray)
        {
            DialogCanvasArray = dialogCanvasArray;
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
        public virtual void ShowDialogCanvas(in DialogType dialogType)
        {
            // 全ダイアログを非表示にする
            HideAllDialogCanvas();

            // 指定ダイアログのみ有効化する
            for (int i = 0; i < DialogCanvasArray.Length; i++)
            {
                // 要素が未設定の場合はスキップする
                if (DialogCanvasArray[i] == null)
                {
                    continue;
                }

                // 対象ダイアログ以外はスキップする
                if (DialogCanvasArray[i].Type != dialogType)
                {
                    continue;
                }

                // 対象ダイアログを表示する
                DialogCanvasArray[i].Canvas.SetActive(true);
            }

            // 現在状態を更新する
            _activeCanvasType = CanvasType.Dialog;
        }

        /// <summary>
        /// 全ダイアログキャンバスを非表示にする
        /// </summary>
        protected void HideAllDialogCanvas()
        {
            // 全ダイアログを走査する
            for (int i = 0; i < DialogCanvasArray.Length; i++)
            {
                // 要素が未設定の場合はスキップする
                if (DialogCanvasArray[i] == null)
                {
                    continue;
                }

                // ダイアログを非表示にする
                DialogCanvasArray[i].Canvas.SetActive(false);
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
        /// <returns>選択対象ボタン</returns>
        public BaseButtonEvent ResolveSelection(
            in CanvasType canvasType,
            in bool isGamePadInput,
            in BaseButtonEvent overrideButtonEvent = null)
        {
            // ダイアログ中はダイアログ初期選択を返す
            if (canvasType == CanvasType.Dialog)
            {
                return GetDialogInitialSelectedButton();
            }

            // ゲームパッド入力時
            if (isGamePadInput)
            {
                // 外部指定ボタンが存在する場合
                if (overrideButtonEvent != null)
                {
                    return overrideButtonEvent;
                }

                // 初期選択ボタンを返す
                return GetInitialSelectedButton();
            }

            // ホバー中ボタンを取得する
            BaseButtonEvent hoverButtonEvent =
                GetLastHoveredButtonEvent(canvasType);

            // ホバー中ボタンが存在する場合
            if (hoverButtonEvent != null)
            {
                return hoverButtonEvent;
            }

            // 該当なし
            return null;
        }

        /// <summary>
        /// 現在状態に応じた初期選択ボタンを取得する
        /// </summary>
        /// <returns>初期選択ボタン</returns>
        public abstract BaseButtonEvent GetInitialSelectedButton();

        /// <summary>
        /// ダイアログ用初期選択ボタンを取得する
        /// </summary>
        /// <returns>ダイアログ初期選択ボタン</returns>
        protected abstract BaseButtonEvent GetDialogInitialSelectedButton();

        /// <summary>
        /// 指定キャンバスの最後に選択したボタンイベントを設定する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <param name="buttonEvent">設定ボタンイベント</param>
        public void SetLastSelectedButtonEvent(
            in CanvasType canvasType,
            in BaseButtonEvent buttonEvent)
        {
            // キャンバス種別を配列添字へ変換する
            int canvasIndex = (int)canvasType;

            // 最後の選択状態を保存する
            _lastSelectedButtonEvents[canvasIndex] = buttonEvent;
        }

        /// <summary>
        /// 指定キャンバスの最後にホバー中のボタンイベントを設定する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <param name="buttonEvent">設定ボタンイベント</param>
        public void SetLastHoveredButtonEvent(
            in CanvasType canvasType,
            in BaseButtonEvent buttonEvent)
        {
            // キャンバス種別を配列添字へ変換する
            int canvasIndex = (int)canvasType;

            // 最後のホバー状態を保存する
            _lastHoveredButtonEvents[canvasIndex] = buttonEvent;
        }

        /// <summary>
        /// 指定キャンバスの最後に選択したボタンイベントを取得する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <returns>最後に選択したボタンイベント</returns>
        public BaseButtonEvent GetLastSelectedButtonEvent(in CanvasType canvasType)
        {
            // キャンバス種別を配列添字へ変換する
            int canvasIndex = (int)canvasType;

            // キャッシュ済み選択状態を返す
            return _lastSelectedButtonEvents[canvasIndex];
        }

        /// <summary>
        /// 指定キャンバスの最後にホバー中のボタンイベントを取得する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <returns>最後にホバー中のボタンイベント</returns>
        public BaseButtonEvent GetLastHoveredButtonEvent(in CanvasType canvasType)
        {
            // キャンバス種別を配列添字へ変換する
            int canvasIndex = (int)canvasType;

            // キャッシュ済みホバー状態を返す
            return _lastHoveredButtonEvents[canvasIndex];
        }

        /// <summary>
        /// 指定キャンバスの最後に選択したボタンイベントをクリアする
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        public void ClearLastSelectedButtonEvent(in CanvasType canvasType)
        {
            // キャンバス種別を配列添字へ変換する
            int canvasIndex = (int)canvasType;

            // 選択状態をクリアする
            _lastSelectedButtonEvents[canvasIndex] = null;
        }

        /// <summary>
        /// 指定キャンバスの最後にホバー中のボタンイベントをクリアする
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        public void ClearLastHoveredButtonEvent(in CanvasType canvasType)
        {
            // キャンバス種別を配列添字へ変換する
            int canvasIndex = (int)canvasType;

            // ホバー状態をクリアする
            _lastHoveredButtonEvents[canvasIndex] = null;
        }
    }
}