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
using OptionSystem.Presentation;
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

        /// <summary>最後に選択したボタンイベント</summary>
        private BaseButtonEvent _lastSelectedButtonEvent;

        /// <summary>最後にホバー選択中のボタンイベント</summary>
        private BaseButtonEvent _lastHoveredButtonEvent;

        // --------------------------------------------------
        // オプション選択制御
        // --------------------------------------------------
        /// <summary>GridLayoutGroup 内の Button 収集クラス</summary>
        private readonly GridLayoutGroupButtonCollector _gridLayoutGroupButtonCollector =
            new GridLayoutGroupButtonCollector();

        // --------------------------------------------------
        // システム参照
        // --------------------------------------------------
        /// <summary>EventSystem キャッシュ</summary>
        private EventSystem _eventSystem;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

        /// <summary>ゲームパッド入力状態フラグ</summary>
        private bool _isGamePadInput = false;

        /// <summary>現在アクティブなキャンバス</summary>
        private ActiveCanvasType _activeCanvasType = ActiveCanvasType.None;

        /// <summary>ポインターアニメーター</summary>
        private Animator _pointerAnimator;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// オプション UI バインダー辞書
        /// </summary>
        private readonly Dictionary<OptionType, OptionButtonBinder> _optionBinders
            = new Dictionary<OptionType, OptionButtonBinder>();

        /// <summary>
        /// オプション選択インデックステーブル
        /// </summary>
        private OptionSelectionIndexTable _optionIndexTable
            = new OptionSelectionIndexTable();

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
            _eventSystem = EventSystem.current;
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;

            if (_eventSystem == null ||
                _gameOptionManager == null ||
                _inputManager == null ||
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

            // オプション選択インデックス初期化
            _optionIndexTable.Initialize(OptionType.PlayerCount, _playerCountSelectedIndex);
            _optionIndexTable.Initialize(OptionType.LimitTime, _limitTimeSelectedIndex);
            _optionIndexTable.Initialize(OptionType.BoardSize, _boardSizeSelectedIndex);
            _optionIndexTable.Initialize(OptionType.ConnectCount, _connectCountSelectedIndex);
            _optionIndexTable.Initialize(OptionType.CameraRotationSpeed, _cameraRotationSpeedSelectedIndex);
            _optionIndexTable.Initialize(OptionType.PointerSpeed, _pointerSpeedSelectedIndex);

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
                    // 入力状態を更新
                    _isGamePadInput = isGamePadInput;

                    // --------------------------------------------------
                    // ゲームパッド入力時
                    // --------------------------------------------------
                    if (isGamePadInput)
                    {
                        // 最後に選択したボタンを再選択
                        RefreshSelectionState(_lastSelectedButtonEvent);

                        return;
                    }

                    // --------------------------------------------------
                    // 仮想パッド入力時
                    // --------------------------------------------------
                    // ホバー中ボタンが存在する場合
                    if (_lastHoveredButtonEvent != null)
                    {
                        // ホバー対象を選択
                        OnSelectButton(_lastHoveredButtonEvent);

                        return;
                    }

                    // 選択解除
                    OnUnSelectButton();
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
                    _lastHoveredButtonEvent = buttonEvent;
                    
                    OnSelectButton(buttonEvent);
                })
                .AddTo(_routerSubscriptions);

            // 選択解除通知
            // ホバー解除時に実行される
            _eventRouter.OnUnSelect
                .Subscribe(buttonEvent =>
                {
                    _lastHoveredButtonEvent = null;

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
            // バインダー生成
            CreateBinder(OptionType.PlayerCount, _playerCountButtons);
            CreateBinder(OptionType.LimitTime, _limitTimeButtons);
            CreateBinder(OptionType.BoardSize, _boardSizeButtons);
            CreateBinder(OptionType.ConnectCount, _connectCountButtons);
            CreateBinder(OptionType.CameraRotationSpeed, _cameraRotationSpeedButtons);
            CreateBinder(OptionType.PointerSpeed, _pointerSpeedButtons);

            // イベント購読
            foreach (OptionButtonBinder binder in _optionBinders.Values)
            {
                _eventRouter.RegisterOptionButtons(binder.Events);
            }
        }

        /// <summary>
        /// バインダー生成
        /// </summary>
        private void CreateBinder(in OptionType type, in GridLayoutGroup group)
        {
            // ボタン取得
            Button[] buttons = _gridLayoutGroupButtonCollector.GetButtons(group);

            // イベント取得
            OptionButtonEvent[] events = _gridLayoutGroupButtonCollector.GetOptionButtons(group);

            // 選択制御クラス生成
            ButtonSelectionController controller = new ButtonSelectionController(buttons);

            // 初期インデックス取得
            int initialIndex = _optionIndexTable.Get(type);

            // バインダー生成
            OptionButtonBinder binder = new OptionButtonBinder(
                type,
                buttons,
                events,
                controller,
                initialIndex
            );

            // 登録
            _optionBinders[type] = binder;
        }

        /// <summary>
        /// 入力状態に応じて選択状態を更新する
        /// </summary>
        /// <param name="buttonEvent">再選択対象</param>
        private void RefreshSelectionState(BaseButtonEvent buttonEvent = null)
        {
            // ゲームパッド入力ではない場合
            if (!_isGamePadInput)
            {
                // EventSystem の選択解除
                OnUnSelectButton();

                return;
            }

            // 再選択対象が存在しない場合
            if (buttonEvent == null)
            {
                // 初期選択を適用
                SelectInitialButtonByCanvas();

                return;
            }

            // 選択状態を更新
            OnSelectButton(buttonEvent);
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

                // 最後に選択したボタンが OptionButton の場合
                if (_lastSelectedButtonEvent is OptionButtonEvent optionButton)
                {
                    // 入力状態に応じて選択状態を更新
                    RefreshSelectionState(optionButton);
                }
                else
                {
                    // 入力状態に応じて初期選択を適用
                    RefreshSelectionState();
                }

                // --------------------------------------------------
                // オプション UI 状態リセット
                // --------------------------------------------------
                foreach (OptionButtonBinder binder in _optionBinders.Values)
                {
                    // バインダー種別に対応する初期インデックスを辞書から取得
                    int index = _optionIndexTable.Get(binder.Type);

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

            // オプションキャンセル押下時
            if (buttonEvent == _optionCancelButtonEvent)
            {
                // スタートキャンバス表示
                ShowStartCanvas();

                // 入力状態に応じて選択状態を更新
                RefreshSelectionState(_optionButtonEvent);

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, -1);

                return;
            }

            // オプション決定押下時
            if (buttonEvent == _optionDecideButtonEvent)
            {
                // スタートキャンバス表示
                ShowStartCanvas();

                // オプション選択インデックスを更新
                foreach (KeyValuePair<OptionType, OptionButtonBinder> binder in _optionBinders)
                {
                    // 現在の選択インデックスを取得
                    int currentIndex = binder.Value.GetCurrentSelectedIndex();

                    // テーブルへ反映
                    _optionIndexTable.Set(binder.Key, currentIndex);
                }

                // 入力状態に応じて選択状態を更新
                RefreshSelectionState(_startButtonEvent);

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
                // 3 x 3 ならボタンオブジェクト非表示、それ以外なら表示
                connectCountBinder.Buttons[i].gameObject.SetActive(!isThreeSize);
            }
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
            _lastSelectedButtonEvent = buttonEvent;

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