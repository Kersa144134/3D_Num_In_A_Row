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
using TMPro;
using UniRx;
using AnimationSystem.Infrastructure;
using BoardSystem.Domain;
using InputSystem.Presentation;
using OptionSystem.Presentation;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using ScoreSystem.Presentation;
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
    public sealed class MainUIPresenter : BaseUIPresenter, IUpdatable, IStreamBindable
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

        /// <summary>ScoreManager キャッシュ</summary>
        private ScoreManager _scoreManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在フェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        /// <summary>ポーズ操作における最後のフェーズ切り替え時刻（秒）</summary>
        private float _lastPausePhaseChangeTime;

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

        /// <summary>ラストターン演出開始ターン数</summary>
        private const int LAST_TURN_EFFECT_START_TURN_COUNT = 15;

        /// <summary>ポーズ操作におけるフェーズ切り替えの最小間隔（秒）</summary>
        private const float PAUSE_PHASE_CHANGE_COOLDOWN = 0.2f;

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
        // UniRx 関連
        // ======================================================

        // --------------------------------------------------
        // 購読管理
        // --------------------------------------------------
        /// <summary>イベント購読管理</summary>
        private CompositeDisposable _disposables;

        /// <summary>ターン数変更ストリーム</summary>
        private IObservable<int> _turnChangedStream;

        /// <summary>制限時間変更ストリーム</summary>
        private IObservable<float> _limitTimeChangedStream;

        /// <summary>スコア更新ストリーム</summary>
        private IObservable<ScoreEvent> _scoreUpdatedStream;

        /// <summary>スコア加算ストリーム</summary>
        private IObservable<ScoreEvent> _scoreAddedStream;

        /// <summary>コンボ加算ストリーム</summary>
        private IObservable<int> _comboAddedStream;

        /// <summary>ゲームパッド使用状態ストリーム</summary>
        private IReadOnlyReactiveProperty<bool> _gamepadUsedStream;

        /// <summary>ポインターロック状態ストリーム</summary>
        private IObservable<bool> _pointerLockStream;

        /// <summary>ポーズ入力ストリーム</summary>
        private IObservable<Unit> _pauseInputStream;

        /// <summary>ボード入力種別通知ストリーム</summary>
        private IReadOnlyReactiveProperty<BoardInputType> _boardInputTypeStream;

        /// <summary>プレイヤー変更ストリーム</summary>
        private IObservable<int> _playerChangeStream;

        /// <summary>列選択表示状態ストリーム</summary>
        private IObservable<bool> _columnSelectVisibleChangedStream;

        // --------------------------------------------------
        // イベント
        // --------------------------------------------------
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
            _scoreManager = ScoreManager.Instance;
            _inputManager = InputManager.Instance;

            // --------------------------------------------------
            // UI 管理
            // --------------------------------------------------


            if (_gameOptionManager == null ||
                _scoreManager == null ||
                _inputManager == null ||
                _intermittentCanvas == null ||
                _outgameCanvas == null ||
                _currentScoreTexts == null ||
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
                _warningLimitTime,
                LAST_TURN_EFFECT_START_TURN_COUNT
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

            // フェーズキャッシュ
            _currentPhase = phase;

            // 制限時間表示更新
            SetLimitTimeVisible(phase == PhaseType.Play);

            // 警告アニメーション速度更新
            SetWarningAnimationSpeed(phase == PhaseType.Play
                ? 1.0f
                : 0.0f);

            // --------------------------------------------------
            // フェーズ別処理
            // --------------------------------------------------
            switch (phase)
            {
                case PhaseType.ChangePlayer:
                    // プレイヤー切替演出再生
                    _limitTimeAnimator?.SetTrigger(IS_CHANGE_PLAYER_HASH);

                    // --------------------------------------------------
                    // SE 再生
                    // --------------------------------------------------
                    // 現在の再生位置算出
                    if (!_soundManager.TryGetPlaybackBlockIndex(BgmType.Main, out int currentPlaybackPosition))
                    {
                        return;
                    }

                    // 次の再生位置算出
                    int nextPlaybackPosition = GetNextPlaybackPosition(currentPlaybackPosition);

                    // 次の再生位置が不正値または同一ブロックの場合は処理なし
                    if (nextPlaybackPosition < 0 || currentPlaybackPosition == nextPlaybackPosition)
                    {
                        return;
                    }

                    // BGM 再生位置更新
                    SetPlaybackPosition(nextPlaybackPosition);

                    break;

                case PhaseType.Pause:
                    // ポーズ状態更新
                    SetPauseState(true);

                    break;

                case PhaseType.Finish:
                    // 終了演出再生
                    _outgameCanvasAnimator?.SetTrigger(IS_FINISH_HASH);

                    break;
            }
        }

        protected override void OnPhaseExitInternal(in PhaseType phase)
        {
            base.OnPhaseExitInternal(phase);

            // --------------------------------------------------
            // フェーズ別処理
            // --------------------------------------------------
            switch (phase)
            {
                case PhaseType.Ready:
                    _outgameCanvasAnimator?.SetTrigger(IS_START_HASH);

                    // BGM フェード
                    _soundManager?.SetBGMVolume(BgmType.Main, 0.2f, 1.0f);

                    // 現在の再生位置算出
                    if (!_soundManager.TryGetPlaybackBlockIndex(BgmType.Main, out int currentPlaybackPosition))
                    {
                        return;
                    }

                    // 現在の再生位置が既に 1 の場合は処理なし
                    if (currentPlaybackPosition == 1)
                    {
                        return;
                    }
                    
                    // BGM 再生位置更新
                    SetPlaybackPosition(1);

                    break;

                case PhaseType.Pause:
                    // ポーズ状態更新
                    SetPauseState(false);

                    break;
            }
        }

        protected override void OnExitInternal()
        {
            base.OnExitInternal();

            // イベント購読解除
            UnbindStreams();
            
            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.Dispose();
            }

            // BGM 停止
            StopBgm();
        }

        // ======================================================
        // IStreamBindable イベント
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        public void BindStreams()
        {
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            _turnChangedStream
                .DistinctUntilChanged()
                .Subscribe(turn => UpdateTurnCountDisplay(turn))
                .AddTo(_disposables);

            _limitTimeChangedStream
                .DistinctUntilChanged()
                .Subscribe(time => UpdateLimitTimeDisplay(time))
                .AddTo(_disposables);

            _scoreUpdatedStream
                .Subscribe(e => UpdateCurrentScore(e.PlayerId, e.LineLength))
                .AddTo(_disposables);

            _scoreAddedStream
                .Subscribe(e => UpdateAddScore(e.PlayerId, e.LineLength))
                .AddTo(_disposables);

            _comboAddedStream
                .DistinctUntilChanged()
                .Subscribe(combo => UpdateComboCountDisplay(combo))
                .AddTo(_disposables);

            _gamepadUsedStream
                .DistinctUntilChanged()
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

            _pointerLockStream
                .DistinctUntilChanged()
                .Subscribe(isLock =>
                {
                    _isPointerLock = isLock;

                    // ロック状態でない場合にポインター表示
                    SetPointerVisible(!isLock);
                })
                .AddTo(_disposables);

            _pauseInputStream
                .Subscribe(_ =>
                {
                    // 現在時間取得
                    float currentTime = Time.time;

                    // クールダウン未満なら処理なし
                    if (currentTime - _lastPausePhaseChangeTime < PAUSE_PHASE_CHANGE_COOLDOWN)
                    {
                        return;
                    }

                    // Play → Pause
                    if (_currentPhase == PhaseType.Play)
                    {
                        _onPhaseChangeRequested.OnNext(PhaseType.Pause);
                        _lastPausePhaseChangeTime = currentTime;

                        return;
                    }

                    // Pause → Play
                    if (_currentPhase == PhaseType.Pause &&
                        _uiStateController.GetActiveCanvasType() == CanvasType.Pause)
                    {
                        _onPhaseChangeRequested.OnNext(PhaseType.Play);
                        _lastPausePhaseChangeTime = currentTime;
                    }
                })
                .AddTo(_disposables);

            _boardInputTypeStream
                .DistinctUntilChanged()
                .Subscribe(inputType =>
                {
                    switch (inputType)
                    {
                        case BoardInputType.Drop:
                            {
                                // 平行投影へ切り替え
                                SetSwitchProjection(false);

                                // 落下操作説明を表示
                                SetInputInfoActive(_inputInfoPieceDrop);

                                break;
                            }

                        case BoardInputType.Rotate:
                            {
                                // 透視投影へ切り替え
                                SetSwitchProjection(true);

                                // 回転操作説明を表示
                                SetInputInfoActive(_inputInfoBoardRotation);

                                break;
                            }
                    }
                })
                .AddTo(_disposables);

            // プレイヤー切り替えアニメーション
            _playerChangeStream
                .Subscribe(playerIndex => _intermittentCanvasAnimator?.SetInteger(IS_PLAYER_ID_HASH, playerIndex))
                .AddTo(_disposables);

            _columnSelectVisibleChangedStream
                .DistinctUntilChanged()
                .Subscribe(isVisible =>
                {
                    _isPointerTarget = isVisible;

                    UpdatePointerTargetAnimation(isVisible);
                })
                .AddTo(_disposables);

            Subscribe();
        }

        /// <summary>
        /// イベントストリームを受け取る
        /// </summary>
        public void UnbindStreams()
        {
            _disposables?.Dispose();
        }
        
        // ======================================================
        // 入力継承イベント
        // ======================================================

        /// <summary>キャンセル入力時</summary>
        protected override void OnCancelInput()
        {
            // UI イベント未購読の場合処理なし
            if (_uiEventDisposables == null)
            {
                return;
            }

            switch (_uiStateController.GetActiveCanvasType())
            {
                case CanvasType.Dialog:
                    OnDialogCanvasCancelInput();
                    break;

                case CanvasType.Pause:
                    OnPauseCanvasCancelInput();
                    break;

                default:
                    return;
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
                OnDialogCanvasCancelInput();
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
                OnPauseCanvasCancelInput();
                return;
            }

            // --------------------------------------------------
            // タイトルに戻るボタン
            // --------------------------------------------------
            if (actionType == UIActionType.ReturnToTitle)
            {
                // SE 再生
                _soundManager?.PlaySE(SeType.UI_ShowDialog, 0.5f);

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
        // 画面フェード派生イベント
        // ======================================================

        /// <summary>
        /// フェードアウト開始時
        /// </summary>
        protected override void OnFadeOutStart()
        {
            base.OnFadeOutStart();

            // BGM 再生
            StartBgm();
        }

        /// <summary>
        /// フェードアウト終了時
        /// </summary>
        protected override void OnFadeOutFinish()
        {
            base.OnFadeOutFinish();

            // UI イベント購読
            SubscribeUiEvents();
        }

        // ======================================================
        // サウンド派生イベント
        // ======================================================

        /// <summary>
        /// BGM 再生開始時
        /// </summary>
        protected override void StartBgm()
        {
            _soundManager?.SetBGMVolume(BgmType.Main, 0.1f, 0);
            _soundManager?.PlayBGM(BgmType.Main, 0);
        }

        /// <summary>
        /// BGM 再生停止時
        /// </summary>
        protected override void StopBgm()
        {
            _soundManager?.StopBGM(BgmType.Main);
        }

        /// <summary>
        /// BGM 再生位置更新時
        /// </summary>
        /// <param name="block">対象再生ブロック</param>
        protected override void SetPlaybackPosition(in int block)
        {
            _soundManager?.SetPlaybackPosition(BgmType.Main, block);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        /// <param name="turnChanged">ターンの現在ターン数を通知するストリーム</param>
        /// <param name="limitTimeChanged">制限時間の残り時間を通知するストリーム</param>
        /// <param name="scoreUpdated">スコア更新を通知するストリーム</param>
        /// <param name="scoreAdded">スコア加算を通知するストリーム</param>
        /// <param name="comboAdded">コンボ加算を通知するストリーム</param>
        /// <param name="gamepadUsed">ゲームパッド使用状態を通知するストリーム</param>
        /// <param name="pointerLock">ポインターロック状態を通知するストリーム</param>
        /// <param name="pauseInput">ポーズ入力を通知するストリーム</param>
        /// <param name="boardInputType">ボード入力種別を通知するストリーム</param>
        /// <param name="playerChange">プレイヤーインデックス変更を通知するストリーム</param>
        /// <param name="columnSelectVisibleChanged">列選択表示の表示状態を通知するストリーム</param>
        public void SetStreams(
            in IObservable<int> turnChanged,
            in IObservable<float> limitTimeChanged,
            in IObservable<ScoreEvent> scoreUpdated,
            in IObservable<ScoreEvent> scoreAdded,
            in IObservable<int> comboAdded,
            in IReadOnlyReactiveProperty<bool> gamepadUsed,
            in IObservable<bool> pointerLock,
            in IObservable<Unit> pauseInput,
            in IReadOnlyReactiveProperty<BoardInputType> boardInputType,
            in IObservable<int> playerChange,
            in IObservable<bool> columnSelectVisibleChanged)
        {
            _turnChangedStream = turnChanged;
            _limitTimeChangedStream = limitTimeChanged;
            _scoreUpdatedStream = scoreUpdated;
            _scoreAddedStream = scoreAdded;
            _comboAddedStream = comboAdded;
            _gamepadUsedStream = gamepadUsed;
            _pointerLockStream = pointerLock;
            _pauseInputStream = pauseInput;
            _boardInputTypeStream = boardInputType;
            _playerChangeStream = playerChange;
            _columnSelectVisibleChangedStream = columnSelectVisibleChanged;
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

            if (_uiView is MainUIView mainUIView)
            {
                mainUIView.Subscribe();

                // コンボ加算演出
                mainUIView.OnComboVisible
                    .Subscribe(_ => _comboAnimator?.SetTrigger(ADD_COMBO_HASH))
                    .AddTo(_baseDisposables);

                // 残り時間警告表示
                mainUIView.OnWarningVisible
                    .Subscribe(_ => _limitTimeAnimator?.SetTrigger(IS_WARNING_HASH))
                    .AddTo(_baseDisposables);

                // スコア加算アニメーション開始
                mainUIView.OnAddScoreAnimationStarted
                    .Subscribe(playerIndex => _soundManager?.PlaySE(SeType.Score_Add))
                    .AddTo(_baseDisposables);

                // スコア加算アニメーション終了
                mainUIView.OnAddScoreAnimationFinished
                    .Subscribe(playerIndex => _soundManager?.StopLoopSE(SeType.Score_Add))
                    .AddTo(_baseDisposables);
            }

            // UI イベント購読
            SubscribeUiEvents();
        }

        /// <summary>
        /// UI イベント購読
        /// </summary>
        protected override void SubscribeUiEvents()
        {
            base.SubscribeUiEvents();

            // プレイヤー切り替えアニメーション終了通知
            _intermittentCanvasAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ => _onChangePlayerAnimationEnd.OnNext(Unit.Default))
                .AddTo(_uiEventDisposables);

            // アウトゲーム関連アニメーション終了通知
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
                .AddTo(_uiEventDisposables);
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
        // キャンバス
        // --------------------------------------------------
        /// <summary>
        /// ダイアログキャンバス表示中のキャンセル入力処理
        /// </summary>
        private void OnDialogCanvasCancelInput()
        {
            // SE 再生
            _soundManager?.PlaySE(SeType.UI_HideDialog, 0.5f);

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
        }

        /// <summary>
        /// ポーズキャンバス表示中のキャンセル入力処理
        /// </summary>
        private void OnPauseCanvasCancelInput()
        {
            // Play フェーズに戻るリクエスト通知
            _onPhaseChangeRequested.OnNext(PhaseType.Play);
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

            // スコア加算演出
            animator?.SetTrigger(ADD_SCORE_HASH);
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

            // ターン数がラストターン演出開始ターンの場合
            if (turnCount == LAST_TURN_EFFECT_START_TURN_COUNT)
            {
                // 現在の再生位置算出
                if (!_soundManager.TryGetPlaybackBlockIndex(BgmType.Main, out int currentPlaybackPosition))
                {
                    return;
                }

                // 現在の再生位置が既に 4 の場合は処理なし
                if (currentPlaybackPosition == 4)
                {
                    return;
                }

                // BGM 再生位置更新
                SetPlaybackPosition(4);
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
        /// Pause 状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="isPause">Pause 状態の場合はtrue</param>
        private void SetPauseState(in bool isPause)
        {
            _outgameCanvasAnimator?.SetBool(IS_PAUSE_HASH, isPause);

            if (_uiStateController is MainUIStateController mainUIStateController)
            {
                if (isPause)
                {
                    // BGM フェード
                    _soundManager?.SetBGMVolume(BgmType.Main, 0.1f, 0.2f);

                    // SE 再生
                    _soundManager?.PlaySE(SeType.UI_ShowPause, 0.75f);

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
                    // BGM フェード
                    _soundManager?.SetBGMVolume(BgmType.Main, 0.2f, 1.0f);

                    // SE 再生
                    _soundManager?.PlaySE(SeType.UI_HidePause, 0.75f);

                    mainUIStateController.HidePauseCanvas();
                }
            }
        }

        /// <summary>
        /// 投影方式を切り替える
        /// </summary>
        /// <param name="isSwitch">true:透視 / false:平行</param>
        private void SetSwitchProjection(in bool isSwitch)
        {
            _effectAnimator?.SetBool(IS_SWITCH_PROJECTION_HASH, isSwitch);
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

        // --------------------------------------------------
        // サウンド
        // --------------------------------------------------
        /// <summary>
        /// 次の再生位置取得
        /// </summary>
        /// <param name="currentPlaybackPosition">現在の再生位置</param>
        /// <returns>次の再生位置</returns>
        private int GetNextPlaybackPosition(in int currentPlaybackPosition)
        {
            switch (currentPlaybackPosition)
            {
                case 1:
                case 2:
                case 3:
                    // 最大スコアプレイヤーリスト取得
                    IReadOnlyList<int> highestScorePlayerIndices = _scoreManager.GetHighestScorePlayerIndices();

                    // 有効なプレイヤーが存在しない場合
                    if (highestScorePlayerIndices == null || highestScorePlayerIndices.Count == 0)
                    {
                        return -1;
                    }

                    // 最大スコアプレイヤーが複数存在する場合
                    if (highestScorePlayerIndices.Count > 1)
                    {
                        return 1;
                    }

                    // 最大スコアプレイヤー取得
                    int highestScorePlayerIndex = highestScorePlayerIndices[0];

                    // 偶奇で再生位置切替
                    return highestScorePlayerIndex % 2 == 0
                        ? 2
                        : 3;

                case 7:
                    return 8;

                case 8:
                    return 7;

                default:
                    return -1;
            }
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