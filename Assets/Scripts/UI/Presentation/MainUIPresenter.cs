// ======================================================
// MainUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : メインシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using AnimationSystem.Infrastructure;
using InputSystem.Presentation;
using OptionSystem.Presentation;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using SoundSystem.Domain;
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
        /// <summary>現在スコアを表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI[] _currentScoreTexts;

        /// <summary>スコア加算量表示テキスト</summary>
        [SerializeField]
        private TextMeshProUGUI[] _addScoreTexts;

        // --------------------------------------------------
        // ターン
        // --------------------------------------------------
        [Header("ターン")]
        /// <summary>現在ターン数を表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI _turnText;

        // --------------------------------------------------
        // コンボ
        // --------------------------------------------------
        [Header("コンボ")]
        /// <summary>コンボを表示するテキスト</summary>
        [SerializeField]
        private TextMeshPro _comboText;

        /// <summary>最大コンボ表示数</summary>
        [SerializeField]
        private int _maxComboCount;

        /// <summary>
        /// コンボ表示色配列
        /// </summary>
        [SerializeField]
        private Color[] _comboColors;

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        [Header("タイマー")]
        /// <summary>制限時間を表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI[] _limitTimeTexts;

        /// <summary>警告開始タイミング（秒）</summary>
        [SerializeField]
        private int _warningLimitTime = 5;

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

        /// <summary>スコア加算量表示のアニメーター</summary>
        [SerializeField]
        private Animator[] _addScoreAnimators;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI 管理
        // --------------------------------------------------
        /// <summary>入力アイコン収集クラス</summary>
        private InputIconCollector _inputIconCollector = new InputIconCollector();

        /// <summary>断続更新対象のキャンバスのアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _intermittentCanvasAnimationEventNotifier;

        /// <summary>アウトゲームキャンバスのアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _outgameCanvasCanvasAnimationEventNotifier;

        // --------------------------------------------------
        // システム
        // --------------------------------------------------
        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在フェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        /// <summary>ポインターターゲット検出中フラグ</summary>
        private bool _isPointerTarget = false;

        /// <summary>制限時間表示中フラグ</summary>
        private bool _isLimitTimeVisible = false;

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
        /// <summary>断続更新対象キャンバスのアニメーター</summary>
        private Animator _intermittentCanvasAnimator;

        /// <summary>アウトゲーム関連キャンバスのアニメーター</summary>
        private Animator _outgameCanvasAnimator;

        /// <summary>コンボ表示のアニメーター</summary>
        private Animator _comboAnimator;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>IsStart パラメータ名</summary>
        private static readonly int IS_START_HASH = Animator.StringToHash("IsStart");

        /// <summary>IsChangePlayer パラメータ名</summary>
        private static readonly int IS_CHANGE_PLAYER_HASH = Animator.StringToHash("IsChangePlayer");

        /// <summary>IsPause パラメータ名</summary>
        private static readonly int IS_PAUSE_HASH = Animator.StringToHash("IsPause");

        /// <summary>IsFinish パラメータ名</summary>
        private static readonly int IS_FINISH_HASH = Animator.StringToHash("IsFinish");

        /// <summary>IsPlayerID パラメータ名</summary>
        private static readonly int IS_PLAYER_ID_HASH = Animator.StringToHash("IsPlayerID");

        /// <summary>AddScore パラメータ名</summary>
        private static readonly int ADD_SCORE_HASH = Animator.StringToHash("AddScore");

        /// <summary>AddCombo パラメータ名</summary>
        private static readonly int ADD_COMBO_HASH = Animator.StringToHash("AddCombo");

        /// <summary>IsWarning パラメータ名</summary>
        private static readonly int IS_WARNING_HASH = Animator.StringToHash("IsWarning");
        
        /// <summary>SwitchProjection パラメータ名</summary>
        private static readonly int IS_SWITCH_PROJECTION_HASH = Animator.StringToHash("IsSwitchProjection");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>Ready アニメーション終了通知用 Subject</summary>
        private readonly Subject<Unit> _onReadyAnimationEnd = new Subject<Unit>();

        /// <summary>Ready アニメーション終了ストリーム</summary>
        public IObservable<Unit> OnReadyAnimationEnd => _onReadyAnimationEnd;

        /// <summary>Finish アニメーション終了通知用 Subject</summary>
        private readonly Subject<Unit> _onFinishAnimationEnd = new Subject<Unit>();

        /// <summary>Finish アニメーション終了ストリーム</summary>
        public IObservable<Unit> OnFinishAnimationEnd => _onFinishAnimationEnd;

        /// <summary>プレイヤー切り替えアニメーション終了通知用 Subject</summary>
        private readonly Subject<Unit> _onChangePlayerAnimationEnd = new Subject<Unit>();

        /// <summary>プレイヤー切り替えアニメーション終了ストリーム</summary>
        public IObservable<Unit> OnChangePlayerAnimationEnd => _onChangePlayerAnimationEnd;

        /// <summary>投影切り替え用 Subject</summary>
        private readonly Subject<bool> _onSwitchProjection = new Subject<bool>();

        /// <summary>投影切り替えストリーム</summary>
        public IObservable<bool> OnSwitchProjection => _onSwitchProjection;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;

            // --------------------------------------------------
            // UI 管理
            // --------------------------------------------------


            if (_gameOptionManager == null ||
                _inputManager == null ||
                _intermittentCanvas == null ||
                _outgameCanvas == null ||
                _comboText == null)
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
            _uiView = new MainUIView(
                _currentScoreTexts,
                _addScoreTexts,
                _turnText,
                _comboText,
                _limitTimeTexts,
                _gameOptionManager.TurnCount,
                _maxComboCount,
                _comboColors,
                _warningLimitTime
            );
            _uiView.Initialize(
                _binarizationFeature,
                _binarizationMaterial,
                _greyScaleFeature,
                _greyScaleMaterial,
                _distortionFeature,
                _distortionMaterial,
                _pointer,
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
            // ダイアログボタン初期化
            // --------------------------------------------------
            RegisterDialogButtons();

            // --------------------------------------------------
            // 通常ボタン初期化
            // --------------------------------------------------
            // 通常ボタンイベント登録
            RegisterNormalButtons(_mainNormalButtons);

            // --------------------------------------------------
            // UI ボタンの参照解決クラス生成
            // --------------------------------------------------
            _uiActionButtonResolver = new UIActionButtonResolver(_dialogButtonEventTable, _normalButtonEventTable);

            // --------------------------------------------------
            // パネル初期化
            // --------------------------------------------------
            // パネルイベント登録
            RegisterPanelEvents();

            // --------------------------------------------------
            // キャンバス初期化
            // --------------------------------------------------
            // キャンバス状態管理クラス生成
            _uiStateController = new MainUIStateController(
                _uiActionButtonResolver,
                _dialogCanvasArray,
                _initialSelectedPauseCanvasButton
            );

            // --------------------------------------------------
            // アニメーター初期化
            // --------------------------------------------------
            // アニメーター取得
            _intermittentCanvasAnimator = _intermittentCanvas.GetComponent<Animator>();
            _outgameCanvasAnimator = _outgameCanvas.GetComponent<Animator>();
            _comboAnimator = _comboText.gameObject.GetComponent<Animator>();

            // アニメーター速度をタイムスケール非依存に設定
            SetAnimatorUnscaledTime(_outgameCanvasAnimator);
            SetAnimatorUnscaledTime(_limitTimeAnimator);

            // アニメーションイベント通知クラス取得
            _intermittentCanvasAnimationEventNotifier = _intermittentCanvas.GetComponent<AnimationEventNotifier>();
            _outgameCanvasCanvasAnimationEventNotifier = _outgameCanvas.GetComponent<AnimationEventNotifier>();
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (_isPointerLock)
            {
                return;
            }
            
            // ポインター取得
            Vector2 screenPos = _inputManager.Pointer;

            // ビューへ反映
            _uiView.UpdatePointer(screenPos);
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

            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.Dispose();
            }
        }

        // ======================================================
        // ボタン派生イベント
        // ======================================================

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
        /// 通常ボタンクリック時
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        protected override void OnNormalButtonClick(NormalButtonEvent buttonEvent)
        {
            // --------------------------------------------------
            // ダイアログ中の処理
            // --------------------------------------------------
            if (_uiStateController.GetActiveCanvasType() == CanvasType.Dialog)
            {
                HandleDialogButtonClick(buttonEvent);

                return;
            }

            // --------------------------------------------------
            // 通常 UI 処理
            // --------------------------------------------------
            HandleNormalButtonClick(buttonEvent);
        }

        /// <summary>
        /// ダイアログボタンクリック処理
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void HandleDialogButtonClick(in NormalButtonEvent buttonEvent)
        {
            // UI アクション種別へ変換できない場合は処理なし
            if (!_uiActionButtonResolver.TryGetDialogType(buttonEvent, out UIActionType actionType, out DialogType dialogType))
            {
                return;
            }

            // --------------------------------------------------
            // ダイアログ：YES
            // --------------------------------------------------
            if (actionType == UIActionType.DialogYes)
            {
                // SE 再生
                _soundManager?.PlaySE(SeType.UI_Decide);

                // ダイアログイベント実行
                _onDialogEvent.OnNext(dialogType);

                return;
            }

            // --------------------------------------------------
            // ダイアログ：NO
            // --------------------------------------------------
            if (actionType == UIActionType.DialogNo)
            {
                // SE 再生
                _soundManager?.PlaySE(SeType.UI_HideDialog);

                // ポーズ画面のボタンを操作可能に更新
                SetButtonInteractable(_uiActionButtonResolver.GetNormalButton(UIActionType.ReturnToMain), true);
                SetButtonInteractable(_uiActionButtonResolver.GetNormalButton(UIActionType.ReturnToTitle), true);

                // ダイアログキャンバスを非表示にする
                _uiStateController.HideDialogCanvas();

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

                // 最後に選択されていたボタンを取得する
                BaseButtonEvent selectedButtonEvent =
                    _uiStateController.GetLastSelectedButtonEvent(nextCanvasType);

                // 入力状態に応じて初期選択を適用する
                SetSelectionState(nextCanvasType, selectedButtonEvent);

                // ダイアログ非表示を通知する
                _onDialogVisibleChanged.OnNext(false);

                return;
            }
        }

        /// <summary>
        /// 通常ボタンクリック処理
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void HandleNormalButtonClick(in NormalButtonEvent buttonEvent)
        {
            // UI アクション種別へ変換できない場合は処理なし
            if (!_uiActionButtonResolver.TryGetNormalType(buttonEvent, out UIActionType actionType))
            {
                return;
            }

            // --------------------------------------------------
            // メインに戻るボタン
            // --------------------------------------------------
            if (actionType == UIActionType.ReturnToMain)
            {
                // Play フェーズに戻るリクエスト通知
                _onPhaseChangeRequested.OnNext(PhaseType.Play);

                return;
            }

            // --------------------------------------------------
            // タイトルに戻るボタン
            // --------------------------------------------------
            if (actionType == UIActionType.ReturnToTitle)
            {
                // SE 再生
                _soundManager?.PlaySE(SeType.UI_ShowDialog);

                // ポーズ画面のボタンを操作不可に更新
                SetButtonInteractable(_uiActionButtonResolver.GetNormalButton(UIActionType.ReturnToMain), false);
                SetButtonInteractable(_uiActionButtonResolver.GetNormalButton(UIActionType.ReturnToTitle), false);

                // ダイアログキャンバスを表示する
                _uiStateController.ShowDialogCanvas(DialogType.ReturnTitle);

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

                // 入力状態に応じて初期選択を適用する
                SetSelectionState(nextCanvasType);

                // ダイアログ表示を通知する
                _onDialogVisibleChanged.OnNext(true);

                return;
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
            CanvasType activeCanvasType = _uiStateController.GetActiveCanvasType();

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
            CanvasType activeCanvasType = _uiStateController.GetActiveCanvasType();

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
                if (_uiView is MainUIView mainUIView)
                {
                    mainUIView.SetNormalFocus(normalButton.Button, isFocus);
                }
            }
        }

        // ======================================================
        // サウンド派生イベント
        // ======================================================

        /// <summary>
        /// BGM 開始時
        /// </summary>
        protected override void StartBgm()
        {
            _soundManager?.SetBGMVolume(BgmType.Main, 0.5f, 0);
            _soundManager?.PlayBGM(BgmType.Main, 0);
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
        /// <param name="scoreUpdated">スコア加算を通知するストリーム</param>
        /// <param name="onPauseInput">ポーズ入力を通知するストリーム</param>
        /// <param name="pointerLock">ポインターロック状態を通知するストリーム</param>
        /// <param name="gamepadUsed">ゲームパッド使用状態を通知するストリーム</param>
        /// <param name="columnSelectVisibleChanged">列選択表示の表示状態を通知するストリーム</param>
        /// <param name="dropRequested">落下入力予約を通知するストリーム</param>
        /// <param name="rotateRequested">回転入力予約を通知するストリーム</param>
        /// <param name="turnChanged">ターンの現在ターン数を通知するストリーム</param>
        /// <param name="limitTimeChanged">制限時間の残り時間を通知するストリーム</param>
        public void BindStreams(
            in IObservable<PhaseType> phase,
            in IObservable<int> playerChange,
            in IObservable<ScoreEvent> scoreUpdated,
            in IObservable<ScoreEvent> scoreAdded,
            in IObservable<Unit> onPauseInput,
            in IObservable<bool> pointerLock,
            in IObservable<bool> gamepadUsed,
            in IObservable<bool> columnSelectVisibleChanged,
            in IObservable<Unit> dropRequested,
            in IObservable<Unit> rotateRequested,
            in IObservable<int> turnChanged,
            in IObservable<int> comboAdded,
            in IObservable<float> limitTimeChanged)
        {
            phase
                .Skip(1)
                .Subscribe(type =>
                {
                    _currentPhase = type;

                    // Ready
                    bool isReady = _currentPhase == PhaseType.Ready;
                    SetReadyState(isReady);

                    // Play
                    bool isPlay = _currentPhase == PhaseType.Play;
                    SetLimitTimeVisible(isPlay);
                    SetWarningAnimationSpeed(isPlay
                        ? 1.0f
                        : 0.0f);

                    // ChangePlayer
                    bool isChangePlayer = _currentPhase == PhaseType.ChangePlayer;
                    if (isChangePlayer)
                    {
                        TriggerPlayerChange();
                    }

                    // Pause
                    bool isPause = _currentPhase == PhaseType.Pause;
                    SetPauseState(isPause);

                    // Finish
                    bool isFinish = _currentPhase == PhaseType.Finish;
                    SetFinishState(isFinish);
                })
                .AddTo(_disposables);

            playerChange
                .Subscribe(playerIndex => SetChangePlayerState(playerIndex))
                .AddTo(_disposables);

            scoreUpdated
                .Subscribe(e => UpdateCurrentScore(e.PlayerId, e.LineLength))
                .AddTo(_disposables);

            scoreAdded
                .Subscribe(e => UpdateAddScore(e.PlayerId, e.LineLength))
                .AddTo(_disposables);

            onPauseInput
                .Subscribe(_ =>
                {
                    switch (_currentPhase)
                    {
                        case PhaseType.Play:
                            // ポーズフェーズ遷移リクエスト
                            _onPhaseChangeRequested.OnNext(PhaseType.Pause);

                            break;

                        case PhaseType.Pause:

                            // ポーズ画面表示中の場合
                            if (_uiStateController.GetActiveCanvasType() == CanvasType.Pause)
                            {
                                // プレイフェーズ遷移リクエスト
                                _onPhaseChangeRequested.OnNext(PhaseType.Play);
                            }

                            break;
                    }
                })
                .AddTo(_disposables);

            pointerLock
                .DistinctUntilChanged()
                .Subscribe(isLock =>
                {
                    _isPointerLock = isLock;

                    // ロック状態でない場合にポインター表示
                    SetPointerVisible(!isLock);
                })
                .AddTo(_disposables);

            gamepadUsed
                .Subscribe(isUsed =>
                {
                    // 現在の入力デバイス状態を保持
                    _isGamePadInput = isUsed;

                    // 現在アクティブなキャンバス状態を取得
                    CanvasType activeCanvasType = _uiStateController.GetActiveCanvasType();

                    // 最後に選択していたボタンを取得
                    BaseButtonEvent selectedButtonEvent =
                        _uiStateController.GetLastSelectedButtonEvent(activeCanvasType);

                    // 入力状態に応じて初期選択を適用
                    SetSelectionState(activeCanvasType, selectedButtonEvent);

                    // 入力状態に応じて入力アイコン表示を更新
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

            turnChanged
                .DistinctUntilChanged()
                .Subscribe(turn => UpdateTurnCountDisplay(turn))
                .AddTo(_disposables);

            comboAdded
                .DistinctUntilChanged()
                .Subscribe(combo => UpdateComboCountDisplay(combo))
                .AddTo(_disposables);

            limitTimeChanged
                .DistinctUntilChanged()
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
                .Subscribe(_ => _onChangePlayerAnimationEnd.OnNext(Unit.Default))
                .AddTo(_disposables);

            // アニメーション終了通知
            _outgameCanvasCanvasAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ =>
                {
                    if (_currentPhase == PhaseType.Ready)
                    {
                        _onReadyAnimationEnd.OnNext(Unit.Default);

                        return;
                    }

                    if (_currentPhase == PhaseType.Finish)
                    {
                        _onFinishAnimationEnd.OnNext(Unit.Default);
                    }
                })
                .AddTo(_disposables);

            // シーン遷移状態解除
            _isSceneTransitioning = false;

            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.OnComboVisible
                    .Subscribe(_ => TriggerComboVisible())
                    .AddTo(_disposables);
                mainUIView.OnWarningVisible
                    .Subscribe(_ => TriggerWarningVisible())
                    .AddTo(_disposables);

                mainUIView.Subscribe();
            }
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
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// 現在スコア表示更新
        /// </summary>
        /// <param name="playerId">プレイヤー ID（1 ベース）</param>
        /// <param name="score">スコア</param>
        private void UpdateCurrentScore(in int playerId, in int score)
        {
            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.UpdateCurrentScore(playerId, score);

                _soundManager?.PlaySE(SeType.Score_Add);
            }
        }

        /// <summary>
        /// 加算スコア表示更新
        /// </summary>
        /// <param name="playerId">プレイヤー ID（1 ベース）</param>
        /// <param name="score">スコア</param>
        private void UpdateAddScore(in int playerId, in int score)
        {
            if (score <= 0)
            {
                return;
            }

            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.UpdateAddScore(playerId, score);
            }

            // 配列用インデックスへ変換
            int playerIndex = playerId - 1;

            if (playerIndex < 0 || playerIndex >= _addScoreAnimators.Length)
            {
                return;
            }

            Animator animator = _addScoreAnimators[playerIndex];

            if (animator == null)
            {
                return;
            }

            // スコア加算演出を機動
            animator.SetTrigger(ADD_SCORE_HASH);
        }

        // --------------------------------------------------
        // ターン
        // --------------------------------------------------
        /// <summary>
        /// 現在のターン数を UI に表示する
        /// </summary>
        /// <param name="turnCount">現在のターン数</param>
        private void UpdateTurnCountDisplay(in int turnCount)
        {
            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.UpdateTurnCount(turnCount);
            }
        }

        // --------------------------------------------------
        // コンボ
        // --------------------------------------------------
        /// <summary>
        /// 現在のコンボ数を UI に表示する
        /// </summary>
        /// <param name="comboCount">現在のコンボ数</param>
        private void UpdateComboCountDisplay(in int comboCount)
        {
            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.UpdateComboCount(comboCount);
            }
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
            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.SetLimitTimeVisible(isVisible);
            }

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

            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.UpdateLimitTime(limitTime);
            }
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        protected override void SetPointerVisible(in bool isVisible)
        {
            base.SetPointerVisible(isVisible);

            UpdatePointerTargetAnimation(_isPointerTarget);
        }

        /// <summary>
        /// 入力情報 UI の表示を切り替える
        /// 指定されたオブジェクトのみ表示し、それ以外は非表示にする
        /// null の場合は全て非表示
        /// </summary>
        /// <param name="target">表示対象の入力情報 UI</param>
        private void SetInputInfoActive(in GameObject target)
        {
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

            if (!isReady)
            {
                _outgameCanvasAnimator.SetTrigger(IS_START_HASH);
            }
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

            if (_uiStateController is MainUIStateController mainUIStateController)
            {
                if (isPause)
                {
                    // SE 再生
                    _soundManager?.PlaySE(SeType.UI_ShowPause);

                    // ポーズキャンバスを表示する
                    mainUIStateController.ShowPauseCanvas();

                    // 次のキャンバス状態を取得する
                    CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

                    // 最後に選択されていたボタンを取得する
                    BaseButtonEvent selectedButtonEvent =
                        _uiStateController.GetLastSelectedButtonEvent(nextCanvasType);

                    // 入力状態に応じて初期選択を適用する
                    SetSelectionState(nextCanvasType, selectedButtonEvent);

                    // ターゲット検出状態を解除
                    UpdatePointerTargetAnimation(false);
                }
                else
                {
                    // SE 再生
                    _soundManager?.PlaySE(SeType.UI_HidePause);

                    mainUIStateController.HidePauseCanvas();
                }
            }
        }

        /// <summary>
        /// Finish 状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="isFinish">Finish 状態の場合はtrue</param>
        private void SetFinishState(in bool isFinish)
        {
            if (_outgameCanvasAnimator == null)
            {
                return;
            }

            if (isFinish)
            {
                _outgameCanvasAnimator.SetTrigger(IS_FINISH_HASH);
            }
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

        /// <summary>
        /// コンボ表示を起動する
        /// </summary>
        private void TriggerComboVisible()
        {
            _comboAnimator?.SetTrigger(ADD_COMBO_HASH);
        }

        /// <summary>
        /// 警告表示を起動する
        /// </summary>
        private void TriggerWarningVisible()
        {
            _limitTimeAnimator?.SetTrigger(IS_WARNING_HASH);
        }

        /// <summary>
        /// プレイヤー変更による警告アニメーションの強制終了
        /// </summary>
        private void TriggerPlayerChange()
        {
            _limitTimeAnimator?.SetTrigger(IS_CHANGE_PLAYER_HASH);
        }

        /// <summary>
        /// 警告アニメーションの再生速度を設定する
        /// </summary>
        /// <param name="speed">再生速度</param>
        private void SetWarningAnimationSpeed(in float speed)
        {
            if (_limitTimeAnimator == null)
            {
                return;
            }

            _limitTimeAnimator.speed = speed;
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