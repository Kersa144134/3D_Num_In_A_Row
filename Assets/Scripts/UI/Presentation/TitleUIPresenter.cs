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
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UniRx;
using InputSystem.Presentation;
using OptionSystem.Domain;
using PhaseSystem.Domain;
using UISystem.Application;
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
        // 列挙型
        // ======================================================

        /// <summary>
        /// 現在アクティブな UI キャンバス種別
        /// </summary>
        private enum ActiveCanvasType
        {
            None,
            Start,
            Option
        }
        
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
        // ポインター
        // --------------------------------------------------
        [Header("ポインター")]
        /// <summary>ポインターを表示する Image</summary>
        [SerializeField]
        private GameObject _pointer;

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        [Header("基本ボタン")]
        /// <summary>スタートボタン</summary>
        [SerializeField]
        private Button _startButton;

        /// <summary>オプションボタン</summary>
        [SerializeField]
        private Button _optionButton;

        /// <summary>オプションのキャンセルボタン</summary>
        [SerializeField]
        private Button _optionCancelButton;

        /// <summary>オプションの決定ボタン</summary>
        [SerializeField]
        private Button _optionDecideButton;

        [Header("オプションボタングループ")]
        /// <summary>プレイヤー人数</summary>
        [SerializeField]
        private GridLayoutGroup _playerCountButtons;

        /// <summary>制限時間</summary>
        [SerializeField]
        private GridLayoutGroup _limitTimeButtons;

        /// <summary>盤面サイズ</summary>
        [SerializeField]
        private GridLayoutGroup _boardSizeButtons;

        /// <summary>ライン成立条件</summary>
        [SerializeField]
        private GridLayoutGroup _connectCountButtons;

        /// <summary>カメラ回転速度</summary>
        [SerializeField]
        private GridLayoutGroup _cameraRotationSpeedButtons;

        /// <summary>ポインター速度</summary>
        [SerializeField]
        private GridLayoutGroup _pointerSpeedButtons;

        // --------------------------------------------------
        // ボタンカラー
        // --------------------------------------------------
        [Header("ボタンカラー")]
        /// <summary>選択時カラー</summary>
        [SerializeField]
        private Color _selectOnColor = Color.white;

        /// <summary>非選択時カラー</summary>
        [SerializeField]
        private Color _selectOffColor = Color.gray;

        /// <summary>フォーカス時カラー</summary>
        [SerializeField]
        private Color _focusOnColor = Color.white;

        /// <summary>非フォーカス時カラー</summary>
        [SerializeField]
        private Color _focusOffColor = Color.gray;

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
        // オプション選択インデックス
        // --------------------------------------------------
        [Header("オプション選択インデックス")]
        /// <summary>プレイヤー人数</summary>
        [SerializeField]
        private int _playerCountSelectedIndex = 0;

        /// <summary>制限時間</summary>
        [SerializeField]
        private int _limitTimeSelectedIndex = 1;

        /// <summary>盤面サイズ</summary>
        [SerializeField]
        private int _boardSizeSelectedIndex = 0;

        /// <summary>ライン成立条件</summary>
        [SerializeField]
        private int _connectCountSelectedIndex = 0;

        /// <summary>カメラ回転速度</summary>
        [SerializeField]
        private int _cameraRotationSpeedSelectedIndex = 1;

        /// <summary>ポインター速度</summary>
        [SerializeField]
        private int _pointerSpeedSelectedIndex = 1;

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
        private TitleUIEventRouter _eventRouter;

        // --------------------------------------------------
        // ボタンイベント
        // --------------------------------------------------
        /// <summary>スタートボタンイベント</summary>
        private NormalButtonEvent _startButtonEvent;

        /// <summary>オプションボタンイベント</summary>
        private NormalButtonEvent _optionButtonEvent;

        /// <summary>オプションキャンセルボタンイベント</summary>
        private NormalButtonEvent _optionCancelButtonEvent;

        /// <summary>オプション決定ボタンイベント</summary>
        private NormalButtonEvent _optionDecideButtonEvent;

        /// <summary>プレイヤー人数ボタンイベント配列</summary>
        private OptionButtonEvent[] _playerCountButtonEvents;

        /// <summary>制限時間ボタンイベント配列</summary>
        private OptionButtonEvent[] _limitTimeButtonEvents;

        /// <summary>盤面サイズボタンイベント配列</summary>
        private OptionButtonEvent[] _boardSizeButtonEvents;

        /// <summary>ライン成立条件ボタンイベント配列</summary>
        private OptionButtonEvent[] _connectCountButtonEvents;

        /// <summary>カメラ回転速度ボタンイベント配列</summary>
        private OptionButtonEvent[] _cameraRotationSpeedButtonEvents;

        /// <summary>ポインター速度ボタンイベント配列</summary>
        private OptionButtonEvent[] _pointerSpeedButtonEvents;

        /// <summary>現在選択中のボタンイベント</summary>
        private BaseButtonEvent _currentSelectedButtonEvent;

        /// <summary>現在ホバー選択中のボタンイベント</summary>
        private BaseButtonEvent _currentHoveredButtonEvent;

        // --------------------------------------------------
        // オプション選択制御
        // --------------------------------------------------
        /// <summary>GridLayoutGroup 内の Button 収集クラス</summary>
        private readonly GridLayoutGroupButtonCollector _gridLayoutGroupButtonCollector =
            new GridLayoutGroupButtonCollector();

        /// <summary>プレイヤー人数ボタン選択状態制御クラス</summary>
        private ButtonSelectionController _playerCountSelectionController;

        /// <summary>制限時間ボタン選択状態制御クラス</summary>
        private ButtonSelectionController _limitTimeSelectionController;

        /// <summary>盤面サイズボタン選択状態制御クラス</summary>
        private ButtonSelectionController _boardSizeSelectionController;

        /// <summary>ライン成立条件ボタン選択状態制御クラス</summary>
        private ButtonSelectionController _connectCountSelectionController;

        /// <summary>カメラ回転速度ボタン選択状態制御クラス</summary>
        private ButtonSelectionController _cameraRotationSpeedSelectionController;

        /// <summary>ポインター速度ボタン選択状態制御クラス</summary>
        private ButtonSelectionController _pointerSpeedSelectionController;

        // --------------------------------------------------
        // システム参照
        // --------------------------------------------------
        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        /// <summary>EventSystem キャッシュ</summary>
        private EventSystem _eventSystem;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

        /// <summary>ゲームパッド入力状態フラグ</summary>
        private bool _isGamePadInput = false;

        /// <summary>現在アクティブなキャンバス</summary>
        private ActiveCanvasType _activeCanvasType = ActiveCanvasType.None;

        /// <summary>プレイヤー人数ボタン配列</summary>
        private Button[] _playerCountButtonArray;

        /// <summary>制限時間ボタン配列</summary>
        private Button[] _limitTimeButtonArray;

        /// <summary>盤面サイズボタン配列</summary>
        private Button[] _boardSizeButtonArray;

        /// <summary>ライン成立条件ボタン配列</summary>
        private Button[] _connectCountButtonArray;

        /// <summary>カメラ回転速度ボタン配列</summary>
        private Button[] _cameraRotationSpeedButtonArray;

        /// <summary>ポインター速度ボタン配列</summary>
        private Button[] _pointerSpeedButtonArray;

        /// <summary>ポインターアニメーター</summary>
        private Animator _pointerAnimator;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// OptionButtonEvent と選択制御クラスの対応辞書
        /// </summary>
        private readonly Dictionary<OptionButtonEvent, ButtonSelectionController>
            _optionSelectionControllerMap = new Dictionary<OptionButtonEvent, ButtonSelectionController>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>BoardSize パラメータ名</summary>
        private static readonly int BOARD_SIZE_HASH = Animator.StringToHash("BoardSize");

        /// <summary>IsTarget パラメータ名</summary>
        private static readonly int IS_TARGET_HASH = Animator.StringToHash("IsTarget");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベントルーター購読管理</summary>
        private CompositeDisposable _routerSubscriptions;

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

        /// <summary>入力ロック状態購読</summary>
        private IDisposable _inputLockSubscription;

        /// <summary>ゲームパッド入力状態購読</summary>
        private IDisposable _gamePadInputSubscription;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // インスタンスからコンポーネント取得
            _inputManager = InputManager.Instance;
            _eventSystem = EventSystem.current;

            if (_inputManager == null ||
                _eventSystem == null ||
                _pointer == null ||
                _startButton == null ||
                _optionButton == null ||
                _optionCancelButton == null ||
                _optionDecideButton == null ||
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

            // ビュー生成
            _titleUIView = new TitleUIView(
                _pointer,
                _selectOnColor,
                _selectOffColor,
                _focusOnColor,
                _focusOffColor
            );

            // アニメーター取得
            _pointerAnimator = _pointer.GetComponent<Animator>();

            // スタートキャンバスを表示
            ShowStartCanvas();

            // ルーターイベント購読
            BindRouterEvents();

            // 通常ボタン初期化
            InitializeNormalButtons();

            // オプションボタン初期化
            InitializeOptionButtons();
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

        protected override void OnPhaseEnterInternal(in PhaseType phase) { }

        protected override void OnPhaseExitInternal(in PhaseType phase) { }

        protected override void OnExitInternal()
        {
            base.OnExitInternal();

            // イベント購読解除
            UnbindInputLockStream();
            UnbindGamePadInputStream();
            UnbindButtonEvents();
            UnbindRouterEvents();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// 入力ロック状態を購読する
        /// </summary>
        /// <param name="input">true: ロック / false: 解除</param>
        public void BindInputLockStream(in IObservable<bool> input)
        {
            // 多重購読防止
            _inputLockSubscription?.Dispose();

            _inputLockSubscription = input
                .Subscribe(isLock =>
                {
                    // 入力ロック状態を更新
                    _isInputLock = isLock;
                });
        }

        /// <summary>
        /// 入力ロック状態ストリームの購読を解除する
        /// </summary>
        public void UnbindInputLockStream()
        {
            _inputLockSubscription?.Dispose();
            _inputLockSubscription = null;
        }

        /// <summary>
        /// ゲームパッド入力状態を購読する
        /// </summary>
        /// <param name="input">
        /// true: ゲームパッド入力 / false: マウス入力
        /// </param>
        public void BindGamePadInputStream(in IObservable<bool> input)
        {
            // 多重購読防止
            _gamePadInputSubscription?.Dispose();

            _gamePadInputSubscription = input
                .DistinctUntilChanged()
                .Subscribe(isGamePadInput =>
                {
                    _isGamePadInput = isGamePadInput;

                    // --------------------------------------------------
                    // ゲームパッド入力時
                    // --------------------------------------------------
                    if (isGamePadInput)
                    {
                        // 最後に選択したボタンが存在しない場合
                        if (_currentSelectedButtonEvent == null)
                        {
                            // ナビゲーション不能になるため初期選択を設定
                            SelectInitialButtonByCanvas();

                            return;
                        }

                        // 最後に選択したボタンを再選択
                        OnSelectButton(_currentSelectedButtonEvent);

                        return;
                    }

                    // --------------------------------------------------
                    // 仮想パッド入力時
                    // --------------------------------------------------
                    // ホバー中ボタンが存在する場合
                    if (_currentHoveredButtonEvent != null)
                    {
                        // ホバー中ボタンを選択状態にする
                        OnSelectButton(_currentHoveredButtonEvent);

                        return;
                    }

                    // 最後に選択したボタンが存在する場合
                    if (_currentSelectedButtonEvent != null)
                    {
                        // 選択解除
                        OnUnSelectButton();
                    }
                });
        }

        /// <summary>
        /// ゲームパッド入力状態ストリームの購読を解除する
        /// </summary>
        public void UnbindGamePadInputStream()
        {
            // 購読解除
            _gamePadInputSubscription?.Dispose();

            // 参照解放
            _gamePadInputSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// ルーターのイベントを購読する
        /// </summary>
        private void BindRouterEvents()
        {
            // 多重購読防止
            UnbindRouterEvents();

            // ルーター初期化
            _eventRouter = new TitleUIEventRouter();
            _routerSubscriptions = new CompositeDisposable();

            // 通常ボタンクリック
            _eventRouter.OnNormalButtonClick
                .Subscribe(buttonEvent =>
                {
                    OnNormalButtonClick(buttonEvent);
                })
                .AddTo(_routerSubscriptions);

            // オプションボタンクリック
            _eventRouter.OnOptionButtonClick
                .Subscribe(buttonEvent =>
                {
                    OnOptionButtonClick(buttonEvent);
                })
                .AddTo(_routerSubscriptions);

            // フォーカス通知
            _eventRouter.OnFocus
                .Subscribe(buttonEvent =>
                {
                    OnFocusButton(buttonEvent);
                })
                .AddTo(_routerSubscriptions);

            // フォーカス解除通知
            _eventRouter.OnUnFocus
                .Subscribe(buttonEvent =>
                {
                    OnUnFocusButton(buttonEvent);
                })
                .AddTo(_routerSubscriptions);

            // 選択通知
            // ホバー選択時に実行される
            _eventRouter.OnSelect
                .Subscribe(buttonEvent =>
                {
                    _currentHoveredButtonEvent = buttonEvent;
                    
                    OnSelectButton(buttonEvent);
                })
                .AddTo(_routerSubscriptions);

            // 選択解除通知
            // ホバー解除時に実行される
            _eventRouter.OnUnSelect
                .Subscribe(buttonEvent =>
                {
                    _currentHoveredButtonEvent = null;

                    OnUnSelectButton();
                })
                .AddTo(_routerSubscriptions);
        }

        /// <summary>
        /// ルーターイベント購読を解除する
        /// </summary>
        private void UnbindRouterEvents()
        {
            _eventRouter?.Dispose();
            _eventRouter = null;

            _routerSubscriptions?.Dispose();
            _routerSubscriptions = null;
        }

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        /// <summary>
        /// スタートキャンバスを表示する
        /// </summary>
        private void ShowStartCanvas()
        {
            _startCanvas.SetActive(true);
            _optionCanvas.SetActive(false);

            _activeCanvasType = ActiveCanvasType.Start;
        }

        /// <summary>
        /// オプションキャンバスを表示する
        /// </summary>
        private void ShowOptionCanvas()
        {
            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(true);

            _activeCanvasType = ActiveCanvasType.Option;
        }

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// 通常ボタンを初期化する
        /// </summary>
        private void InitializeNormalButtons()
        {
            // NormalButtonEvent 取得
            _startButtonEvent = _startButton.GetComponent<NormalButtonEvent>();
            _optionButtonEvent = _optionButton.GetComponent<NormalButtonEvent>();
            _optionCancelButtonEvent = _optionCancelButton.GetComponent<NormalButtonEvent>();
            _optionDecideButtonEvent = _optionDecideButton.GetComponent<NormalButtonEvent>();

            // イベント購読
            _eventRouter.RegisterNormalButton(_startButtonEvent);
            _eventRouter.RegisterNormalButton(_optionButtonEvent);
            _eventRouter.RegisterNormalButton(_optionCancelButtonEvent);
            _eventRouter.RegisterNormalButton(_optionDecideButtonEvent);
        }

        /// <summary>
        /// Grid Layout Group 配下のオプションボタンを初期化する
        /// </summary>
        private void InitializeOptionButtons()
        {
            // Button 配列取得
            _playerCountButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_playerCountButtons);
            _limitTimeButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_limitTimeButtons);
            _boardSizeButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_boardSizeButtons);
            _connectCountButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_connectCountButtons);
            _cameraRotationSpeedButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_cameraRotationSpeedButtons);
            _pointerSpeedButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_pointerSpeedButtons);

            // OptionButtonEvent 配列取得
            _playerCountButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_playerCountButtons);
            _limitTimeButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_limitTimeButtons);
            _boardSizeButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_boardSizeButtons);
            _connectCountButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_connectCountButtons);
            _cameraRotationSpeedButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_cameraRotationSpeedButtons);
            _pointerSpeedButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_pointerSpeedButtons);

            // SelectionController 生成
            _playerCountSelectionController = new ButtonSelectionController(_playerCountButtonArray);
            _limitTimeSelectionController = new ButtonSelectionController(_limitTimeButtonArray);
            _boardSizeSelectionController = new ButtonSelectionController(_boardSizeButtonArray);
            _connectCountSelectionController = new ButtonSelectionController(_connectCountButtonArray);
            _cameraRotationSpeedSelectionController = new ButtonSelectionController(_cameraRotationSpeedButtonArray);
            _pointerSpeedSelectionController = new ButtonSelectionController(_pointerSpeedButtonArray);

            // 辞書構築
            RegisterSelectionControllerMap(_playerCountButtonEvents, _playerCountSelectionController);
            RegisterSelectionControllerMap(_limitTimeButtonEvents, _limitTimeSelectionController);
            RegisterSelectionControllerMap(_boardSizeButtonEvents, _boardSizeSelectionController);
            RegisterSelectionControllerMap(_connectCountButtonEvents, _connectCountSelectionController);
            RegisterSelectionControllerMap(_cameraRotationSpeedButtonEvents, _cameraRotationSpeedSelectionController);
            RegisterSelectionControllerMap(_pointerSpeedButtonEvents, _pointerSpeedSelectionController);

            // 選択状態を設定
            ApplySelectedIndexToControllers();

            // 選択状態ビュー更新
            ApplyAllButtonSelectionState();

            // イベント購読
            _eventRouter.RegisterOptionButtons(_playerCountButtonEvents);
            _eventRouter.RegisterOptionButtons(_limitTimeButtonEvents);
            _eventRouter.RegisterOptionButtons(_boardSizeButtonEvents);
            _eventRouter.RegisterOptionButtons(_connectCountButtonEvents);
            _eventRouter.RegisterOptionButtons(_cameraRotationSpeedButtonEvents);
            _eventRouter.RegisterOptionButtons(_pointerSpeedButtonEvents);
        }

        /// <summary>
        /// OptionButtonEvent と選択制御クラスの対応を登録する
        /// </summary>
        /// <param name="buttonEvents">対象イベント配列</param>
        /// <param name="controller">選択制御クラス</param>
        private void RegisterSelectionControllerMap(
            in OptionButtonEvent[] buttonEvents,
            in ButtonSelectionController controller)
        {
            if (buttonEvents == null || controller == null)
            {
                return;
            }

            foreach (OptionButtonEvent buttonEvent in buttonEvents)
            {
                if (buttonEvent == null)
                {
                    continue;
                }

                // 対応辞書へ登録
                _optionSelectionControllerMap[buttonEvent] = controller;
            }
        }

        /// <summary>
        /// オプション選択インデックスを選択制御クラスへ反映する
        /// </summary>
        private void ApplySelectedIndexToControllers()
        {
            _playerCountSelectionController.SelectByIndex(_playerCountSelectedIndex);
            _limitTimeSelectionController.SelectByIndex(_limitTimeSelectedIndex);
            _boardSizeSelectionController.SelectByIndex(_boardSizeSelectedIndex);
            _connectCountSelectionController.SelectByIndex(_connectCountSelectedIndex);
            _cameraRotationSpeedSelectionController.SelectByIndex(_cameraRotationSpeedSelectedIndex);
            _pointerSpeedSelectionController.SelectByIndex(_pointerSpeedSelectedIndex);
        }

        /// <summary>
        /// 全ボタン選択状態をビューへ反映する
        /// </summary>
        private void ApplyAllButtonSelectionState()
        {
            _titleUIView.ApplyButtonSelectionState(
                _playerCountSelectionController.ButtonArray,
                _playerCountSelectionController.SelectStateArray);
            _titleUIView.ApplyButtonSelectionState(
                _limitTimeSelectionController.ButtonArray,
                _limitTimeSelectionController.SelectStateArray);
            _titleUIView.ApplyButtonSelectionState(
                _boardSizeSelectionController.ButtonArray,
                _boardSizeSelectionController.SelectStateArray);
            _titleUIView.ApplyButtonSelectionState(
                _connectCountSelectionController.ButtonArray,
                _connectCountSelectionController.SelectStateArray);
            _titleUIView.ApplyButtonSelectionState(
                _cameraRotationSpeedSelectionController.ButtonArray,
                _cameraRotationSpeedSelectionController.SelectStateArray);
            _titleUIView.ApplyButtonSelectionState(
                _pointerSpeedSelectionController.ButtonArray,
                _pointerSpeedSelectionController.SelectStateArray);
        }

        /// <summary>
        /// 現在アクティブなキャンバスに応じて初期選択ボタンを選択する
        /// </summary>
        private void SelectInitialButtonByCanvas()
        {
            // 現在アクティブなキャンバス種別に応じて分岐
            switch (_activeCanvasType)
            {
                // スタートキャンバス時
                case ActiveCanvasType.Start:
                    // スタートキャンバス初期選択ボタンを選択
                    OnSelectButton(_initialSelectedStartCanvasButton);

                    break;

                // オプションキャンバス時
                case ActiveCanvasType.Option:
                    // オプションキャンバス初期選択ボタンを選択
                    OnSelectButton(_initialSelectedOptionCanvasButton);

                    break;
            }
        }

        /// <summary>
        /// ButtonEvent の入力イベント購読を解除する
        /// </summary>
        private void UnbindButtonEvents()
        {
            // NormalButtonEvent
            _startButtonEvent?.Dispose();
            _optionButtonEvent?.Dispose();
            _optionCancelButtonEvent?.Dispose();
            _optionDecideButtonEvent?.Dispose();

            // OptionButtonEvent
            DisposeOptionButtonEvents(_playerCountButtonEvents);
            DisposeOptionButtonEvents(_limitTimeButtonEvents);
            DisposeOptionButtonEvents(_boardSizeButtonEvents);
            DisposeOptionButtonEvents(_connectCountButtonEvents);
            DisposeOptionButtonEvents(_cameraRotationSpeedButtonEvents);
            DisposeOptionButtonEvents(_pointerSpeedButtonEvents);
        }

        /// <summary>
        /// OptionButtonEvent 配列の入力イベント購読を解除する
        /// </summary>
        /// <param name="buttonEvents">対象 OptionButton イベント配列</param>
        private void DisposeOptionButtonEvents(
            in OptionButtonEvent[] buttonEvents)
        {
            if (buttonEvents == null)
            {
                return;
            }

            foreach (OptionButtonEvent buttonEvent in buttonEvents)
            {
                if (buttonEvent == null)
                {
                    continue;
                }

                // イベント購読解除
                buttonEvent.Dispose();
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
            // オプションボタン押下時
            if (buttonEvent == _optionButtonEvent)
            {
                // オプションキャンバス表示
                ShowOptionCanvas();

                // ゲームパッド入力の場合
                if (_isGamePadInput)
                {
                    // 最後に選択したボタンが OptionButton の場合
                    if (_currentSelectedButtonEvent is OptionButtonEvent optionButton)
                    {
                        // 選択状態を更新
                        OnSelectButton(optionButton);
                    }
                    else
                    {
                        // ナビゲーション不能になるため初期選択を設定
                        SelectInitialButtonByCanvas();
                    }
                }
                else
                {
                    OnUnSelectButton();
                }

                // ボタン選択状態リセット
                ApplySelectedIndexToControllers();

                // 選択状態ビュー更新
                ApplyAllButtonSelectionState();

                return;
            }

            // オプションキャンセル押下時
            if (buttonEvent == _optionCancelButtonEvent)
            {
                // スタートキャンバス表示
                ShowStartCanvas();

                // ゲームパッド入力の場合
                if (_isGamePadInput)
                {
                    // 選択状態を更新
                    OnSelectButton(_optionButtonEvent);
                }
                else
                {
                    OnUnSelectButton();
                }

                return;
            }

            // オプション決定押下時
            if (buttonEvent == _optionDecideButtonEvent)
            {
                // スタートキャンバス表示
                ShowStartCanvas();

                // オプション選択インデックスを更新
                _playerCountSelectedIndex = _playerCountSelectionController.GetCurrentSelectedIndex();
                _limitTimeSelectedIndex = _limitTimeSelectionController.GetCurrentSelectedIndex();
                _boardSizeSelectedIndex = _boardSizeSelectionController.GetCurrentSelectedIndex();
                _connectCountSelectedIndex = _connectCountSelectionController.GetCurrentSelectedIndex();
                _cameraRotationSpeedSelectedIndex = _cameraRotationSpeedSelectionController.GetCurrentSelectedIndex();
                _pointerSpeedSelectedIndex = _pointerSpeedSelectionController.GetCurrentSelectedIndex();

                // ゲームパッド入力の場合
                if (_isGamePadInput)
                {
                    // 選択状態を更新
                    OnSelectButton(_startButtonEvent);
                }
                else
                {
                    OnUnSelectButton();
                }
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

            // 対応 SelectionController 取得
            if (!_optionSelectionControllerMap.TryGetValue(
                buttonEvent, out ButtonSelectionController controller))
            {
                return;
            }

            if (controller == null)
            {
                return;
            }

            // 選択状態更新
            controller.Select(buttonEvent.Button);

            // 選択状態をビューへ反映
            _titleUIView.ApplyButtonSelectionState(
                controller.ButtonArray,
                controller.SelectStateArray);

            // オプション更新通知
            _onUpdateGameOption.OnNext(buttonEvent.Data);
        }

        /// <summary>
        /// ボタンへフォーカス状態を適用し、フォーカス座標を通知する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnFocusButton(BaseButtonEvent buttonEvent)
        {
            if (buttonEvent == null ||
                buttonEvent.Button == null ||
                buttonEvent.RectTransform == null)
            {
                return;
            }

            // フォーカス状態表示
            _titleUIView.SetFocus(
                buttonEvent.Button,
                true);

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
            if (buttonEvent == null || buttonEvent.Button == null)
            {
                return;
            }
            
            // フォーカス状態非表示
            _titleUIView.SetFocus(buttonEvent.Button, false);

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

            // 選択対象のボタンイベントをキャッシュ
            _currentSelectedButtonEvent = buttonEvent;

            // 選択状態を更新
            SetSelectedGameObject(buttonEvent.gameObject);
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
        /// EventSystem の選択状態を更新する
        /// </summary>
        /// <param name="target">選択対象</param>
        private void SetSelectedGameObject(in GameObject target)
        {
            if (target == null)
            {
                return;
            }

            // 現在選択中のオブジェクト取得
            GameObject currentSelectedObject = _eventSystem.currentSelectedGameObject;

            // 同一オブジェクトが選択されている場合
            if (currentSelectedObject == target)
            {
                return;
            }

            // 選択状態を更新
            _eventSystem.SetSelectedGameObject(target);
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