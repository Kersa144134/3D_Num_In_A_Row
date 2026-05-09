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
        // ポインター
        // --------------------------------------------------
        [Header("ポインター")]
        /// <summary>ポインターを表示する Image</summary>
        [SerializeField]
        private Image _pointerImage;

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
        // オプションキャンバス初期選択ボタン
        // --------------------------------------------------
        [Header("オプションキャンバス初期選択ボタン")]
        /// <summary>オプションキャンバス初期選択ボタン</summary>
        [SerializeField]
        private Button _initialOptionCanvasSelectedButton;

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
        /// <summary>ボードの GameObject ルート</summary>
        [SerializeField]
        private Animator _boardAnimator;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>ビュー</summary>
        private TitleUIView _titleUIView;

        /// <summary>スタートボタンイベント</summary>
        private NormalButtonEvent _startButtonEvent;

        /// <summary>オプションボタンイベント</summary>
        private NormalButtonEvent _optionButtonEvent;

        /// <summary>オプションキャンセルボタンイベント</summary>
        private NormalButtonEvent _optionCancelButtonEvent;

        /// <summary>オプション決定ボタンイベント</summary>
        private NormalButtonEvent _optionDecideButtonEvent;

        /// <summary>現在オプションキャンバスで選択中のボタンイベント</summary>
        private BaseButtonEvent _currentOptionCanvasSelectedButtonEvent;

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

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        /// <summary>EventSystem キャッシュ</summary>
        private EventSystem _eventSystem;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

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

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>BoardSize パラメータ名</summary>
        private static readonly int BOARD_SIZE_HASH = Animator.StringToHash("BoardSize");

        // ======================================================
        // UniRx 変数
        // ======================================================

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

        /// <summary>フェーズ購読</summary>
        private IDisposable _phaseSubscription;

        /// <summary>入力ロック状態購読</summary>
        private IDisposable _inputLockSubscription;

        /// <summary>ポインター表示状態購読</summary>
        private IDisposable _pointerVisibleSubscription;

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
                _startButton == null ||
                _optionButton == null ||
                _optionCancelButton == null ||
                _optionDecideButton == null ||
                _initialOptionCanvasSelectedButton == null)
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
                _pointerImage,
                _selectOnColor,
                _selectOffColor,
                _focusOnColor,
                _focusOffColor
            );

            // 初期選択ボタンイベント設定
            _currentOptionCanvasSelectedButtonEvent =
                _initialOptionCanvasSelectedButton.GetComponent<BaseButtonEvent>();

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
            UnbindPhaseStream();
            UnbindInputLockStream();
            UnbindButtonEvents();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// フェーズ変更ストリームを購読し、現在のフェーズに応じて入力の有効・無効を制御する
        /// </summary>
        /// <param name="phase">フェーズ種別を通知するストリーム</param>
        public void BindPhaseStream(in IObservable<PhaseType> phase)
        {
            // 多重購読防止
            _phaseSubscription?.Dispose();

            _phaseSubscription = phase
                .Subscribe(type =>
                {
                    // Title
                    bool isTitle = type == PhaseType.Title;

                    SetPointerVisible(isTitle);
                });
        }

        /// <summary>
        /// フェーズ変更ストリームの購読を解除する
        /// </summary>
        public void UnbindPhaseStream()
        {
            _phaseSubscription?.Dispose();
            _phaseSubscription = null;
        }

        /// <summary>
        /// 入力ロック状態を購読する
        /// </summary>
        /// <param name="input">true:ロック / false:解除</param>
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
        /// ポインター表示状態を購読する
        /// </summary>
        /// <param name="visible">true:表示 / false:非表示</param>
        public void BindPointerVisibleStream(in IObservable<bool> visible)
        {
            // 多重購読防止
            _pointerVisibleSubscription?.Dispose();

            _pointerVisibleSubscription = visible
                .Subscribe(isVisible =>
                {
                    // ポインター表示状態を更新
                    SetPointerVisible(isVisible);
                });
        }

        /// <summary>
        /// ポインター表示状態ストリームの購読を解除する
        /// </summary>
        public void UnbindPointerVisibleStream()
        {
            _pointerVisibleSubscription?.Dispose();
            _pointerVisibleSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合は true</param>
        private void SetPointerVisible(in bool isVisible)
        {
            _titleUIView.SetPointerVisible(isVisible);
        }

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// 通常ボタンを初期化する
        /// </summary>
        private void InitializeNormalButtons()
        {
            // --------------------------------------------------
            // NormalButtonEvent 取得
            // --------------------------------------------------
            _startButtonEvent = _startButton.GetComponent<NormalButtonEvent>();
            _optionButtonEvent = _optionButton.GetComponent<NormalButtonEvent>();
            _optionCancelButtonEvent = _optionCancelButton.GetComponent<NormalButtonEvent>();
            _optionDecideButtonEvent = _optionDecideButton.GetComponent<NormalButtonEvent>();

            // --------------------------------------------------
            // 選択状態設定
            // --------------------------------------------------
            // EventSystem の選択状態を初期化
            _eventSystem.SetSelectedGameObject(_startButton.gameObject);

            // フォーカスリセット
            _titleUIView.SetFocus(_startButton, true);

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            BindNormalButton(_startButtonEvent);
            BindNormalButton(_optionButtonEvent);
            BindNormalButton(_optionCancelButtonEvent);
            BindNormalButton(_optionDecideButtonEvent);
        }

        /// <summary>
        /// NormalButton の入力イベントを購読する
        /// </summary>
        /// <param name="normalButton">入力対象ボタン配列</param>
        private void BindNormalButton(NormalButtonEvent normalButton)
        {
            if (normalButton == null)
            {
                return;
            }

            // --------------------------------------------------
            // クリック時
            // --------------------------------------------------
            normalButton.OnNormalClickAsObservable
                .Subscribe(_ =>
                {
                    // オプションボタン押下時
                    if (normalButton == _optionButtonEvent)
                    {
                        // 現在保持しているオプション選択ボタンを復元
                        if (_currentOptionCanvasSelectedButtonEvent != null)
                        {
                            // EventSystem の選択状態を復元
                            _eventSystem.SetSelectedGameObject(
                                _currentOptionCanvasSelectedButtonEvent.gameObject);

                            // フォーカス状態を更新
                            NotifyFocus(_currentOptionCanvasSelectedButtonEvent);
                        }

                        // ボタン選択状態リセット
                        ApplySelectedIndexToControllers();

                        // 選択状態ビュー更新
                        ApplyAllButtonSelectionState();

                        return;
                    }

                    // オプションキャンセルボタン押下時
                    if (normalButton == _optionCancelButtonEvent)
                    {
                        // オプションボタンが存在する場合
                        if (_optionButtonEvent != null)
                        {
                            // オプションボタンを選択状態にする
                            _eventSystem.SetSelectedGameObject(
                                _optionButtonEvent.gameObject);

                            // フォーカス状態を更新
                            NotifyFocus(_optionButtonEvent);
                        }

                        return;
                    }

                    // オプション決定ボタン押下時
                    if (normalButton == _optionDecideButtonEvent)
                    {
                        // スタートボタンが存在する場合
                        if (_startButtonEvent != null)
                        {
                            // スタートボタンを選択状態にする
                            _eventSystem.SetSelectedGameObject(
                                _startButtonEvent.gameObject);

                            // フォーカス状態を更新
                            NotifyFocus(_startButtonEvent);
                        }

                        // オプション選択インデックスを更新
                        _playerCountSelectedIndex = _playerCountSelectionController.GetCurrentSelectedIndex();
                        _limitTimeSelectedIndex = _limitTimeSelectionController.GetCurrentSelectedIndex();
                        _boardSizeSelectedIndex = _boardSizeSelectionController.GetCurrentSelectedIndex();
                        _connectCountSelectedIndex = _connectCountSelectionController.GetCurrentSelectedIndex();
                        _cameraRotationSpeedSelectedIndex = _cameraRotationSpeedSelectionController.GetCurrentSelectedIndex();
                        _pointerSpeedSelectedIndex = _pointerSpeedSelectionController.GetCurrentSelectedIndex();

                        return;
                    }
                })
                .AddTo(normalButton);

            // --------------------------------------------------
            // ホバー開始時
            // --------------------------------------------------
            normalButton.OnHoverEnterAsObservable
                .Subscribe(_ =>
                {
                    // フォーカス状態を更新
                    NotifyFocus(normalButton);

                    // EventSystem の選択状態更新
                    SetSelectedButton(normalButton.Button);
                })
                .AddTo(normalButton);

            // --------------------------------------------------
            // ホバー終了時
            // --------------------------------------------------
            normalButton.OnHoverExitAsObservable
                .Subscribe(_ =>
                {
                    // フォーカス状態表示を無効化
                    _titleUIView.SetFocus(normalButton.Button, false);
                })
                .AddTo(normalButton);

            // --------------------------------------------------
            // 選択開始時
            // --------------------------------------------------
            normalButton.OnSelectEnterAsObservable
                .Subscribe(_ =>
                {
                    // フォーカス状態を更新
                    NotifyFocus(normalButton);
                })
                .AddTo(normalButton);

            // --------------------------------------------------
            // 選択終了時
            // --------------------------------------------------
            normalButton.OnSelectExitAsObservable
                .Subscribe(_ =>
                {
                    // フォーカス状態表示を無効化
                    _titleUIView.SetFocus(normalButton.Button, false);
                })
                .AddTo(normalButton);
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
        /// <param name="buttonEvents">対象の ButtonEvent 配列</param>
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

        /// <summary>
        /// Grid Layout Group 配下のオプションボタンを初期化する
        /// </summary>
        private void InitializeOptionButtons()
        {
            // --------------------------------------------------
            // Button 配列取得
            // --------------------------------------------------
            _playerCountButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_playerCountButtons);
            _limitTimeButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_limitTimeButtons);
            _boardSizeButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_boardSizeButtons);
            _connectCountButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_connectCountButtons);
            _cameraRotationSpeedButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_cameraRotationSpeedButtons);
            _pointerSpeedButtonArray = _gridLayoutGroupButtonCollector.GetButtons(_pointerSpeedButtons);

            // --------------------------------------------------
            // OptionButtonEvent 配列取得
            // --------------------------------------------------
            _playerCountButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_playerCountButtons);
            _limitTimeButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_limitTimeButtons);
            _boardSizeButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_boardSizeButtons);
            _connectCountButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_connectCountButtons);
            _cameraRotationSpeedButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_cameraRotationSpeedButtons);
            _pointerSpeedButtonEvents = _gridLayoutGroupButtonCollector.GetOptionButtons(_pointerSpeedButtons);

            // --------------------------------------------------
            // SelectionController 生成
            // --------------------------------------------------
            _playerCountSelectionController = new ButtonSelectionController(_playerCountButtonArray);
            _limitTimeSelectionController = new ButtonSelectionController(_limitTimeButtonArray);
            _boardSizeSelectionController = new ButtonSelectionController(_boardSizeButtonArray);
            _connectCountSelectionController = new ButtonSelectionController(_connectCountButtonArray);
            _cameraRotationSpeedSelectionController = new ButtonSelectionController(_cameraRotationSpeedButtonArray);
            _pointerSpeedSelectionController = new ButtonSelectionController(_pointerSpeedButtonArray);

            // --------------------------------------------------
            // 選択状態を設定
            // --------------------------------------------------
            ApplySelectedIndexToControllers();

            // --------------------------------------------------
            // 選択状態ビュー更新
            // --------------------------------------------------
            ApplyAllButtonSelectionState();

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            BindOptionButtons(_playerCountButtonEvents, _playerCountSelectionController);
            BindOptionButtons(_limitTimeButtonEvents, _limitTimeSelectionController);
            BindOptionButtons(_boardSizeButtonEvents, _boardSizeSelectionController);
            BindOptionButtons(_connectCountButtonEvents, _connectCountSelectionController);
            BindOptionButtons(_cameraRotationSpeedButtonEvents, _cameraRotationSpeedSelectionController);
            BindOptionButtons(_pointerSpeedButtonEvents, _pointerSpeedSelectionController);
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
        /// OptionButton の入力イベントを購読する
        /// </summary>
        /// <param name="optionButtons">入力対象ボタン配列</param>
        /// <param name="controller">選択状態制御</param>
        private void BindOptionButtons(
            OptionButtonEvent[] optionButtons,
            ButtonSelectionController controller)
        {
            if (optionButtons == null)
            {
                return;
            }

            for (int index = 0; index < optionButtons.Length; index++)
            {
                OptionButtonEvent optionButton = optionButtons[index];

                if (optionButton == null)
                {
                    continue;
                }

                // --------------------------------------------------
                // クリック時
                // --------------------------------------------------
                optionButton.OnOptionClickAsObservable
                    .Subscribe(data =>
                    {
                        controller.Select(optionButton.Button);

                        // ビュー反映
                        _titleUIView.ApplyButtonSelectionState(
                            controller.ButtonArray,
                            controller.SelectStateArray);

                        // オプション更新通知
                        _onUpdateGameOption.OnNext(data);
                    })
                    .AddTo(optionButton);

                // --------------------------------------------------
                // ホバー開始時
                // --------------------------------------------------
                optionButton.OnHoverEnterAsObservable
                    .Subscribe(_ =>
                    {
                        // フォーカス状態を更新
                        NotifyFocus(optionButton);

                        // 現在のオプションキャンバス選択ボタンを更新
                        UpdateCurrentOptionSelectedButton(
                            optionButton.Button,
                            optionButton);

                        // EventSystem の選択状態更新
                        SetSelectedButton(optionButton.Button);
                    })
                    .AddTo(optionButton);

                // --------------------------------------------------
                // ホバー終了時
                // --------------------------------------------------
                optionButton.OnHoverExitAsObservable
                    .Subscribe(_ =>
                    {
                        // フォーカス状態表示を無効化
                        _titleUIView.SetFocus(optionButton.Button, false);
                    })
                    .AddTo(optionButton);

                // --------------------------------------------------
                // 選択開始時
                // --------------------------------------------------
                optionButton.OnSelectEnterAsObservable
                    .Subscribe(_ =>
                    {
                        // フォーカス状態を更新
                        NotifyFocus(optionButton);

                        // 現在のオプションキャンバス選択ボタンを更新
                        UpdateCurrentOptionSelectedButton(
                            optionButton.Button,
                            optionButton);
                    })
                    .AddTo(optionButton);

                // --------------------------------------------------
                // 選択終了時
                // --------------------------------------------------
                optionButton.OnSelectExitAsObservable
                    .Subscribe(_ =>
                    {
                        // フォーカス状態表示を無効化
                        _titleUIView.SetFocus(optionButton.Button, false);
                    })
                    .AddTo(optionButton);
            }
        }

        // --------------------------------------------------
        // フォーカス
        // --------------------------------------------------
        /// <summary>
        /// ボタンへフォーカス状態を適用し、フォーカス座標を通知する
        /// </summary>
        /// <param name="buttonEvent">フォーカス対象ボタンイベント</param>
        private void NotifyFocus(in BaseButtonEvent buttonEvent)
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
            Vector2 screenPosition =
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    buttonEvent.RectTransform.position);

            // フォーカス通知
            _onFocusPosition.OnNext(screenPosition);
        }

        /// <summary>
        /// EventSystem の選択状態を変更する
        /// </summary>
        /// <param name="button">選択対象ボタン</param>
        private void SetSelectedButton(in Button button)
        {
            if (_eventSystem == null)
            {
                return;
            }

            if (button == null)
            {
                return;
            }

            // 選択状態を更新
            _eventSystem.SetSelectedGameObject(button.gameObject);
        }

        /// <summary>
        /// 現在オプションキャンバスで選択中のボタン情報を更新する
        /// </summary>
        /// <param name="button">選択対象ボタン</param>
        /// <param name="buttonEvent">選択対象ボタンイベント</param>
        private void UpdateCurrentOptionSelectedButton(
            in Button button,
            in BaseButtonEvent buttonEvent)
        {
            if (button == null)
            {
                return;
            }

            if (buttonEvent == null)
            {
                return;
            }

            // 現在のキャンバス選択ボタンイベントを更新
            _currentOptionCanvasSelectedButtonEvent = buttonEvent;
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

        /// <summary>
        /// スタートキャンバスを表示する
        /// </summary>
        public void ShowStartCanvas()
        {
            _startCanvas.SetActive(true);
            _optionCanvas.SetActive(false);
        }

        /// <summary>
        /// オプションキャンバスを表示する
        /// </summary>
        public void ShowOptionCanvas()
        {
            _startCanvas.SetActive(false);
            _optionCanvas.SetActive(true);
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