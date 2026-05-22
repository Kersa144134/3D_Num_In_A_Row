// ======================================================
// TitleUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : タイトルシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using InputSystem.Presentation;
using OptionSystem.Domain;
using OptionSystem.Infrastructure;
using OptionSystem.Presentation;
using UISystem.Application;
using UISystem.Domain;
using UISystem.Infrastructure;
using UpdateSystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// タイトルシーンにおける UI 演出を管理するプレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.TitleUIPresenter)]
    public sealed class TitleUIPresenter : BaseUIPresenter, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("タイトルシーン固有インスペクタ")]

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        [Header("キャンバス")]
        /// <summary>スタート関連の UI を表示するキャンバス</summary>
        [SerializeField]
        private GameObject _startCanvas;

        /// <summary>オプション関連の UI を表示するキャンバス</summary>
        [SerializeField]
        private GameObject _optionCanvas;

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        [Header("ボタン")]
        /// <summary>タイトルシーン用の通常ボタン配列</summary>
        [SerializeField]
        private NormalButton[] _titleNormalButtons;

        /// <summary>タイトルシーン用のオプションボタン配列</summary>
        [SerializeField]
        private OptionButtonGroup[] _titleOptionButtons;

        // --------------------------------------------------
        // ボタンカラー
        // --------------------------------------------------
        [Header("通常ボタンカラー")]
        /// <summary>通常フォーカス時カラー</summary>
        [SerializeField]
        private Color _normalFocusOnColor = Color.white;

        /// <summary>通常非フォーカス時カラー</summary>
        [SerializeField]
        private Color _normalFocusOffColor = Color.gray;

        [Header("オプションボタンカラー")]
        /// <summary>オプション選択時カラー</summary>
        [SerializeField]
        private Color _optionSelectOnColor = Color.white;

        /// <summary>オプション非選択時カラー</summary>
        [SerializeField]
        private Color _optionSelectOffColor = Color.gray;

        /// <summary>オプションフォーカス時カラー</summary>
        [SerializeField]
        private Color _optionFocusOnColor = Color.white;

        /// <summary>オプション非フォーカス時カラー</summary>
        [SerializeField]
        private Color _optionFocusOffColor = Color.gray;

        // --------------------------------------------------
        // 初期選択ボタン
        // --------------------------------------------------
        [Header("初期選択ボタン")]
        /// <summary>スタートキャンバス初期選択ボタン</summary>
        [SerializeField]
        private BaseButtonEvent _initialSelectedStartCanvasButton;

        /// <summary>オプションキャンバス初期選択ボタン</summary>
        [SerializeField]
        private BaseButtonEvent _initialSelectedOptionCanvasButton;

        // --------------------------------------------------
        // オプション初期選択インデックス
        // --------------------------------------------------
        [Header("オプション初期選択インデックス")]
        /// <summary>オプション種別ごとの選択インデックス管理テーブル</summary>
        [SerializeField]
        private OptionSelectionIndexTable _optionIndexTable;

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        [Header("アニメーター")]
        /// <summary>ボードの GameObject ルートアニメーター</summary>
        [SerializeField]
        private Animator _boardAnimator;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI 管理
        // --------------------------------------------------
        /// <summary>ビュー</summary>
        private TitleUIView _titleUIView;

        /// <summary>イベントを仲介するクラス</summary>
        private readonly UIEventRouter _eventRouter = new UIEventRouter();

        /// <summary>通常ボタンの参照解決クラス</summary>
        private NormalButtonResolver _normalButtonResolver;

        /// <summary>OptionButtonBinder 生成クラス</summary>
        private OptionButtonBinderFactory _optionButtonBinderFactory;

        /// <summary>タイトル UI のキャンバス状態と初期ボタン選択状態を管理するクラス</summary>
        private TitleUIStateController _titleUIStateController;

        // --------------------------------------------------
        // システム参照
        // --------------------------------------------------
        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        /// <summary>
        /// オプション選択状態リポジトリ
        /// </summary>
        private readonly PlayerPrefsOptionSelectionRepository _repository
            = new PlayerPrefsOptionSelectionRepository();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

        /// <summary>ゲームパッド入力状態フラグ</summary>
        private bool _isGamePadInput = false;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 通常ボタンイベント辞書
        /// </summary>
        private Dictionary<UIActionType, NormalButtonEvent> _normalButtonEventTable
            = new Dictionary<UIActionType, NormalButtonEvent>();

        /// <summary>
        /// オプション UI バインダー辞書
        /// </summary>
        private Dictionary<OptionType, OptionButtonBinder> _optionBinders
            = new Dictionary<OptionType, OptionButtonBinder>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>BoardSize パラメータ名</summary>
        private static readonly int BOARD_SIZE_HASH = Animator.StringToHash("BoardSize");

        /// <summary>ボードアニメーション無効値</summary>
        private const int BOARD_ANIMATION_DISABLED = -1;

        /// <summary>3 x 3 ボードサイズ</summary>
        private const int BOARD_SIZE_THREE = 3;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>ルーター用購読管理</summary>
        private readonly CompositeDisposable _routerDisposables = new CompositeDisposable();

        /// <summary>投影切り替え用 Subject</summary>
        private readonly Subject<bool> _onSwitchProjection = new Subject<bool>();

        /// <summary>フォーカス座標通知用 Subject</summary>
        private readonly Subject<Vector2> _onFocusPosition =  new Subject<Vector2>();

        /// <summary>フォーカス座標通知ストリーム</summary>
        public IObservable<Vector2> OnFocusPosition => _onFocusPosition;

        /// <summary>ゲームオプション更新通知用 Subject</summary>
        private readonly Subject<OptionButtonData> _onUpdateGameOption = new Subject<OptionButtonData>();

        /// <summary>ゲームオプション更新通知ストリーム</summary>
        public IObservable<OptionButtonData> OnUpdateGameOption => _onUpdateGameOption;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;

            if (_gameOptionManager == null ||
                _inputManager == null ||
                _dialogUICollector == null ||
                _titleNormalButtons == null ||
                _titleOptionButtons == null ||
                _initialSelectedStartCanvasButton == null ||
                _initialSelectedOptionCanvasButton == null)
            {
                Debug.LogError("[TitleUIPresenter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            // --------------------------------------------------
            // ビュー生成
            // --------------------------------------------------
            _titleUIView = new TitleUIView(
                _pointer,
                _normalFocusOnColor,
                _normalFocusOffColor,
                _optionSelectOnColor,
                _optionSelectOffColor,
                _optionFocusOnColor,
                _optionFocusOffColor
            );

            // --------------------------------------------------
            // 通常ボタン初期化
            // --------------------------------------------------
            // 通常ボタン初期化
            InitializeNormalButtons();

            // 通常ボタンの参照解決クラス生成
            _normalButtonResolver = new NormalButtonResolver(_normalButtonEventTable);

            // --------------------------------------------------
            // オプションボタン初期化
            // --------------------------------------------------
            // オプション初期選択テーブル初期化
            _optionIndexTable.Initialize();

            // 初期選択インデックス取得用リーダー
            IOptionSelectionIndexReader reader;

            // デバッグ用
            _repository.Delete();

            // 保存データが存在する場合
            if (_repository.HasSavedData())
            {
                // PlayerPrefs をそのまま使用
                reader = _repository;
            }
            else
            {
                // ScriptableObject の初期値を使用するため初期化
                _optionIndexTable.Initialize();

                // 未保存データのみを PlayerPrefs に同期
                foreach (OptionType type in Enum.GetValues(typeof(OptionType)))
                {
                    // 保存済みならスキップ
                    if (_repository.Exists(type))
                    {
                        continue;
                    }

                    // ScriptableObject の初期値取得
                    int index = _optionIndexTable.Get(type);

                    // 保存
                    _repository.Save(type, index);
                }

                // 初期データとして Repository を使用
                reader = _repository;
            }

            // バインダーファクトリ生成
            _optionButtonBinderFactory = new OptionButtonBinderFactory(reader);

            // オプションボタン初期化
            InitializeOptionButtons();

            // --------------------------------------------------
            // パネル初期化
            // --------------------------------------------------
            InitializePanelEvents();
            
            // --------------------------------------------------
            // キャンバス初期化
            // --------------------------------------------------
            // キャンバス状態管理クラス生成
            _titleUIStateController = new TitleUIStateController(
                _dialogCanvasArray,
                _startCanvas,
                _optionCanvas,
                _normalButtonResolver.GetButton(UIActionType.DialogYes),
                _initialSelectedStartCanvasButton,
                _initialSelectedOptionCanvasButton
            );

            // スタートキャンバスを表示
            _titleUIStateController.ShowStartCanvas();

            // アニメーター速度をタイムスケール非依存に設定
            SetAnimatorUnscaledTime(_boardAnimator);
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (_isInputLock)
            {
                return;
            }
            
            // ポインター取得
            Vector2 screenPos = _inputManager.Pointer;

            // ビューへ反映
            _titleUIView.UpdatePointer(screenPos);
        }

        protected override void OnExitInternal()
        {
            base.OnExitInternal();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        /// <param name="inputLock">入力ロック状態ストリーム</param>
        /// <param name="gamePadInput">ゲームパッド入力状態ストリーム</param>
        public void BindStreams(
            in IObservable<bool> inputLock,
            in IObservable<bool> gamePadInput)
        {
            inputLock
                .Subscribe(isLock =>
                {
                    _isInputLock = isLock;
                })
                .AddTo(_disposables);


            gamePadInput
                .DistinctUntilChanged()
                .Subscribe(isGamePadInput =>
                {
                    // 現在の入力デバイス状態を保持
                    _isGamePadInput = isGamePadInput;

                    // 現在アクティブなキャンバス状態を取得
                    CanvasType activeCanvasType = _titleUIStateController.GetActiveCanvasType();

                    // 最後に選択していたボタンを取得
                    BaseButtonEvent selectedButtonEvent =
                        _titleUIStateController.GetLastSelectedButtonEvent(activeCanvasType);

                    // 入力状態に応じて初期選択を適用
                    SetSelectionState(activeCanvasType, selectedButtonEvent);
                })
                .AddTo(_disposables);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// イベント購読
        /// </summary>
        protected override void Subscribe()
        {
            base.Subscribe();
            
            // クリック通知
            _eventRouter.OnClick
                .Subscribe(clickEvent =>
                {
                    // --------------------------------------------------
                    // 通常ボタン
                    // --------------------------------------------------
                    if (clickEvent.UIEvent is NormalButtonEvent normalButton)
                    {
                        // 左クリックのみ処理
                        if (clickEvent.ClickType == UIClickType.Left)
                        {
                            OnNormalButtonClick(normalButton);
                        }

                        return;
                    }

                    // --------------------------------------------------
                    // オプションボタン
                    // --------------------------------------------------
                    if (clickEvent.UIEvent is OptionButtonEvent optionButton)
                    {
                        // 左クリックのみ処理
                        if (clickEvent.ClickType == UIClickType.Left)
                        {
                            OnOptionButtonClick(optionButton);
                        }

                        return;
                    }

                    // --------------------------------------------------
                    // パネル
                    // --------------------------------------------------
                    if (clickEvent.UIEvent is BasePanelEvent panelEvent)
                    {
                        OnPanelClick(panelEvent);
                    }
                })
                .AddTo(_routerDisposables);

            // ホバー通知
            _eventRouter.OnHover
                .Subscribe(uiEvent =>
                {
                    // ボタンイベント判定
                    if (uiEvent is not BaseButtonEvent buttonEvent)
                    {
                        return;
                    }

                    // 現在アクティブなキャンバス状態を取得
                    CanvasType activeCanvasType = _titleUIStateController.GetActiveCanvasType();

                    // 選択対象のボタンイベントをキャッシュ
                    _titleUIStateController.SetLastHoveredButtonEvent(activeCanvasType, buttonEvent);

                    OnSelectButton(buttonEvent);
                })
                .AddTo(_routerDisposables);

            // ホバー解除通知
            _eventRouter.OnUnHover
                .Subscribe(uiEvent =>
                {
                    // ボタンイベント判定
                    if (uiEvent is not BaseButtonEvent buttonEvent)
                    {
                        return;
                    }

                    // 現在アクティブなキャンバス状態を取得
                    CanvasType activeCanvasType = _titleUIStateController.GetActiveCanvasType();

                    // 選択対象のボタンイベントをクリア
                    _titleUIStateController.ClearLastHoveredButtonEvent(activeCanvasType);

                    // ホバー対象を選択
                    OnUnSelectButton();
                })
                .AddTo(_routerDisposables);

            // フォーカス通知
            _eventRouter.OnFocus
                .Subscribe(uiEvent =>
                {
                    // ボタンイベント判定
                    if (uiEvent is not BaseButtonEvent buttonEvent)
                    {
                        return;
                    }

                    OnFocusButton(buttonEvent);
                })
                .AddTo(_routerDisposables);

            // フォーカス解除通知
            _eventRouter.OnUnFocus
                .Subscribe(uiEvent =>
                {
                    // ボタンイベント判定
                    if (uiEvent is not BaseButtonEvent buttonEvent)
                    {
                        return;
                    }

                    OnUnFocusButton(buttonEvent);
                })
                .AddTo(_routerDisposables);
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            _disposables?.Dispose();
            _routerDisposables?.Dispose();
            _eventRouter?.Dispose();
        }

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// 通常ボタンイベントを初期化する
        /// </summary>
        private void InitializeNormalButtons()
        {
            // --------------------------------------------------
            // NormalButton 配列生成
            // --------------------------------------------------
            // ダイアログ側ボタン数
            int dialogCount = _dialogUICollector.Buttons != null
                ? _dialogUICollector.Buttons.Length
                : 0;
            // タイトル側ボタン数
            int titleCount = _titleNormalButtons != null
                ? _titleNormalButtons.Length
                : 0;

            NormalButton[] normalButtons = new NormalButton[dialogCount + titleCount];

            // --------------------------------------------------
            // ダイアログボタンコピー
            // --------------------------------------------------
            for (int i = 0; i < dialogCount; i++)
            {
                normalButtons[i] = _dialogUICollector.Buttons[i];
            }

            // --------------------------------------------------
            // タイトルボタンコピー
            // --------------------------------------------------
            for (int i = 0; i < titleCount; i++)
            {
                normalButtons[dialogCount + i] = _titleNormalButtons[i];
            }

            // --------------------------------------------------
            // 辞書生成
            // --------------------------------------------------
            _normalButtonEventTable = _buttonDictionaryBuilder.BuildNormalButtons(normalButtons);

            // --------------------------------------------------
            // イベント登録
            // --------------------------------------------------
            foreach (NormalButtonEvent buttonEvent in _normalButtonEventTable.Values)
            {
                _eventRouter.RegisterNormalButton(buttonEvent);
            }
        }

        /// <summary>
        /// Grid Layout Group 配下のオプションボタンを初期化する
        /// </summary>
        private void InitializeOptionButtons()
        {
            // --------------------------------------------------
            // 辞書生成
            // --------------------------------------------------
            _optionBinders = _buttonDictionaryBuilder.BuildOptionButtons(_optionButtonBinderFactory, _titleOptionButtons);

            // --------------------------------------------------
            // イベント登録
            // --------------------------------------------------
            foreach (OptionButtonBinder binder in _optionBinders.Values)
            {
                _eventRouter.RegisterOptionButtons(binder.Events);
            }
        }

        /// <summary>
        /// パネルイベントを初期化する
        /// </summary>
        private void InitializePanelEvents()
        {
            // --------------------------------------------------
            // イベント登録
            // --------------------------------------------------
            foreach (BasePanelEvent panelEvent in _dialogUICollector.Panels)
            {
                _eventRouter.RegisterPanelEvent(panelEvent);
            }
        }

        /// <summary>
        /// キャンバスと入力状態に応じて選択状態を更新する
        /// </summary>
        /// <param name="canvasType">対象キャンバス</param>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void SetSelectionState(
            in CanvasType canvasType,
            in BaseButtonEvent buttonEvent = null)
        {
            // 選択状態をリセット
            OnUnSelectButton();

            // 選択対象を解決
            BaseButtonEvent targetButton = _titleUIStateController.ResolveSelection(
                canvasType,
                _isGamePadInput,
                buttonEvent);

            if (targetButton == null)
            {
                return;
            }

            // 選択状態を適用
            OnSelectButton(targetButton);
        }

        /// <summary>
        /// ボタンイベントに応じてフォーカス状態を設定する
        /// </summary>
        /// <param name="buttonEvent">対象のボタンイベント</param>
        /// <param name="isFocus">フォーカス状態かどうか</param>
        private void SetFocusState(in BaseButtonEvent buttonEvent, in bool isFocus)
        {
            // 通常ボタンイベント
            if (buttonEvent is NormalButtonEvent normalButton)
            {
                // 通常ボタンのフォーカス状態を有効化
                _titleUIView.SetNormalFocus(normalButton.Button, isFocus);

                return;
            }

            // オプションボタンイベント
            if (buttonEvent is OptionButtonEvent optionButton)
            {
                // オプションボタンのフォーカス状態を有効化
                _titleUIView.SetOptionFocus(optionButton.Button, isFocus);

                return;
            }
        }

        /// <summary>
        /// ボードサイズに応じて ConnectCount の表示状態と初期選択を制御する
        /// </summary>
        private void ApplyBoardSizeDependentConnectCount(in int boardSize)
        {
            // ConnectCount バインダー取得
            if (!_optionBinders.TryGetValue(OptionType.ConnectCount, out OptionButtonBinder connectCountBinder))
            {
                return;
            }

            // --------------------------------------------------
            // 3 x 3 判定
            // --------------------------------------------------
            bool isThreeSize = (boardSize == BOARD_SIZE_THREE);

            // 3 x 3 の場合は強制的に先頭を選択状態にする
            if (isThreeSize)
            {
                // インデックス 0 を選択
                connectCountBinder.SelectByIndex(0);

                // ビューへ選択状態を反映
                _titleUIView.ApplyButtonSelectionState(
                    connectCountBinder.Buttons,
                    connectCountBinder.SelectStateArray);
            }

            for (int i = 1; i < connectCountBinder.Buttons.Length; i++)
            {
                // インデックス 1 以降のボタンオブジェクト非表示
                connectCountBinder.Events[i].gameObject.SetActive(!isThreeSize);
            }
        }

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// NormalButton クリック時の処理
        /// UIActionType に応じて各UI遷移・状態更新を実行する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnNormalButtonClick(NormalButtonEvent buttonEvent)
        {
            // UI アクション種別へ変換できない場合は処理なし
            if (!_normalButtonResolver.TryGetType(buttonEvent, out UIActionType actionType))
            {
                return;
            }

            // --------------------------------------------------
            // ダイアログ：YES
            // --------------------------------------------------
            if (actionType == UIActionType.DialogYes)
            {
                // ダイアログボタンを非表示にする
                _normalButtonResolver.GetButton(UIActionType.DialogYes).gameObject.SetActive(false);
                _normalButtonResolver.GetButton(UIActionType.DialogNo).gameObject.SetActive(false);

                // ダイアログデータ取得
                DialogEvent dialogEvent = buttonEvent.gameObject.GetComponentInParent<DialogEvent>();

                if (dialogEvent != null)
                {
                    // ダイアログイベント発火
                    dialogEvent.InvokeEvent();
                }

                return;
            }

            // --------------------------------------------------
            // ダイアログ：NO
            // --------------------------------------------------
            if (actionType == UIActionType.DialogNo)
            {
                // ダイアログキャンバスを非表示にする
                _titleUIStateController.HideDialogCanvas();

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _titleUIStateController.GetActiveCanvasType();

                // 最後に選択されていたボタンを取得する
                BaseButtonEvent selectedButtonEvent =
                    _titleUIStateController.GetLastSelectedButtonEvent(nextCanvasType);

                // 入力状態に応じて初期選択を適用する
                SetSelectionState(nextCanvasType, selectedButtonEvent);

                // ダイアログ非表示を通知する
                _onDialogVisibleChanged.OnNext(false);

                return;
            }

            // --------------------------------------------------
            // スタートボタン
            // --------------------------------------------------
            // タイトルスタートボタン押下時の処理
            if (actionType == UIActionType.TitleStart)
            {
                // ダイアログキャンバスを表示する
                _titleUIStateController.ShowDialogCanvas(DialogType.Confirm);

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _titleUIStateController.GetActiveCanvasType();

                // ダイアログ用ボタンを表示する
                _normalButtonResolver.GetButton(UIActionType.DialogYes).gameObject.SetActive(true);
                _normalButtonResolver.GetButton(UIActionType.DialogNo).gameObject.SetActive(true);

                // 初期フォーカスを Yes ボタンに設定する
                SetSelectionState(nextCanvasType, _normalButtonResolver.GetButton(UIActionType.DialogYes));

                // ダイアログ表示を通知する
                _onDialogVisibleChanged.OnNext(true);

                return;
            }

            // --------------------------------------------------
            // オプション
            // --------------------------------------------------
            if (actionType == UIActionType.TitleOption)
            {
                // オプションキャンバスを表示する
                _titleUIStateController.ShowOptionCanvas();

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _titleUIStateController.GetActiveCanvasType();

                // 最後に選択されていたボタンを取得する
                BaseButtonEvent selectedButtonEvent =
                    _titleUIStateController.GetLastSelectedButtonEvent(nextCanvasType);

                // 直前の選択が OptionButton の場合はそれを復元する
                if (selectedButtonEvent is OptionButtonEvent optionButton)
                {
                    SetSelectionState(nextCanvasType, optionButton);
                }
                else
                {
                    // 入力状態に応じて初期選択を適用する
                    SetSelectionState(nextCanvasType);
                }

                // 現在のボードサイズを取得する
                int boardSize = _gameOptionManager.BoardSize;

                // ConnectCount オプション表示制御を適用する
                ApplyBoardSizeDependentConnectCount(boardSize);

                // 各オプション種別ごとに状態を復元する
                foreach (KeyValuePair<OptionType, OptionButtonBinder> binder in _optionBinders)
                {
                    // 現在保存されている選択インデックスを取得する
                    int index = _repository.Get(binder.Key);

                    // バインダー内部の選択状態を更新する
                    binder.Value.SelectByIndex(index);

                    // UI表示へ反映する
                    _titleUIView.ApplyButtonSelectionState(
                        binder.Value.Buttons,
                        binder.Value.SelectStateArray);
                }

                // ボード変更アニメーションを実行する
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, boardSize);

                return;
            }

            // --------------------------------------------------
            // オプションキャンセル
            // --------------------------------------------------
            if (actionType == UIActionType.TitleOptionCancel)
            {
                // スタートキャンバスを表示する
                _titleUIStateController.ShowStartCanvas();

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _titleUIStateController.GetActiveCanvasType();

                // 初期フォーカスをオプションボタンに設定する
                SetSelectionState(nextCanvasType, _normalButtonResolver.GetButton(UIActionType.TitleOption));

                // ボードアニメーションをリセットする
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, BOARD_ANIMATION_DISABLED);

                return;
            }

            // --------------------------------------------------
            // オプション確定
            // --------------------------------------------------
            if (actionType == UIActionType.TitleOptionDecide)
            {
                // スタートキャンバスを表示する
                _titleUIStateController.ShowStartCanvas();

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _titleUIStateController.GetActiveCanvasType();

                // 初期フォーカスをスタートボタンに設定する
                SetSelectionState(nextCanvasType, _normalButtonResolver.GetButton(UIActionType.TitleStart));

                // 各オプションの選択状態を保存する
                foreach (KeyValuePair<OptionType, OptionButtonBinder> binder in _optionBinders)
                {
                    // 現在選択されているインデックスを取得する
                    int currentIndex = binder.Value.GetCurrentSelectedIndex();

                    // 永続データへ保存する
                    _repository.Save(binder.Key, currentIndex);
                }

                // ボードアニメーションをリセットする
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, BOARD_ANIMATION_DISABLED);
            }
        }

        /// <summary>
        /// オプションボタンクリック時
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnOptionButtonClick(OptionButtonEvent buttonEvent)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // 種別取得
            OptionType type = buttonEvent.Data.Type;

            // バインダー取得
            if (!_optionBinders.TryGetValue(type, out OptionButtonBinder binder))
            {
                return;
            }

            // ボタン選択処理
            binder.SelectByButton(buttonEvent.Button);

            // 選択状態をビューへ反映
            _titleUIView.ApplyButtonSelectionState(
                binder.Buttons,
                binder.SelectStateArray);

            // オプション更新通知
            _onUpdateGameOption.OnNext(buttonEvent.Data);

            // ボードサイズが変更された場合
            if (type == OptionType.BoardSize)
            {
                int boardSize = (int)buttonEvent.Data.BoardSizeType;

                // ConnectCount 制御
                ApplyBoardSizeDependentConnectCount(boardSize);

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, boardSize);
            }
        }

        /// <summary>
        /// パネルクリック時
        /// </summary>
        /// <param name="panelEvent">対象パネルイベント</param>
        private void OnPanelClick(BasePanelEvent panelEvent)
        {
            if (panelEvent == null)
            {
                return;
            }

            if (panelEvent is NormalPanelEvent)
            {
                // 現在アクティブなキャンバス状態を取得
                CanvasType activeCanvasType = _titleUIStateController.GetActiveCanvasType();

                // ダイアログの場合
                if (activeCanvasType == CanvasType.Dialog)
                {
                    // ダイアログキャンバス非表示
                    _titleUIStateController.HideDialogCanvas();

                    // 遷移先のキャンバス状態を取得
                    CanvasType nextCanvasType = _titleUIStateController.GetActiveCanvasType();

                    // 最後に選択していたボタンを取得
                    BaseButtonEvent selectedButtonEvent =
                        _titleUIStateController.GetLastSelectedButtonEvent(nextCanvasType);

                    // 遷移先のキャンバスで最後に選択していたボタンを適用
                    SetSelectionState(nextCanvasType, selectedButtonEvent);

                    // ダイアログ非表示を通知
                    _onDialogVisibleChanged.OnNext(false);
                }

                return;
            }
        }

        /// <summary>
        /// ボタンへフォーカス状態を適用し、フォーカス座標を通知する
        /// </summary>
        /// <param name="uiEvent">対象イベント</param>
        private void OnFocusButton(BaseUIEvent uiEvent)
        {
            // ボタンイベント判定
            if (uiEvent is not BaseButtonEvent buttonEvent)
            {
                return;
            }

            // 現在アクティブなキャンバス状態を取得
            CanvasType activeCanvasType = _titleUIStateController.GetActiveCanvasType();

            // オプションキャンバスの場合
            if (activeCanvasType == CanvasType.Option)
            {
                // 対象ボタンが OptionButton の場合
                if (buttonEvent is OptionButtonEvent optionButton)
                {
                    // 選択対象のボタンイベントをキャッシュ
                    _titleUIStateController.SetLastSelectedButtonEvent(activeCanvasType, buttonEvent);
                }
            }
            else
            {
                // 選択対象のボタンイベントをキャッシュ
                _titleUIStateController.SetLastSelectedButtonEvent(activeCanvasType, buttonEvent);
            }

            // フォーカス状態表示
            SetFocusState(buttonEvent, true);

            // スクリーン座標変換
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
                null,
                buttonEvent.RectTransform.position);

            // フォーカス通知
            _onFocusPosition.OnNext(screenPosition);

            // ターゲット検出状態を有効化
            UpdatePointerTargetAnimation(true);
        }

        /// <summary>
        /// ボタンのフォーカス状態を解除する
        /// </summary>
        /// <param name="uiEvent">対象イベント</param>
        private void OnUnFocusButton(BaseUIEvent uiEvent)
        {
            // ボタンイベント判定
            if (uiEvent is not BaseButtonEvent buttonEvent)
            {
                return;
            }

            // フォーカス状態非表示
            SetFocusState(buttonEvent, false);

            // ターゲット検出状態を解除
            UpdatePointerTargetAnimation(false);
        }

        /// <summary>
        /// EventSystem の選択状態を変更する
        /// </summary>
        /// <param name="uiEvent">対象イベント</param>
        private void OnSelectButton(BaseUIEvent uiEvent)
        {
            // ボタンイベント判定
            if (uiEvent is not BaseButtonEvent buttonEvent)
            {
                return;
            }

            // 現在選択中のオブジェクト取得
            GameObject currentSelectedObject = _eventSystem.currentSelectedGameObject;

            // 同一オブジェクトが選択されている場合
            if (currentSelectedObject == buttonEvent.gameObject)
            {
                return;
            }

            // 選択状態を更新
            _eventSystem.SetSelectedGameObject(buttonEvent.gameObject);
        }

        /// <summary>
        /// EventSystem の選択状態を解除する
        /// </summary>
        private void OnUnSelectButton()
        {
            // 選択解除
            _eventSystem.SetSelectedGameObject(null);
        }

        // ======================================================
        // アニメーションイベント
        // ======================================================

        /// <summary>
        /// 投影切り替え開始イベント
        /// </summary>
        public void OnSwitchProjectionStart()
        {
            _onSwitchProjection.OnNext(true);
        }

        /// <summary>
        /// 投影切り替え終了イベント
        /// </summary>
        public void OnSwitchProjectionEnd()
        {
            _onSwitchProjection.OnNext(false);
        }
    }
}