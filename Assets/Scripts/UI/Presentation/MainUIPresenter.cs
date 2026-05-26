// ======================================================
// MainUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : メインシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using AnimationSystem.Infrastructure;
using InputSystem.Presentation;
using OptionSystem.Presentation;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using UISystem.Application;
using UISystem.Domain;
using UISystem.Infrastructure;
using UpdateSystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// メインシーンにおける UI 演出を管理するプレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.MainUIPresenter)]
    public sealed class MainUIPresenter : BaseUIPresenter, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("メインシーン固有インスペクタ")]

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        [Header("キャンバス")]
        /// <summary>断続更新対象のキャンバス</summary>
        [SerializeField]
        private GameObject _intermittentCanvas;

        /// <summary>アウトゲーム関連のキャンバス</summary>
        [SerializeField]
        private GameObject _outgameCanvas;

        // --------------------------------------------------
        // プレイヤー情報
        // --------------------------------------------------
        [Header("プレイヤー情報")]
        /// <summary>プレイヤー情報を表示する GameObject 配列</summary>
        [SerializeField]
        private GameObject[] _playerInfoArray;

        // --------------------------------------------------
        // 入力情報
        // --------------------------------------------------
        [Header("入力情報")]
        /// <summary>駒落下時の入力情報を表示する GameObject 配列</summary>
        [SerializeField]
        private GameObject _inputInfoPieceDrop;

        /// <summary>ボード回転時の入力情報を表示する GameObject 配列</summary>
        [SerializeField]
        private GameObject _inputInfoBoardRotation;

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        [Header("スコア")]
        /// <summary>スコアを表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI[] _scoreTexts;

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        [Header("タイマー")]
        /// <summary>制限時間を表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI[] _limitTimeTexts;

        /// <summary>警告開始タイミング（秒）</summary>
        [SerializeField]
        private float _warningLimitTime = 5f;

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        [Header("ボタン")]
        /// <summary>メインシーン用の通常ボタン配列</summary>
        [SerializeField]
        private NormalButton[] _mainNormalButtons;

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

        // --------------------------------------------------
        // 初期選択ボタン
        // --------------------------------------------------
        [Header("初期選択ボタン")]
        /// <summary>ポーズキャンバス初期選択ボタン</summary>
        [SerializeField]
        private BaseButtonEvent _initialSelectedPauseCanvasButton;

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        [Header("アニメーター")]
        /// <summary>制限時間のアニメーター</summary>
        [SerializeField]
        private Animator _limitTimeAnimator;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI 管理
        // --------------------------------------------------
        /// <summary>ビュー</summary>
        private readonly MainUIView _mainUIView = new MainUIView();

        /// <summary>メイン UI のキャンバス状態と初期ボタン選択状態を管理するクラス</summary>
        private MainUIStateController _mainUIStateController;

        /// <summary>入力アイコン収集クラス</summary>
        private InputIconCollector _inputIconCollector = new InputIconCollector();

        /// <summary>断続更新対象のキャンバスのアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _intermittentCanvasAnimationEventNotifier;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ポインターターゲット検出中フラグ</summary>
        private bool _isPointerTarget = false;

        /// <summary>制限時間表示中フラグ</summary>
        private bool _isLimitTimeVisible = false;

        /// <summary>警告アニメーション表示中フラグ</summary>
        private bool _isWarning;

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>ゲームパッド入力アイコン群</summary>
        private Image[] _gamepadInputIcons;

        /// <summary>仮想パッド入力アイコン群</summary>
        private Image[] _virtualpadInputIcons;

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        /// <summary>断続更新対象のキャンバス</summary>
        private Animator _intermittentCanvasAnimator;

        /// <summary>アウトゲーム関連のキャンバス</summary>
        private Animator _outgameCanvasAnimator;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>Ready パラメータ名</summary>
        private static readonly int IS_READY_HASH = Animator.StringToHash("IsReady");

        /// <summary>Pause パラメータ名</summary>
        private static readonly int IS_PAUSE_HASH = Animator.StringToHash("IsPause");

        /// <summary>PlayerID パラメータ名</summary>
        private static readonly int IS_PLAYER_ID_HASH = Animator.StringToHash("IsPlayerID");

        /// <summary>IsWarning パラメータ名</summary>
        private static readonly int IS_WARNING_HASH = Animator.StringToHash("IsWarning");

        /// <summary>SwitchProjection パラメータ名</summary>
        private static readonly int IS_SWITCH_PROJECTION_HASH = Animator.StringToHash("IsSwitchProjection");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>ChangePlayerアニメーション終了通知用 Subject</summary>
        private readonly Subject<Unit> _onChangePlayerAnimationEnd = new Subject<Unit>();

        /// <summary>ChangePlayerアニメーション終了通知ストリーム</summary>
        public IObservable<Unit> OnChangePlayerAnimationEnd => _onChangePlayerAnimationEnd;
        
        /// <summary>投影切り替え用 Subject</summary>
        private readonly Subject<bool> _onSwitchProjection = new Subject<bool>();

        /// <summary>投影切り替えストリーム</summary>
        public IObservable<bool> OnSwitchProjection => _onSwitchProjection;

        /// <summary>フォーカス座標通知用 Subject</summary>
        private readonly Subject<Vector2> _onFocusPosition = new Subject<Vector2>();

        /// <summary>フォーカス座標通知ストリーム</summary>
        public IObservable<Vector2> OnFocusPosition => _onFocusPosition;

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
                _intermittentCanvas == null ||
                _outgameCanvas == null)
            {
                Debug.LogError("[MainUIPresenter] クラスの初期化に失敗しました。");

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
            _mainUIView.InitializeBase(
                _binarizationFeature,
                _binarizationMaterial,
                _greyScaleFeature,
                _greyScaleMaterial,
                _distortionFeature,
                _distortionMaterial,
                _pointer
            );
            _mainUIView.Initialize(
                _scoreTexts,
                _limitTimeTexts,
                _normalFocusOnColor,
                _normalFocusOffColor
            );

            // プレイヤー情報の非表示処理
            for (int i = _gameOptionManager.PlayerCount; i < _playerInfoArray.Length; i++)
            {
                _playerInfoArray[i].SetActive(false);
            }

            // --------------------------------------------------
            // 入力アイコンの取得
            // --------------------------------------------------
            _inputIconCollector.CollectInputIcons(
                _intermittentCanvas,
                _outgameCanvas,
                out _gamepadInputIcons,
                out _virtualpadInputIcons
            );

            // --------------------------------------------------
            // 通常ボタン初期化
            // --------------------------------------------------
            // 通常ボタンイベント登録
            RegisterNormalButtons(_mainNormalButtons);

            // 通常ボタンの参照解決クラス生成
            _normalButtonResolver = new NormalButtonResolver(_normalButtonEventTable);

            // --------------------------------------------------
            // パネル初期化
            // --------------------------------------------------
            // パネルイベント登録
            RegisterPanelEvents();

            // --------------------------------------------------
            // キャンバス初期化
            // --------------------------------------------------
            // キャンバス状態管理クラス生成
            _mainUIStateController = new MainUIStateController(
                _dialogCanvasArray,
                _normalButtonResolver.GetButton(UIActionType.DialogYes),
                _initialSelectedPauseCanvasButton
            );

            // アニメーター取得
            _intermittentCanvasAnimator = _intermittentCanvas.GetComponent<Animator>();
            _outgameCanvasAnimator = _outgameCanvas.GetComponent<Animator>();

            // アニメーター速度をタイムスケール非依存に設定
            SetAnimatorUnscaledTime(_outgameCanvasAnimator);
            SetAnimatorUnscaledTime(_limitTimeAnimator);

            // アニメーションイベント通知クラス取得
            _intermittentCanvasAnimationEventNotifier = _intermittentCanvas.GetComponent<AnimationEventNotifier>();
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            // エフェクト更新
            _mainUIView.UpdateEffect(
                _isBinarizationEnabled,
                _binarizationDistortionCenter,
                _binarizationDistortionStrength,
                _binarizationNoise,
                _binarizationThreshold,
                _binarizationLight,
                _binarizationDark,
                _isGreyScaleEnabled,
                _greyScaleStrength,
                _greyScaleDistortionCenter,
                _greyScaleDistortionStrength,
                _greyScaleNoise,
                _greyScaleLight,
                _greyScaleDark,
                _isDistortionEnabled,
                _distortionCenter,
                _distortionStrength,
                _distortionNoise
            );

            if (_isInputLock)
            {
                return;
            }
            
            // ポインター取得
            Vector2 screenPos = _inputManager.Pointer;

            // ビューへ反映
            _mainUIView.UpdatePointer(screenPos);
        }

        protected override void OnPhaseEnterInternal(in PhaseType phase)
        {
            base.OnPhaseEnterInternal(phase);
        }

        protected override void OnPhaseExitInternal(in PhaseType phase)
        {
            base.OnPhaseExitInternal(phase);
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
        /// <param name="phase">フェーズ状態を通知するストリーム</param>
        /// <param name="playerChange">プレイヤーインデックス変更を通知するストリーム</param>
        /// <param name="scoreUpdated">スコア更新を通知するストリーム</param>
        /// <param name="inputLock">入力ロック状態を通知するストリーム</param>
        /// <param name="gamepadUsed">ゲームパッド使用状態を通知するストリーム</param>
        /// <param name="columnSelectVisibleChanged">列選択表示の表示状態を通知するストリーム</param>
        /// <param name="dropRequested">落下入力予約を通知するストリーム</param>
        /// <param name="rotateRequested">回転入力予約を通知するストリーム</param>
        /// <param name="limitTime">制限時間の残り時間を通知するストリーム</param>
        public void BindStreams(
            in IObservable<PhaseType> phase,
            in IObservable<int> playerChange,
            in IObservable<ScoreEvent> scoreUpdated,
            in IObservable<bool> inputLock,
            in IObservable<bool> gamepadUsed,
            in IObservable<bool> columnSelectVisibleChanged,
            in IObservable<Unit> dropRequested,
            in IObservable<Unit> rotateRequested,
            in IObservable<float> limitTime)
        {
            phase
                .Subscribe(type =>
                {
                    // Ready
                    bool isReady = type == PhaseType.Ready;
                    SetReadyState(isReady);

                    // Play
                    bool isPlay = type == PhaseType.Play;
                    SetLimitTimeVisible(isPlay);

                    // Pause
                    bool isPause = type == PhaseType.Pause;
                    SetPauseState(isPause);
                })
                .AddTo(_disposables);

            playerChange
                .Subscribe(playerIndex => SetChangePlayerState(playerIndex))
                .AddTo(_disposables);

            scoreUpdated
                .Subscribe(e => UpdateScore(e.PlayerId, e.LineLength))
                .AddTo(_disposables);

            inputLock
                .DistinctUntilChanged()
                .Subscribe(isLock =>
                {
                    _isInputLock = isLock;

                    // ロック状態でない場合にポインター表示
                    SetPointerVisible(!isLock);
                })
                .AddTo(_disposables);

            gamepadUsed
                .DistinctUntilChanged()
                .Subscribe(isUsed =>
                {
                    // 現在の入力デバイス状態を保持
                    _isGamePadInput = isUsed;

                    SetInputIconVisible(isUsed);
                })
                .AddTo(_disposables);

            columnSelectVisibleChanged
                .DistinctUntilChanged()
                .Subscribe(isVisible =>
                {
                    _isPointerTarget = isVisible;

                    UpdatePointerTargetAnimation(isVisible);
                })
                .AddTo(_disposables);

            dropRequested
                .Subscribe(_ =>
                {
                    SetSwitchProjection(false);

                    SetInputInfoActive(_inputInfoPieceDrop);
                })
                .AddTo(_disposables);

            rotateRequested
                .Subscribe(_ =>
                {
                    SetSwitchProjection(true);

                    SetInputInfoActive(_inputInfoBoardRotation);
                })
                .AddTo(_disposables);

            limitTime
                .Subscribe(time => UpdateLimitTimeDisplay(time))
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

            // アニメーション終了通知
            _intermittentCanvasAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ =>
                {
                    _onChangePlayerAnimationEnd.OnNext(Unit.Default);
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            _disposables?.Dispose();
        }

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// クリックイベント受信時
        /// </summary>
        /// <param name="clickEvent">クリックイベント</param>
        protected override void OnClickEventInternal(UIClickEvent clickEvent)
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
            // パネル
            // --------------------------------------------------
            if (clickEvent.UIEvent is BasePanelEvent panelEvent)
            {
                OnPanelClick(panelEvent);
            }
        }

        /// <summary>
        /// ホバーイベント受信時
        /// </summary>
        /// <param name="uiEvent">UI イベント</param>
        protected override void OnHoverEventInternal(BaseUIEvent uiEvent)
        {
            // ボタンイベント判定
            if (uiEvent is not BaseButtonEvent buttonEvent)
            {
                return;
            }

            // 現在アクティブなキャンバス状態を取得
            CanvasType activeCanvasType = _mainUIStateController.GetActiveCanvasType();

            // 選択対象のボタンイベントをキャッシュ
            _mainUIStateController.SetLastHoveredButtonEvent(
                activeCanvasType,
                buttonEvent);

            OnSelectButton(buttonEvent);
        }

        /// <summary>
        /// ホバー解除イベント受信時
        /// </summary>
        /// <param name="uiEvent">UI イベント</param>
        protected override void OnUnHoverEventInternal(BaseUIEvent uiEvent)
        {
            // ボタンイベント判定
            if (uiEvent is not BaseButtonEvent buttonEvent)
            {
                return;
            }

            // 現在アクティブなキャンバス状態を取得
            CanvasType activeCanvasType = _mainUIStateController.GetActiveCanvasType();

            // 選択対象のボタンイベントをクリア
            _mainUIStateController.ClearLastHoveredButtonEvent(activeCanvasType);

            // ホバー解除処理
            OnUnSelectButton();
        }

        /// <summary>
        /// フォーカスイベント受信時
        /// </summary>
        /// <param name="uiEvent">UI イベント</param>
        protected override void OnFocusEventInternal(BaseUIEvent uiEvent)
        {
            // ボタンイベント判定
            if (uiEvent is not BaseButtonEvent buttonEvent)
            {
                return;
            }

            OnFocusButton(buttonEvent);
        }

        /// <summary>
        /// フォーカス解除イベント受信時
        /// </summary>
        /// <param name="uiEvent">UI イベント</param>
        protected override void OnUnFocusEventInternal(BaseUIEvent uiEvent)
        {
            // ボタンイベント判定
            if (uiEvent is not BaseButtonEvent buttonEvent)
            {
                return;
            }

            OnUnFocusButton(buttonEvent);
        }

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
                // ダイアログキャンバスを非表示にする
                _mainUIStateController.HideDialogCanvas();

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
                _mainUIStateController.HideDialogCanvas();

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _mainUIStateController.GetActiveCanvasType();

                // 最後に選択されていたボタンを取得する
                BaseButtonEvent selectedButtonEvent =
                    _mainUIStateController.GetLastSelectedButtonEvent(nextCanvasType);

                // 入力状態に応じて初期選択を適用する
                SetSelectionState(nextCanvasType, selectedButtonEvent);

                // ダイアログ非表示を通知する
                _onDialogVisibleChanged.OnNext(false);

                return;
            }

            // --------------------------------------------------
            // メインに戻るボタン
            // --------------------------------------------------
            // タイトルスタートボタン押下時の処理
            if (actionType == UIActionType.ReturnToMain)
            {
                return;
            }

            // --------------------------------------------------
            // タイトルに戻るボタン
            // --------------------------------------------------
            // タイトルスタートボタン押下時の処理
            if (actionType == UIActionType.ReturnToTitle)
            {
                // ダイアログキャンバスを表示する
                _mainUIStateController.ShowDialogCanvas(DialogType.Confirm);

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _mainUIStateController.GetActiveCanvasType();

                // ダイアログ用ボタンを表示する
                _normalButtonResolver.GetButton(UIActionType.DialogYes).gameObject.SetActive(true);
                _normalButtonResolver.GetButton(UIActionType.DialogNo).gameObject.SetActive(true);

                // 初期フォーカスを Yes ボタンに設定する
                SetSelectionState(nextCanvasType, _normalButtonResolver.GetButton(UIActionType.DialogYes));

                // ダイアログ表示を通知する
                _onDialogVisibleChanged.OnNext(true);

                return;
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
                CanvasType activeCanvasType = _mainUIStateController.GetActiveCanvasType();

                // ダイアログの場合
                if (activeCanvasType == CanvasType.Dialog)
                {
                    // ダイアログキャンバス非表示
                    _mainUIStateController.HideDialogCanvas();

                    // 遷移先のキャンバス状態を取得
                    CanvasType nextCanvasType = _mainUIStateController.GetActiveCanvasType();

                    // 最後に選択していたボタンを取得
                    BaseButtonEvent selectedButtonEvent =
                        _mainUIStateController.GetLastSelectedButtonEvent(nextCanvasType);

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
            CanvasType activeCanvasType = _mainUIStateController.GetActiveCanvasType();

            // オプションキャンバスの場合
            if (activeCanvasType == CanvasType.Option)
            {
                // 対象ボタンが OptionButton の場合
                if (buttonEvent is OptionButtonEvent optionButton)
                {
                    // 選択対象のボタンイベントをキャッシュ
                    _mainUIStateController.SetLastSelectedButtonEvent(activeCanvasType, buttonEvent);
                }
            }
            else
            {
                // 選択対象のボタンイベントをキャッシュ
                _mainUIStateController.SetLastSelectedButtonEvent(activeCanvasType, buttonEvent);
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

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// キャンバスと入力状態に応じて選択状態を更新する
        /// </summary>
        /// <param name="canvasType">対象キャンバス</param>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        protected override void SetSelectionState(
            in CanvasType canvasType,
            in BaseButtonEvent buttonEvent = null)
        {
            // 選択状態をリセット
            OnUnSelectButton();

            // 選択対象を解決
            BaseButtonEvent targetButton = _mainUIStateController.ResolveSelection(
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
        protected override void SetFocusState(in BaseButtonEvent buttonEvent, in bool isFocus)
        {
            // 通常ボタンイベント
            if (buttonEvent is NormalButtonEvent normalButton)
            {
                // 通常ボタンのフォーカス状態を有効化
                _mainUIView.SetNormalFocus(normalButton.Button, isFocus);

                return;
            }
        }

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// スコア表示更新
        /// </summary>
        /// <param name="playerId">プレイヤー ID（1 ベース）</param>
        /// <param name="score">スコア</param>
        private void UpdateScore(in int playerId, in int score)
        {
            _mainUIView.UpdateScore(playerId, score);
        }
        
        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        /// <summary>
        /// 制限時間テキストの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        private void SetLimitTimeVisible(in bool isVisible)
        {
            _mainUIView.SetLimitTimeVisible(isVisible);

            _isLimitTimeVisible = isVisible;
        }

        /// <summary>
        /// 制限時間を UI に表示する
        /// </summary>
        /// <param name="limitTime">残り時間（秒）</param>
        private void UpdateLimitTimeDisplay(in float limitTime)
        {
            // 非表示時は処理なし
            if (!_isLimitTimeVisible)
            {
                return;
            }

            _mainUIView.UpdateLimitTime(limitTime);

            // アニメーション更新
            UpdateWarningAnimation(limitTime);
        }

        /// <summary>
        /// 警告アニメーションの状態を更新する
        /// </summary>
        /// <param name="limitTime">残り時間（秒）</param>
        private void UpdateWarningAnimation(in float limitTime)
        {
            // 非表示時は強制解除
            if (!_isLimitTimeVisible)
            {
                SetWarningState(false);

                return;
            }

            // 警告判定
            bool isWarning = limitTime > 0f && limitTime <= _warningLimitTime;

            // 状態変化なし時は処理なし
            if (_isWarning == isWarning)
            {
                return;
            }

            SetWarningState(isWarning);
        }

        /// <summary>
        /// 警告状態を更新する
        /// </summary>
        /// <param name="isWarning">警告状態</param>
        private void SetWarningState(bool isWarning)
        {
            _isWarning = isWarning;

            _limitTimeAnimator?.SetBool(IS_WARNING_HASH, isWarning);
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        private void SetPointerVisible(in bool isVisible)
        {
            _mainUIView.SetPointerVisible(isVisible);

            UpdatePointerTargetAnimation(_isPointerTarget);
        }

        /// <summary>
        /// 入力情報 UI の表示を切り替える
        /// 指定されたオブジェクトのみ表示し、それ以外は非表示にする
        /// null の場合は全て非表示
        /// </summary>
        /// <param name="target">表示対象の入力情報UI</param>
        private void SetInputInfoActive(in GameObject target)
        {
            if (target == null)
            {
                if (_inputInfoPieceDrop != null)
                {
                    _inputInfoPieceDrop.SetActive(false);
                }

                if (_inputInfoBoardRotation != null)
                {
                    _inputInfoBoardRotation.SetActive(false);
                }

                return;
            }

            if (_inputInfoPieceDrop != null)
            {
                _inputInfoPieceDrop.SetActive(_inputInfoPieceDrop == target);
            }

            if (_inputInfoBoardRotation != null)
            {
                _inputInfoBoardRotation.SetActive(_inputInfoBoardRotation == target);
            }
        }

        /// <summary>
        /// 入力アイコンの表示切り替え
        /// </summary>
        /// <param name="isGamepadUsed">
        /// true: Gamepad 表示 / Virtualpad 非表示
        /// false: Virtualpad 表示 / Gamepad 非表示
        /// </param>
        private void SetInputIconVisible(in bool isGamepadUsed)
        {
            // Gamepad
            if (_gamepadInputIcons != null)
            {
                for (int i = 0; i < _gamepadInputIcons.Length; i++)
                {
                    _gamepadInputIcons[i].enabled = isGamepadUsed;
                }
            }

            // Virtualpad
            if (_virtualpadInputIcons != null)
            {
                for (int i = 0; i < _virtualpadInputIcons.Length; i++)
                {
                    _virtualpadInputIcons[i].enabled = !isGamepadUsed;
                }
            }
        }

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        /// <summary>
        /// Ready 状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="isReady">Ready 状態の場合はtrue</param>
        private void SetReadyState(in bool isReady)
        {
            if (_outgameCanvasAnimator == null)
            {
                return;
            }

            _outgameCanvasAnimator.SetBool(IS_READY_HASH, isReady);
        }

        /// <summary>
        /// ChangePlayer 状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="playerId">プレイヤーインデックス</param>
        private void SetChangePlayerState(in int playerId)
        {
            if (_intermittentCanvasAnimator == null)
            {
                return;
            }

            _intermittentCanvasAnimator.SetInteger(IS_PLAYER_ID_HASH, playerId);
        }

        /// <summary>
        /// Pause 状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="isPause">Pause 状態の場合はtrue</param>
        private void SetPauseState(in bool isPause)
        {
            if (_outgameCanvasAnimator == null)
            {
                return;
            }

            _outgameCanvasAnimator.SetBool(IS_PAUSE_HASH, isPause);
        }

        /// <summary>
        /// 投影方式を切り替える
        /// </summary>
        /// <param name="isSwitch">true:透視 / false:平行</param>
        private void SetSwitchProjection(in bool isSwitch)
        {
            if (_effectAnimator == null)
            {
                return;
            }

            _effectAnimator.SetBool(IS_SWITCH_PROJECTION_HASH, isSwitch);
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