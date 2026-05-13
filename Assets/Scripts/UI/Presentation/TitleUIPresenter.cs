// ======================================================
// TitleUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : タイトルシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using InputSystem.Presentation;
using OptionSystem.Domain;
using OptionSystem.Infrastructure;
using OptionSystem.Presentation;
using System;
using System.Collections.Generic;
using UISystem.Application;
using UISystem.Domain;
using UISystem.Infrastructure;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UpdateSystem.Domain;
using static UnityEngine.GraphicsBuffer;

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
        /// <summary>EventSystem キャッシュ</summary>
        private EventSystem _eventSystem;

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

        /// <summary>IsTarget パラメータ名</summary>
        private static readonly int IS_TARGET_HASH = Animator.StringToHash("IsTarget");

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
            _eventSystem = EventSystem.current;
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;

            if (_eventSystem == null ||
                _gameOptionManager == null ||
                _inputManager == null ||
                _pointer == null ||
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
            // キャンバス初期化
            // --------------------------------------------------
            // キャンバス状態管理クラス生成
            _titleUIStateController = new TitleUIStateController(
                _dialogCanvas,
                _startCanvas,
                _optionCanvas,
                _normalButtonResolver.Get(UIActionType.DialogYes),
                _initialSelectedStartCanvasButton,
                _initialSelectedOptionCanvasButton
            );

            // スタートキャンバスを表示
            _titleUIStateController.ShowStartCanvas();

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            Subscribe();
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

            // イベント購読解除
            Dispose();
            _disposables?.Dispose();
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
        private void Subscribe()
        {
            // 通常ボタンクリック
            _eventRouter.OnNormalButtonClick
                .Subscribe(buttonEvent => OnNormalButtonClick(buttonEvent))
                .AddTo(_routerDisposables);

            // オプションボタンクリック
            _eventRouter.OnOptionButtonClick
                .Subscribe(buttonEvent => OnOptionButtonClick(buttonEvent))
                .AddTo(_routerDisposables);

            // ホバー通知
            _eventRouter.OnHover
                .Subscribe(buttonEvent =>
                {
                    // 現在アクティブなキャンバス状態を取得
                    CanvasType activeCanvasType = _titleUIStateController.GetActiveCanvasType();

                    // 選択対象のボタンイベントをキャッシュ
                    _titleUIStateController.SetLastHoveredButtonEvent(activeCanvasType, buttonEvent);

                    OnSelectButton(buttonEvent);
                })
                .AddTo(_routerDisposables);

            // ホバー解除通知
            _eventRouter.OnUnHover
                .Subscribe(buttonEvent =>
                {
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
                .Subscribe(buttonEvent => OnFocusButton(buttonEvent))
                .AddTo(_routerDisposables);

            // フォーカス解除通知
            _eventRouter.OnUnFocus
                .Subscribe(buttonEvent => OnUnFocusButton(buttonEvent))
                .AddTo(_routerDisposables);
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void Dispose()
        {
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
            NormalButton[] normalButtons = new NormalButton[_baseNormalButtons.Length + _titleNormalButtons.Length];

            // ベースボタン配列コピー
            Array.Copy(_baseNormalButtons, 0,  normalButtons, 0, _baseNormalButtons.Length);

            // タイトルボタン配列コピー
            Array.Copy( _titleNormalButtons, 0, normalButtons, _baseNormalButtons.Length, _titleNormalButtons.Length);

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
        /// キャンバスと入力状態に応じて選択状態を更新する
        /// </summary>
        /// <param name="canvasType">対象キャンバス</param>
        /// <param name="cachedButtonEvent">対象ボタンイベント</param>
        private void SetSelectionState(
            in CanvasType canvasType,
            in BaseButtonEvent buttonEvent = null)
        {
            // 選択状態をリセット
            OnUnSelectButton();

            // --------------------------------------------------
            // ゲームパッド入力時
            // --------------------------------------------------
            if (_isGamePadInput)
            {
                // --------------------------------------------------
                // ダイアログキャンバス処理
                // --------------------------------------------------
                if (canvasType == CanvasType.Dialog)
                {
                    // ダイアログ Yes ボタンを取得
                    BaseButtonEvent dialogYesButton =
                        _titleUIStateController.GetInitialSelectedButton();

                    // ダイアログ Yes ボタンを適用
                    OnSelectButton(dialogYesButton);
                    return;
                }

                // --------------------------------------------------
                // 通常キャンバス処理
                // --------------------------------------------------
                // 対象ボタンイベントが存在しない場合は初期選択
                if (buttonEvent == null)
                {
                    // 初期選択ボタンを取得
                    BaseButtonEvent initialButtonEvent =
                        _titleUIStateController.GetInitialSelectedButton();

                    // 初期選択を適用
                    OnSelectButton(initialButtonEvent);

                    return;
                }

                // 対象ボタンを適用
                OnSelectButton(buttonEvent);
            }

            // --------------------------------------------------
            // マウス入力時
            // --------------------------------------------------
            // 現在キャンバスで最後にホバーしたボタンを取得
            BaseButtonEvent hoverButtonEvent =
                _titleUIStateController.GetLastHoveredButtonEvent(canvasType);

            // ホバー対象が存在しない場合は処理なし
            if (hoverButtonEvent == null)
            {
                return;
            }

            // ホバー中のボタンを適用
            OnSelectButton(hoverButtonEvent);
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
        /// NormalButton クリック時
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnNormalButtonClick(NormalButtonEvent buttonEvent)
        {
            // 現在アクティブなキャンバス状態を取得
            CanvasType activeCanvasType = _titleUIStateController.GetActiveCanvasType();

            // 最後に選択していたボタンを取得
            BaseButtonEvent selectedButtonEvent =
                _titleUIStateController.GetLastSelectedButtonEvent(activeCanvasType);

            // --------------------------------------------------
            // ダイアログ Yes ボタン
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeDialogYes)
            && typeDialogYes == UIActionType.DialogYes)
            {
                // ダイアログボタン非表示
                buttonEvent.gameObject.SetActive(false);
                _normalButtonResolver.Get(UIActionType.DialogNo).gameObject.SetActive(false);

                return;
            }

            // --------------------------------------------------
            // ダイアログ No ボタン
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeDialogNo)
            && typeDialogNo == UIActionType.DialogNo)
            {
                // ダイアログキャンバス非表示
                _titleUIStateController.HideDialogCanvas();

                // 最後に選択していたボタンを適用
                SetSelectionState(activeCanvasType, selectedButtonEvent);

                // ダイアログ非表示を通知
                _onDialogVisibleChanged.OnNext(false);

                return;
            }

            // --------------------------------------------------
            // スタートボタン
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeStart)
                && typeStart == UIActionType.TitleStart)
            {
                // ダイアログキャンバス表示
                _titleUIStateController.ShowDialogCanvas();

                // ダイアログボタン表示
                _normalButtonResolver.Get(UIActionType.DialogYes).gameObject.SetActive(true);
                _normalButtonResolver.Get(UIActionType.DialogNo).gameObject.SetActive(true);

                // ダイアログ YES ボタンを適用
                SetSelectionState(activeCanvasType, _normalButtonResolver.Get(UIActionType.DialogYes));

                // ダイアログ表示を通知
                _onDialogVisibleChanged.OnNext(true);

                return;
            }

            // --------------------------------------------------
            // オプションボタン
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeOption)
                && typeOption == UIActionType.TitleOption)
            {
                // オプションキャンバス表示
                _titleUIStateController.ShowOptionCanvas();

                // 最後に選択したボタンが OptionButton の場合
                if (selectedButtonEvent is OptionButtonEvent optionButton)
                {
                    // 最後に選択したオプションボタンを適用
                    SetSelectionState(activeCanvasType, optionButton);
                }
                else
                {
                    // 入力状態に応じて初期選択を適用
                    SetSelectionState(activeCanvasType);
                }

                // --------------------------------------------------
                // オプション UI 状態リセット
                // --------------------------------------------------
                foreach (OptionButtonBinder binder in _optionBinders.Values)
                {
                    // バインダー種別に対応する初期インデックスをリポジトリから取得
                    int index = _repository.Get(binder.Type);

                    // インデックスを適用して選択状態を更新
                    binder.SelectByIndex(index);

                    // ビューへ選択状態を反映
                    _titleUIView.ApplyButtonSelectionState(
                        binder.Buttons,
                        binder.SelectStateArray);
                }

                // 盤面サイズ取得
                int boardSize = _gameOptionManager.BoardSize;

                // ConnectCount 制御
                ApplyBoardSizeDependentConnectCount(boardSize);

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, boardSize);

                return;
            }

            // --------------------------------------------------
            // オプションキャンセルボタン
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeCancel)
                && typeCancel == UIActionType.TitleOptionCancel)
            {
                // スタートキャンバス表示
                _titleUIStateController.ShowStartCanvas();

                // オプションボタンを適用
                SetSelectionState(activeCanvasType, _normalButtonResolver.Get(UIActionType.TitleOption));

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, -1);

                return;
            }

            // --------------------------------------------------
            // オプション決定ボタン
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeDecide)
                && typeDecide == UIActionType.TitleOptionDecide)
            {
                // スタートキャンバス表示
                _titleUIStateController.ShowStartCanvas();

                // オプション選択インデックスを更新
                foreach (KeyValuePair<OptionType, OptionButtonBinder> binder in _optionBinders)
                {
                    // 現在の選択インデックスを取得
                    int currentIndex = binder.Value.GetCurrentSelectedIndex();

                    // リポジトリへ反映
                    _repository.Save(binder.Key, currentIndex);
                }

                // スタートボタンを適用
                SetSelectionState(activeCanvasType, _normalButtonResolver.Get(UIActionType.TitleStart));

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, -1);
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
        /// ボタンへフォーカス状態を適用し、フォーカス座標を通知する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnFocusButton(BaseButtonEvent buttonEvent)
        {
            if (buttonEvent == null ||
                buttonEvent.RectTransform == null)
            {
                return;
            }

            // 現在アクティブなキャンバス状態を取得
            CanvasType activeCanvasType =
                _titleUIStateController.GetActiveCanvasType();

            // 選択対象のボタンイベントをキャッシュ
            _titleUIStateController.SetLastSelectedButtonEvent(activeCanvasType, buttonEvent);

            // フォーカス状態表示
            SetFocusState(buttonEvent, true);

            // スクリーン座標変換
            Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(
                null,
                buttonEvent.RectTransform.position);

            // フォーカス通知
            _onFocusPosition.OnNext(screenPosition);

            // ターゲット検出状態を有効化
            _pointerAnimator?.SetBool(IS_TARGET_HASH, true);
        }

        /// <summary>
        /// ボタンのフォーカス状態を解除する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnUnFocusButton(BaseButtonEvent buttonEvent)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // フォーカス状態非表示
            SetFocusState(buttonEvent, false);

            // ターゲット検出状態を解除
            _pointerAnimator?.SetBool(IS_TARGET_HASH, false);
        }

        /// <summary>
        /// EventSystem の選択状態を変更する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnSelectButton(BaseButtonEvent buttonEvent)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // 現在選択中のオブジェクト取得
            GameObject currentSelectedObject = _eventSystem.currentSelectedGameObject;

            // 同一オブジェクトが選択されている場合
            if (currentSelectedObject == buttonEvent.GameObject)
            {
                return;
            }

            // 選択状態を更新
            _eventSystem.SetSelectedGameObject(buttonEvent.GameObject);
        }

        /// <summary>
        /// EventSystem の選択状態を解除する
        /// </summary>
        private void OnUnSelectButton()
        {
            // 選択解除
            _eventSystem.SetSelectedGameObject(null);
        }

        /// <summary>
        /// ダイアログ入力時の処理を行う
        /// </summary>
        /// <param name="isDecide">決定入力かどうか</param>
        protected override void HandleDialogInput(in bool isDecide)
        {
            if (!isDecide)
            {
                // ダイアログキャンバス非表示
                _titleUIStateController.HideDialogCanvas();

                // 現在アクティブなキャンバス状態を取得
                CanvasType activeCanvasType = _titleUIStateController.GetActiveCanvasType();

                // 入力状態に応じて初期選択を適用
                SetSelectionState(activeCanvasType);

                // ダイアログ非表示を通知
                _onDialogVisibleChanged.OnNext(false);

                return;
            }
        }

        // ======================================================
        // ボタンイベント
        // ======================================================

        /// <summary>
        /// シーン遷移リクエストを通知する
        /// </summary>
        public override void RequestSceneChange()
        {
            base.RequestSceneChange();
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