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
        // コンポーネント参照
        // ======================================================

        /// <summary>UI ボタンの参照解決クラス</summary>
        protected readonly UIActionButtonResolver _uiActionButtonResolver;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        /// <summary>ダイアログ UI キャンバス配列</summary>
        protected readonly DialogCanvasDefinition[] _dialogCanvasArray;

        // --------------------------------------------------
        // 状態
        // --------------------------------------------------
        /// <summary>現在アクティブなキャンバス状態</summary>
        private CanvasType _activeCanvasType = CanvasType.None;

        /// <summary>現在アクティブなダイアログ種別</summary>
        protected DialogType _activeDialogType = DialogType.None;

        /// <summary>キャンバスごとの最後に選択したボタンイベント</summary>
        private readonly BaseButtonEvent[] _lastSelectedButtonEvents =
            new BaseButtonEvent[Enum.GetValues(typeof(CanvasType)).Length];

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="uIActionButtonResolver">UI ボタンの参照解決クラス</param>
        /// <param name="dialogCanvasArray">ダイアログキャンバス配列</param>
        protected BaseUIStateController(
            in UIActionButtonResolver uIActionButtonResolver,
            in DialogCanvasDefinition[] dialogCanvasArray)
        {
            _uiActionButtonResolver = uIActionButtonResolver;
            _dialogCanvasArray = dialogCanvasArray;
        }

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 現在状態に応じた初期選択ボタンを取得する
        /// </summary>
        /// <returns>初期選択ボタン</returns>
        protected abstract BaseButtonEvent GetInitialSelectedButton();

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
        /// 現在アクティブなダイアログ種別を取得する
        /// </summary>
        /// <returns>現在アクティブなキャンバス種別</returns>
        public DialogType GetActiveDialogType()
        {
            return _activeDialogType;
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
            for (int i = 0; i < _dialogCanvasArray.Length; i++)
            {
                if (_dialogCanvasArray[i] == null)
                {
                    continue;
                }

                // 対象ダイアログ以外はスキップ
                if (_dialogCanvasArray[i].Type != dialogType)
                {
                    continue;
                }

                // 対象ダイアログを表示する
                _dialogCanvasArray[i].Canvas.SetActive(true);
            }

            // 現在状態を更新する
            _activeCanvasType = CanvasType.Dialog;
            _activeDialogType = dialogType;
        }

        /// <summary>
        /// ダイアログキャンバスを非表示にする
        /// </summary>
        public virtual void HideDialogCanvas()
        {
            HideAllDialogCanvas();
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

            return null;
        }

        /// <summary>
        /// 指定キャンバスの最後に選択したボタンイベントを設定する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <param name="buttonEvent">設定ボタンイベント</param>
        public void SetLastSelectedButtonEvent(
            in CanvasType canvasType,
            in BaseButtonEvent buttonEvent)
        {
            // キャンバス種別を int へ変換する
            int canvasIndex = (int)canvasType;

            // 最後の選択状態を保存する
            _lastSelectedButtonEvents[canvasIndex] = buttonEvent;
        }

        /// <summary>
        /// 指定キャンバスの最後に選択したボタンイベントを取得する
        /// </summary>
        /// <param name="canvasType">対象キャンバス種別</param>
        /// <returns>最後に選択したボタンイベント</returns>
        public BaseButtonEvent GetLastSelectedButtonEvent(in CanvasType canvasType)
        {
            // キャンバス種別を int へ変換する
            int canvasIndex = (int)canvasType;

            // キャッシュ済み選択状態を返す
            return _lastSelectedButtonEvents[canvasIndex];
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 現在アクティブなキャンバス状態をセットする
        /// </summary>
        /// <returns>現在アクティブなキャンバス種別</returns>
        protected void SetActiveCanvasType(in CanvasType activeCanvasType)
        {
            _activeCanvasType = activeCanvasType;
        }

        /// <summary>
        /// 全ダイアログキャンバスを非表示にする
        /// </summary>
        protected void HideAllDialogCanvas()
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

        /// <summary>
        /// ダイアログ用初期選択ボタンを取得する
        /// DialogType に応じて Yes / No を切り替える
        /// </summary>
        protected BaseButtonEvent GetDialogInitialSelectedButton()
        {
            // --------------------------------------------------
            // 現在のダイアログ種別を取得
            // --------------------------------------------------
            DialogType dialogType = _activeDialogType;

            // --------------------------------------------------
            // DialogType に応じて初期選択を決定
            // --------------------------------------------------
            switch (dialogType)
            {
                case DialogType.StartGame:
                case DialogType.Pause:
                    return _uiActionButtonResolver.GetDialogButton(UIActionType.DialogYes, dialogType);

                case DialogType.ExitGame:
                    return _uiActionButtonResolver.GetDialogButton(UIActionType.DialogNo, dialogType);
            }

            return null;
        }
    }
}