// ======================================================
// TitleUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : タイトルシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using InputSystem.Presentation;
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

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

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
        // 定数
        // ======================================================

        /// <summary>BoardSize パラメータ名</summary>
        private static readonly int BOARD_SIZE_HASH = Animator.StringToHash("BoardSize");

        /// <summary>PlayerID パラメータ名</summary>
        private static readonly int IS_PLAYER_ID_HASH = Animator.StringToHash("IsPlayerID");

        /// <summary>Pause パラメータ名</summary>
        private static readonly int IS_PAUSE_HASH = Animator.StringToHash("IsPause");

        /// <summary>SwitchProjection パラメータ名</summary>
        private static readonly int IS_SWITCH_PROJECTION_HASH = Animator.StringToHash("IsSwitchProjection");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>投影切り替え用 Subject</summary>
        private readonly Subject<bool> _onSwitchProjection = new Subject<bool>();

        /// <summary>投影切り替えストリーム</summary>
        public IObservable<bool> OnSwitchProjection => _onSwitchProjection;

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
        /// <param name="isVisible">表示する場合はtrue</param>
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
            _playerCountSelectionController = new ButtonSelectionController(_playerCountButtonArray, 0);
            _limitTimeSelectionController = new ButtonSelectionController(_limitTimeButtonArray, 1);
            _boardSizeSelectionController = new ButtonSelectionController(_boardSizeButtonArray, 0);
            _connectCountSelectionController = new ButtonSelectionController(_connectCountButtonArray, 0);
            _cameraRotationSpeedSelectionController = new ButtonSelectionController(_cameraRotationSpeedButtonArray, 1);
            _pointerSpeedSelectionController = new ButtonSelectionController(_pointerSpeedButtonArray, 1);

            // --------------------------------------------------
            // ビュー更新
            // --------------------------------------------------
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
                OptionButton optionButton =
                    optionButtons[index];

                if (optionButton == null)
                {
                    continue;
                }

                Button button = optionButton.GetComponent<Button>();

                int captureIndex = index;

                if (button == null)
                {
                    continue;
                }

                // クリック時
                optionButton.OnClickAsObservable
                    .Subscribe(_ =>
                    {
                        controller.Select(button);

                        // ビュー反映
                        _titleUIView.ApplyButtonSelectionState(
                            controller.ButtonArray,
                            controller.SelectStateArray);
                    })
                    .AddTo(optionButton);

                // フォーカス開始時
                optionButton.OnFocusEnterAsObservable
                    .Subscribe(_ =>
                    {
                        _titleUIView.SetFocus(button, true);
                    })
                    .AddTo(optionButton);

                // フォーカス終了
                optionButton.OnFocusExitAsObservable
                    .Subscribe(_ =>
                    {
                        _titleUIView.SetFocus(button, false);
                    })
                    .AddTo(optionButton);
            }
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