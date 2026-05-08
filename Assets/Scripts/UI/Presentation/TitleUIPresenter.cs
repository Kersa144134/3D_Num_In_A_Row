// ======================================================
// TitleUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : タイトルシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using InputSystem.Presentation;
using OptionSystem.Domain;
using OptionSystem.Presentation;
using PhaseSystem.Domain;
using System;
using System.Collections.Generic;
using UISystem.Infrastructure;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
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
        // GridLayoutGroup
        // --------------------------------------------------
        [Header("ボタングループ")]
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
        // ボタン選択インデックス
        // --------------------------------------------------
        [Header("初期選択インデックス")]
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

        /// <summary>GridLayoutGroup 内の Button を収集するクラス</summary>
        private readonly GridLayoutGroupButtonCollector _gridLayoutGroupButtonCollector =
            new GridLayoutGroupButtonCollector();

        /// <summary>プレイヤー人数ボタン選択制御</summary>
        private ButtonSelectionController _playerCountSelectionController;

        /// <summary>制限時間ボタン選択制御</summary>
        private ButtonSelectionController _limitTimeSelectionController;

        /// <summary>盤面サイズボタン選択制御</summary>
        private ButtonSelectionController _boardSizeSelectionController;

        /// <summary>ライン成立条件ボタン選択制御</summary>
        private ButtonSelectionController _connectCountSelectionController;

        /// <summary>カメラ回転速度ボタン選択制御</summary>
        private ButtonSelectionController _cameraRotationSpeedSelectionController;

        /// <summary>ポインター速度ボタン選択制御</summary>
        private ButtonSelectionController _pointerSpeedSelectionController;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        /// <summary>EventSystem キャッシュ</summary>
        private EventSystem _eventSystem;

        // ======================================================
        // フィールド
        // ======================================================

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

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// OptionButton に紐づく RectTransform を保持するキャッシュ辞書
        /// </summary>
        private readonly Dictionary<OptionButton, RectTransform>
            _buttonRectTransformMap = new Dictionary<OptionButton, RectTransform>();

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
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;
            _eventSystem = EventSystem.current;

            if (_inputManager == null)
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

            // ボタン初期化
            InitializeButtons();
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
            UnbindOptionButtons();
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

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// Grid Layout Group 配下の Button を初期化する
        /// </summary>
        private void InitializeButtons()
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
            // OptionButton 配列取得
            // --------------------------------------------------
            OptionButton[] playerCountButtons = _gridLayoutGroupButtonCollector.GetOptionButtons(_playerCountButtons);
            OptionButton[] limitTimeButtons = _gridLayoutGroupButtonCollector.GetOptionButtons(_limitTimeButtons);
            OptionButton[] boardSizeButtons = _gridLayoutGroupButtonCollector.GetOptionButtons(_boardSizeButtons);
            OptionButton[] connectCountButtons = _gridLayoutGroupButtonCollector.GetOptionButtons(_connectCountButtons);
            OptionButton[] cameraRotationSpeedButtons = _gridLayoutGroupButtonCollector.GetOptionButtons(_cameraRotationSpeedButtons);
            OptionButton[] pointerSpeedButtons = _gridLayoutGroupButtonCollector.GetOptionButtons(_pointerSpeedButtons);

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
            // 選択状態適用
            // --------------------------------------------------
            ApplySelectionIndexState();

            // --------------------------------------------------
            // ビュー更新
            // --------------------------------------------------
            ApplyAllButtonSelectionState();

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            BindOptionButtons(playerCountButtons, _playerCountSelectionController);
            BindOptionButtons(limitTimeButtons, _limitTimeSelectionController);
            BindOptionButtons(boardSizeButtons, _boardSizeSelectionController);
            BindOptionButtons(connectCountButtons, _connectCountSelectionController);
            BindOptionButtons(cameraRotationSpeedButtons, _cameraRotationSpeedSelectionController);
            BindOptionButtons(pointerSpeedButtons, _pointerSpeedSelectionController);
        }

        /// <summary>
        /// 選択インデックス状態を全 SelectionController へ反映する
        /// </summary>
        private void ApplySelectionIndexState()
        {
            // プレイヤー人数
            _playerCountSelectionController.SelectByIndex(
                _playerCountSelectedIndex);

            // 制限時間
            _limitTimeSelectionController.SelectByIndex(
                _limitTimeSelectedIndex);

            // 盤面サイズ
            _boardSizeSelectionController.SelectByIndex(
                _boardSizeSelectedIndex);

            // ライン成立条件
            _connectCountSelectionController.SelectByIndex(
                _connectCountSelectedIndex);

            // カメラ回転速度
            _cameraRotationSpeedSelectionController.SelectByIndex(
                _cameraRotationSpeedSelectedIndex);

            // ポインター速度
            _pointerSpeedSelectionController.SelectByIndex(
                _pointerSpeedSelectedIndex);
        }
        
        /// <summary>
        /// 全ボタン選択状態をビューへ反映する
        /// </summary>
        private void ApplyAllButtonSelectionState()
        {
            // プレイヤー人数
            _titleUIView.ApplyButtonSelectionState(
                _playerCountSelectionController.ButtonArray,
                _playerCountSelectionController.SelectStateArray);

            // 制限時間
            _titleUIView.ApplyButtonSelectionState(
                _limitTimeSelectionController.ButtonArray,
                _limitTimeSelectionController.SelectStateArray);

            // 盤面サイズ
            _titleUIView.ApplyButtonSelectionState(
                _boardSizeSelectionController.ButtonArray,
                _boardSizeSelectionController.SelectStateArray);

            // ライン成立条件
            _titleUIView.ApplyButtonSelectionState(
                _connectCountSelectionController.ButtonArray,
                _connectCountSelectionController.SelectStateArray);

            // カメラ回転速度
            _titleUIView.ApplyButtonSelectionState(
                _cameraRotationSpeedSelectionController.ButtonArray,
                _cameraRotationSpeedSelectionController.SelectStateArray);

            // ポインター速度
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
            OptionButton[] optionButtons,
            ButtonSelectionController controller)
        {
            if (optionButtons == null)
            {
                return;
            }

            for (int index = 0; index < optionButtons.Length; index++)
            {
                OptionButton optionButton = optionButtons[index];

                if (optionButton == null)
                {
                    continue;
                }

                // Button コンポーネント取得
                Button button = optionButton.GetComponent<Button>();

                if (button == null)
                {
                    continue;
                }

                // RectTransform 取得
                RectTransform rectTransform = button.transform as RectTransform;

                if (rectTransform == null)
                {
                    continue;
                }

                // 座標キャッシュ登録
                _buttonRectTransformMap[optionButton] = rectTransform;

                // --------------------------------------------------
                // クリック時
                // --------------------------------------------------
                optionButton.OnClickAsObservable
                    .Subscribe(data =>
                    {
                        controller.Select(button);

                        // ビュー反映
                        _titleUIView.ApplyButtonSelectionState(
                            controller.ButtonArray,
                            controller.SelectStateArray);

                        switch (data.Type)
                        {
                            case OptionType.PlayerCount:
                                _gameOptionManager.SetPlayerCount(data.IntValue);
                                break;

                            case OptionType.LimitTime:
                                _gameOptionManager.SetLimitTime(data.FloatValue);
                                break;

                            case OptionType.BoardSize:
                                _gameOptionManager.SetBoardSize(data.BoardSizeType);
                                break;

                            case OptionType.ConnectCount:
                                _gameOptionManager.SetConnectCount(data.IntValue);
                                break;

                            case OptionType.CameraRotationSpeed:
                                _gameOptionManager.SetCameraRotationSpeed(data.FloatValue);
                                break;

                            case OptionType.PointerSpeed:
                                _gameOptionManager.SetPointerSpeed(data.FloatValue);
                                break;
                        }
                    })
                    .AddTo(optionButton);

                // --------------------------------------------------
                // ホバー開始時
                // --------------------------------------------------
                optionButton.OnHoverEnterAsObservable
                    .Subscribe(_ =>
                    {
                        // フォーカス状態表示を有効化
                        _titleUIView.SetFocus(button, true);

                        // EventSystem の選択状態更新
                        SetSelectedButton(button);

                        // スクリーン座標へ変換
                        Vector2 screenPosition =
                            RectTransformUtility.WorldToScreenPoint(
                                null,
                                rectTransform.position);

                        // フォーカス座標通知
                        _onFocusPosition.OnNext(screenPosition);
                    })
                    .AddTo(optionButton);

                // --------------------------------------------------
                // ホバー終了時
                // --------------------------------------------------
                optionButton.OnHoverExitAsObservable
                    .Subscribe(_ =>
                    {
                        // フォーカス状態表示を無効化
                        _titleUIView.SetFocus(button, false);
                    })
                    .AddTo(optionButton);

                // --------------------------------------------------
                // 選択開始時
                // --------------------------------------------------
                optionButton.OnSelectEnterAsObservable
                    .Subscribe(_ =>
                    {
                        // フォーカス状態表示を有効化
                        _titleUIView.SetFocus(button, true);

                        // スクリーン座標へ変換
                        Vector2 screenPosition =
                            RectTransformUtility.WorldToScreenPoint(
                                null,
                                rectTransform.position);

                        // フォーカス座標通知
                        _onFocusPosition.OnNext(screenPosition);
                    })
                    .AddTo(optionButton);

                // --------------------------------------------------
                // 選択終了時
                // --------------------------------------------------
                optionButton.OnSelectExitAsObservable
                    .Subscribe(_ =>
                    {
                        // フォーカス状態表示を無効化
                        _titleUIView.SetFocus(button, false);
                    })
                    .AddTo(optionButton);
            }
        }

        /// <summary>
        /// OptionButton の入力イベント購読を解除する
        /// </summary>
        private void UnbindOptionButtons()
        {
            // 全 OptionButton を走査
            foreach (KeyValuePair<OptionButton, RectTransform> pair
                in _buttonRectTransformMap)
            {
                OptionButton optionButton = pair.Key;

                if (optionButton == null)
                {
                    continue;
                }

                // イベント購読解除
                optionButton.Dispose();
            }

            // キャッシュ削除
            _buttonRectTransformMap.Clear();
        }

        // --------------------------------------------------
        // フォーカス
        // --------------------------------------------------
        /// <summary>
        /// EventSystem の選択状態を変更する
        /// </summary>
        /// <param name="button">選択対象ボタン</param>
        private void SetSelectedButton(
            Button button)
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
            _eventSystem.SetSelectedGameObject( button.gameObject);
        }

        // ======================================================
        // ボタンイベント
        // ======================================================

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

            // ボタン選択リセット
            ApplySelectionIndexState();
            ApplyAllButtonSelectionState();

            // フォーカスリセット
            _titleUIView.ResetFocus();
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