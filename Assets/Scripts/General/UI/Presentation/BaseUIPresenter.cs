// ======================================================
// BaseUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-04-09
// 概要     : UI エフェクトのインスペクタ設定と制御を担うプレゼンター
// ======================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UniRx;
using PhaseSystem.Domain;
using SoundSystem.Domain;
using SoundSystem.Presentation;
using UISystem.Application;
using UISystem.Domain;
using UISystem.Infrastructure;
using UpdateSystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// UI エフェクトの制御を行うプレゼンター
    /// </summary>
    public abstract class BaseUIPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // --------------------------------------------------
        // ダイアログ
        // --------------------------------------------------
        [Header("ダイアログ")]
        /// <summary>ダイアログ関連の UI を表示するキャンバス配列</summary>
        [SerializeField]
        protected DialogCanvasDefinition[] _dialogCanvasArray;

        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        [Header("ポインター")]
        /// <summary>ポインターを表示する Image</summary>
        [SerializeField]
        protected GameObject _pointer;

        // --------------------------------------------------
        // 演出 <2 値化>
        // --------------------------------------------------
        [Header("演出 <2 値化>")]
        /// <summary>2 値化エフェクトのRenderFeature</summary>
        [SerializeField]
        protected ScriptableRendererFeature _binarizationFeature;

        /// <summary>2 値化エフェクト用マテリアル</summary>
        [SerializeField]
        protected Material _binarizationMaterial;

        /// <summary>2 値化エフェクトの有効状態</summary>
        [SerializeField]
        protected bool _isBinarizationEnabled;

        /// <summary>歪み中心座標</summary>
        [SerializeField]
        protected Vector2 _binarizationDistortionCenter;

        /// <summary>歪み強度</summary>
        [SerializeField]
        protected float _binarizationDistortionStrength;

        /// <summary>ノイズ強度</summary>
        [SerializeField]
        protected float _binarizationNoise;

        /// <summary>ポスタライズ閾値</summary>
        [SerializeField]
        protected float _binarizationThreshold;

        /// <summary>明部カラー</summary>
        [SerializeField]
        protected Color _binarizationLight;

        /// <summary>暗部カラー</summary>
        [SerializeField]
        protected Color _binarizationDark;

        // --------------------------------------------------
        // 演出 <グレースケール>
        // --------------------------------------------------
        [Header("演出 <グレースケール>")]
        /// <summary>グレースケールのRenderFeature</summary>
        [SerializeField]
        protected ScriptableRendererFeature _greyScaleFeature;

        /// <summary>グレースケール用マテリアル</summary>
        [SerializeField]
        protected Material _greyScaleMaterial;

        /// <summary>グレースケールの有効状態</summary>
        [SerializeField]
        protected bool _isGreyScaleEnabled;

        /// <summary>グレースケール強度</summary>
        [SerializeField]
        protected Vector3 _greyScaleStrength;

        /// <summary>歪み中心</summary>
        [SerializeField]
        protected Vector2 _greyScaleDistortionCenter;

        /// <summary>歪み強度</summary>
        [SerializeField]
        protected float _greyScaleDistortionStrength;

        /// <summary>ノイズ強度</summary>
        [SerializeField]
        protected float _greyScaleNoise;

        /// <summary>明部カラー</summary>
        [SerializeField]
        protected Color _greyScaleLight;

        /// <summary>暗部カラー</summary>
        [SerializeField]
        protected Color _greyScaleDark;

        // --------------------------------------------------
        // 演出 <歪み>
        // --------------------------------------------------
        [Header("演出 <歪み>")]
        /// <summary>歪みのRenderFeature</summary>
        [SerializeField]
        protected ScriptableRendererFeature _distortionFeature;

        /// <summary>歪み用マテリアル</summary>
        [SerializeField]
        protected Material _distortionMaterial;

        /// <summary>歪みの有効状態</summary>
        [SerializeField]
        protected bool _isDistortionEnabled;

        /// <summary>歪み中心</summary>
        [SerializeField]
        protected Vector2 _distortionCenter;

        /// <summary>歪み強度</summary>
        [SerializeField]
        protected float _distortionStrength;

        /// <summary>ノイズ強度</summary>
        [SerializeField]
        protected float _distortionNoise;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>UI ビュー</summary>
        protected BaseUIView _uiView;

        /// <summary>イベントを仲介するクラス</summary>
        protected readonly UIEventRouter _eventRouter = new UIEventRouter();

        /// <summary>UI 状態制御クラス</summary>
        protected BaseUIStateController _uiStateController;

        /// <summary>ボタンの辞書およびバインダー構築を行うクラス</summary>
        protected readonly ButtonDictionaryBuilder _buttonDictionaryBuilder = new ButtonDictionaryBuilder();

        /// <summary>UI ボタンの参照解決クラス</summary>
        protected UIActionButtonResolver _uiActionButtonResolver;

        /// <summary>ダイアログ UI のイベント購読対象を収集するクラス</summary>
        protected readonly DialogUICollector _dialogUICollector = new DialogUICollector();

        /// <summary>フェードシステムキャッシュ</summary>
        protected Fade _fade;

        /// <summary>EventSystem キャッシュ</summary>
        protected EventSystem _eventSystem;

        /// <summary>SoundManager キャッシュ</summary>
        protected SoundManager _soundManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ゲームパッド入力状態フラグ</summary>
        protected bool _isGamePadInput = false;

        /// <summary>ポインターロックフラグ</summary>
        protected bool _isPointerLock = false;

        /// <summary>シーン遷移中かどうかを示すフラグ</summary>
        protected bool _isSceneTransitioning = true;

        /// <summary>フォーカス中ボタン RectTransform</summary>
        private RectTransform _focusButtonRectTransform;

        /// <summary>フォーカス中ボタンのルート Canvas</summary>
        private Canvas _focusCanvas;

        /// <summary>フォーカス中ボタンのルート Canvas の RectTransform</summary>
        private RectTransform _focusCanvasRectTransform;
        
        /// <summary>エフェクト用アニメーター</summary>
        protected Animator _effectAnimator;

        /// <summary>ポインターアニメーター</summary>
        protected Animator _pointerAnimator;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 通常ボタンイベント辞書
        /// </summary>
        protected Dictionary<UIActionType, NormalButtonEvent> _normalButtonEventTable
            = new Dictionary<UIActionType, NormalButtonEvent>();

        /// <summary>
        /// ダイアログ用通常ボタンイベント辞書
        /// </summary>
        protected Dictionary<(UIActionType uiActionType, DialogType dialogType), NormalButtonEvent> _dialogButtonEventTable
            = new Dictionary<(UIActionType uiActionType, DialogType dialogType), NormalButtonEvent>();

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>ルーター用購読管理</summary>
        protected readonly CompositeDisposable _routerDisposables = new CompositeDisposable();

        /// <summary>フェーズ遷移予約通知用 Subject</summary>
        protected readonly Subject<PhaseType> _onPhaseChangeRequested = new Subject<PhaseType>();

        /// <summary>フェーズ遷移予約ストリーム</summary>
        public IObservable<PhaseType> OnPhaseChangeRequested => _onPhaseChangeRequested;

        /// <summary>ダイアログ表示状態通知用 Subject</summary>
        protected readonly Subject<bool> _onDialogVisibleChanged = new Subject<bool>();

        /// <summary>ダイアログ表示状態通知ストリーム</summary>
        public IObservable<bool> OnDialogVisibleChanged => _onDialogVisibleChanged;

        /// <summary>フォーカス座標通知用 Subject</summary>
        private readonly Subject<Vector2> _onFocusPosition = new Subject<Vector2>();

        /// <summary>フォーカス座標通知ストリーム</summary>
        public IObservable<Vector2> OnFocusPosition => _onFocusPosition;

        /// <summary>フェードイン完了通知用 Subject</summary>
        private readonly Subject<Unit> _onFadeInCompleted = new Subject<Unit>();

        /// <summary>フェードイン完了通知ストリーム</summary>
        public IObservable<Unit> OnFadeInCompletedStream => _onFadeInCompleted;

        /// <summary>フェードアウト完了通知用 Subject</summary>
        private readonly Subject<Unit> _onFadeOutCompleted = new Subject<Unit>();

        /// <summary>フェードアウト完了通知ストリーム</summary>
        public IObservable<Unit> OnFadeOutCompletedStream => _onFadeOutCompleted;

        // --------------------------------------------------
        // ダイアログイベント
        // --------------------------------------------------
        /// <summary>ダイアログイベント通知用 Subject</summary>
        protected readonly Subject<DialogType> _onDialogEvent = new Subject<DialogType>();

        /// <summary>ダイアログイベントストリーム</summary>
        public IObservable<DialogType> OnDialogEvent => _onDialogEvent;

        /// <summary>シーン遷移リクエスト通知用 Subject</summary>
        protected readonly Subject<Unit> _onSceneChangeRequested = new Subject<Unit>();

        /// <summary>シーン遷移リクエスト通知ストリーム</summary>
        public IObservable<Unit> OnSceneChangeRequested => _onSceneChangeRequested;

        /// <summary>ゲーム終了リクエスト通知用 Subject</summary>
        protected readonly Subject<Unit> _onExitGameRequested = new Subject<Unit>();

        /// <summary>ゲーム終了リクエスト通知ストリーム</summary>
        public IObservable<Unit> OnExitGameRequested => _onExitGameRequested;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>IsTarget パラメータ名</summary>
        protected static readonly int IS_TARGET_HASH = Animator.StringToHash("IsTarget");

        /// <summary>通常ボタン選択時の拡大倍率</summary>
        private const float NORMAL_BUTTON_SELECTED_SCALE = 1.05f;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // インスタンスからコンポーネント取得
            _eventSystem = EventSystem.current;
            _fade = Fade.Instance;
            _soundManager = SoundManager.Instance;

            if (_eventSystem == null ||
                _fade == null ||
                _dialogCanvasArray == null ||
                _pointer == null)
            {
                Debug.LogError("[BaseUIPresenter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }
            
            // ダイアログ UI コンポーネント取得
            _dialogUICollector.Collect(_dialogCanvasArray);

            // アニメーター取得
            _effectAnimator = GetComponent<Animator>();
            _pointerAnimator = _pointer.GetComponent<Animator>();

            // アニメーター速度をタイムスケール非依存に設定
            SetAnimatorUnscaledTime(_effectAnimator);
            SetAnimatorUnscaledTime(_pointerAnimator);

            OnEnterInternal();
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // 全画面エフェクト更新
            _uiView?.UpdateFullScreenEffect(
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

            // ゲームパッド入力かつフォーカス中ボタンが存在する場合
            if (_isGamePadInput &&
                _focusButtonRectTransform != null &&
                _focusCanvasRectTransform != null)
            {
                // Canvas ローカル座標へ変換
                Vector2 localPosition = _focusCanvasRectTransform.InverseTransformPoint(_focusButtonRectTransform.position);

                // Canvas 左下基準へ変換
                Vector2 canvasPosition =localPosition + (_focusCanvasRectTransform.rect.size * 0.5f);

                // フォーカス座標通知
                _onFocusPosition.OnNext(canvasPosition);
            }
            OnLateUpdateInternal(unscaledDeltaTime);
        }

        public void OnPhaseEnter(in PhaseType phase)
        {
            OnPhaseEnterInternal(phase);
        }

        public void OnPhaseExit(in PhaseType phase)
        {
            OnPhaseExitInternal(phase);
        }

        public void OnExit()
        {
            Dispose();
            
            OnExitInternal();
        }

        // ======================================================
        // IUpdatable 継承イベント
        // ======================================================

        protected virtual void OnEnterInternal() { }

        protected virtual void OnLateUpdateInternal(in float unscaledDeltaTime) { }

        protected virtual void OnPhaseEnterInternal(in PhaseType phase) { }

        protected virtual void OnPhaseExitInternal(in PhaseType phase) { }

        protected virtual void OnExitInternal() { }

        // ======================================================
        // ボタン継承イベント
        // ======================================================

        /// <summary>クリックイベント受信時</summary>
        protected virtual void OnClickEventInternal(UIClickEvent clickEvent) { }

        /// <summary>通常ボタンクリック時</summary>
        protected virtual void OnNormalButtonClick(NormalButtonEvent buttonEvent){ }

        /// <summary>オプションボタンクリック時</summary>
        protected virtual void OnOptionButtonClick(OptionButtonEvent buttonEvent) { }
        
        /// <summary>ホバーイベント受信時</summary>
        protected virtual void OnHoverEventInternal(BaseUIEvent uiEvent) { }

        /// <summary>ホバー解除イベント受信時</summary>
        protected virtual void OnUnHoverEventInternal(BaseUIEvent uiEvent) { }

        /// <summary>フォーカスイベント受信時</summary>
        protected virtual void OnFocusEventInternal(BaseUIEvent uiEvent) { }

        /// <summary>フォーカス解除イベント受信時</summary>
        protected virtual void OnUnFocusEventInternal(BaseUIEvent uiEvent) { }

        /// <summary>ボタンイベントに応じてフォーカス状態を設定する</summary>
        protected virtual void SetFocusState(in BaseButtonEvent buttonEvent, in bool isFocus) { }

        // ======================================================
        // 画面フェード継承イベント
        // ======================================================

        /// <summary>フェードイン開始時</summary>
        protected virtual void OnFadeInStart() { }

        /// <summary>フェードアウト開始時</summary>
        protected virtual void OnFadeOutStart() { }

        // ======================================================
        // サウンド継承イベント
        // ======================================================

        /// <summary>BGM 開始時</summary>
        protected virtual void StartBgm() { }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// 共通イベントストリームをまとめて購読する
        /// </summary>
        public void BindBaseStreams(
            in IObservable<float> fadeInSeconds,
            in IObservable<float> fadeOutSeconds,
            in IObservable<Unit> fadeCompleted)
        {
            fadeInSeconds
                .Subscribe(time =>
                {
                    StartCoroutine(FadeInRoutine(time));

                    OnFadeInStart();
                })
                .AddTo(_disposables);

            fadeOutSeconds
                .Subscribe(time =>
                {
                    StartCoroutine(FadeOutRoutine(time));

                    OnFadeOutStart();
                })
                .AddTo(_disposables);

            fadeCompleted
                .Take(1)
                .Subscribe(_ =>
                {
                    // UI のイベント購読は画面フェード完了後に実行
                    Subscribe();

                    // BGM 再生
                    StartBgm();
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
        protected virtual void Subscribe()
        {
            // ダイアログイベント
            for (int i = 0; i < _dialogCanvasArray.Length; i++)
            {
                _onDialogEvent
                    .Subscribe(eventType => HandleDialogEventReceived(eventType))
                    .AddTo(_disposables);
            }

            // クリック通知
            _eventRouter.OnClick
                .Subscribe(clickEvent =>
                {
                    // シーン遷移中はクリック無効
                    if (_isSceneTransitioning)
                    {
                        return;
                    }

                    OnClickEventInternal(clickEvent);
                })
                .AddTo(_routerDisposables);

            // ホバー通知
            _eventRouter.OnHover
                .Subscribe(uiEvent =>
                {
                    OnHoverEventInternal(uiEvent);
                })
                .AddTo(_routerDisposables);

            // ホバー解除通知
            _eventRouter.OnUnHover
                .Subscribe(uiEvent =>
                {
                    OnUnHoverEventInternal(uiEvent);
                })
                .AddTo(_routerDisposables);

            // フォーカス通知
            _eventRouter.OnFocus
                .Subscribe(uiEvent =>
                {
                    // 通常ボタンイベントの場合、ボタンスケール変更
                    if (uiEvent is NormalButtonEvent)
                    {
                        uiEvent.transform.localScale = new Vector3(
                            NORMAL_BUTTON_SELECTED_SCALE,
                            NORMAL_BUTTON_SELECTED_SCALE,
                            NORMAL_BUTTON_SELECTED_SCALE);
                    }

                    OnFocusEventInternal(uiEvent);
                })
                .AddTo(_routerDisposables);

            // フォーカス解除通知
            _eventRouter.OnUnFocus
                .Subscribe(uiEvent =>
                {
                    // 通常ボタンイベントの場合、ボタンスケール変更
                    if (uiEvent is NormalButtonEvent)
                    {
                        uiEvent.transform.localScale = Vector3.one;
                    }

                    OnUnFocusEventInternal(uiEvent);
                })
                .AddTo(_routerDisposables);
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        protected virtual void Dispose()
        {
            _disposables?.Dispose();
            _routerDisposables?.Dispose();

            _eventRouter?.Dispose();
        }

        /// <summary>
        /// ダイアログボタンイベントを登録する
        /// DialogType付きボタンのみ処理する
        /// </summary>
        protected void RegisterDialogButtons()
        {
            // --------------------------------------------------
            // ダイアログボタン取得
            // --------------------------------------------------
            DialogButton[] dialogButtons = _dialogUICollector.Buttons;

            if (dialogButtons == null || dialogButtons.Length == 0)
            {
                return;
            }

            // --------------------------------------------------
            // ビルド処理
            // --------------------------------------------------
            Dictionary<(UIActionType, DialogType), NormalButtonEvent> dialogTable =
                _buttonDictionaryBuilder.BuildDialogButtons(dialogButtons);

            // --------------------------------------------------
            // 辞書登録
            // --------------------------------------------------
            foreach (KeyValuePair<(UIActionType, DialogType), NormalButtonEvent> kv in dialogTable)
            {
                _dialogButtonEventTable[kv.Key] = kv.Value;
            }

            // --------------------------------------------------
            // イベント登録
            // --------------------------------------------------
            foreach (NormalButtonEvent buttonEvent in _dialogButtonEventTable.Values)
            {
                _eventRouter.RegisterNormalButton(buttonEvent);
            }
        }
        
        /// <summary>
        /// 通常ボタンイベントを登録する
        /// </summary>
        protected void RegisterNormalButtons(in NormalButton[] normalButtons)
        {
            if (normalButtons == null || normalButtons.Length == 0)
            {
                return;
            }

            // --------------------------------------------------
            // ビルド処理
            // --------------------------------------------------
            Dictionary<UIActionType, NormalButtonEvent> normalTable =
                _buttonDictionaryBuilder.BuildNormalButtons(normalButtons);

            // --------------------------------------------------
            // 辞書登録
            // --------------------------------------------------
            foreach (KeyValuePair<UIActionType, NormalButtonEvent> kv in normalTable)
            {
                _normalButtonEventTable[kv.Key] = kv.Value;
            }

            // --------------------------------------------------
            // イベント登録
            // --------------------------------------------------
            foreach (NormalButtonEvent buttonEvent in _normalButtonEventTable.Values)
            {
                _eventRouter.RegisterNormalButton(buttonEvent);
            }
        }

        /// <summary>
        /// パネルイベントを登録する
        /// </summary>
        protected void RegisterPanelEvents()
        {
            // イベント登録
            foreach (BasePanelEvent panelEvent in _dialogUICollector.Panels)
            {
                _eventRouter.RegisterPanelEvent(panelEvent);
            }
        }

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// ダイアログイベント実行時
        /// </summary>
        private void HandleDialogEventReceived(DialogType dialogType)
        {
            // ゲーム開始
            // タイトルに戻る
            if (dialogType == DialogType.StartGame ||
                dialogType == DialogType.ReturnTitle)
            {
                _onSceneChangeRequested.OnNext(Unit.Default);

                // シーン遷移中フラグを有効化
                _isSceneTransitioning = true;

                return;
            }

            // ゲーム終了
            if (dialogType == DialogType.ExitGame)
            {
                _onExitGameRequested.OnNext(Unit.Default);

                // フェードイン開始
                OnFadeInStart();

                return;
            }
        }

        /// <summary>
        /// パネルクリック時
        /// </summary>
        /// <param name="panelEvent">対象パネルイベント</param>
        protected void OnPanelClick(BasePanelEvent panelEvent)
        {
            if (panelEvent == null)
            {
                return;
            }

            // 現在アクティブなキャンバス状態を取得
            CanvasType activeCanvasType = _uiStateController.GetActiveCanvasType();

            // SE 再生
            // ダイアログの場合
            if (activeCanvasType == CanvasType.Dialog)
            {
                _soundManager?.PlaySE(SeType.UI_HideDialog);
            }
            else
            {
                _soundManager?.PlaySE(SeType.UI_Click);
            }

            // ダイアログ以外は処理なし
            if (activeCanvasType != CanvasType.Dialog)
            {
                return;
            }

            // NormalPanelEvent 以外は処理なし
            if (panelEvent is not NormalPanelEvent)
            {
                return;
            }

            // ダイアログキャンバス非表示
            _uiStateController.HideDialogCanvas();

            // 遷移先のキャンバス状態を取得
            CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

            // 最後に選択していたボタンを取得
            BaseButtonEvent selectedButtonEvent = _uiStateController.GetLastSelectedButtonEvent(nextCanvasType);

            // 遷移先のキャンバスで最後に選択していたボタンを適用
            SetSelectionState(nextCanvasType, selectedButtonEvent);

            // ダイアログ非表示を通知
            _onDialogVisibleChanged.OnNext(false);
        }

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// ボタンへフォーカス状態を適用し、フォーカス座標を通知する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        protected void OnFocusButton(BaseButtonEvent buttonEvent)
        {
            // SE 再生
            _soundManager?.PlaySE(SeType.UI_Focus);

            // 現在アクティブなキャンバス状態を取得
            CanvasType activeCanvasType = _uiStateController.GetActiveCanvasType();

            // オプションキャンバスの場合
            if (activeCanvasType == CanvasType.Option)
            {
                // OptionButton のみ選択状態を保存
                if (buttonEvent is OptionButtonEvent)
                {
                    _uiStateController.SetLastSelectedButtonEvent(
                        activeCanvasType,
                        buttonEvent);
                }
            }
            else
            {
                // 通常選択状態を保存
                _uiStateController.SetLastSelectedButtonEvent(
                    activeCanvasType,
                    buttonEvent);
            }

            // フォーカス状態表示
            SetFocusState(buttonEvent, true);

            // ターゲット検出演出を有効化
            UpdatePointerTargetAnimation(true);

            // ボタンの RectTransform をキャッシュ
            _focusButtonRectTransform = buttonEvent.RectTransform;

            // フォーカス対象未設定の場合
            if (_focusButtonRectTransform == null)
            {
                // Canvas キャッシュクリア
                _focusCanvas = null;
                _focusCanvasRectTransform = null;

                return;
            }

            // 親 Canvas 取得
            _focusCanvas = _focusButtonRectTransform.GetComponentInParent<Canvas>();

            // Canvas の RectTransform キャッシュ
            _focusCanvasRectTransform = _focusCanvas.transform as RectTransform;
        }

        /// <summary>
        /// ボタンのフォーカス状態を解除する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        protected void OnUnFocusButton(BaseButtonEvent buttonEvent)
        {
            // フォーカス状態非表示
            SetFocusState(buttonEvent, false);

            // ターゲット検出状態を解除
            UpdatePointerTargetAnimation(false);

            // RectTransform キャッシュを破棄
            _focusButtonRectTransform = null;
            _focusCanvas = null;
            _focusCanvasRectTransform = null;
        }

        /// <summary>
        /// EventSystem の選択状態を変更する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        protected void OnSelectButton(BaseButtonEvent buttonEvent)
        {
            // 現在選択中のオブジェクト取得
            GameObject currentSelectedObject = _eventSystem.currentSelectedGameObject;

            // 同一オブジェクトが選択されている場合
            if (currentSelectedObject == buttonEvent.gameObject)
            {
                // ターゲット検出演出を有効化
                UpdatePointerTargetAnimation(true);
                
                return;
            }

            // 選択状態を更新
            _eventSystem.SetSelectedGameObject(buttonEvent.gameObject);
        }

        /// <summary>
        /// EventSystem の選択状態を解除する
        /// </summary>
        protected void OnUnSelectButton()
        {
            // 選択解除
            _eventSystem.SetSelectedGameObject(null);
        }

        /// <summary>
        /// キャンバスと入力状態に応じて選択状態を更新する
        /// </summary>
        /// <param name="canvasType">対象キャンバス</param>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        protected void SetSelectionState(
            in CanvasType canvasType,
            in BaseButtonEvent buttonEvent = null)
        {
            // 選択状態をリセット
            OnUnSelectButton();

            // 選択対象を解決
            BaseButtonEvent targetButton = _uiStateController.ResolveSelection(
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
        /// 指定したボタンイベントの interactable 状態を更新する
        /// </summary>
        /// /// <param name="buttonEvent">対象ボタンイベント</param>
        /// <param name="isInteractable">ボタンに設定する interactable 状態</param>
        protected void SetButtonInteractable(
            in BaseButtonEvent buttonEvent,
            in bool isInteractable)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // ボタンの操作可能状態を更新する
            buttonEvent.Button.interactable = isInteractable;
        }

        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        protected virtual void SetPointerVisible(in bool isVisible)
        {
            _uiView.SetPointerVisible(isVisible);
        }

        /// <summary>
        /// ポインターのターゲット検出状態アニメーションを更新する
        /// </summary>
        protected void UpdatePointerTargetAnimation(in bool isTarget)
        {
            if (_pointerAnimator == null)
            {
                return;
            }

            _pointerAnimator.SetBool(IS_TARGET_HASH, isTarget);
        }

        // --------------------------------------------------
        // アニメーション
        // --------------------------------------------------
        /// <summary>
        /// アニメーターをタイムスケール非依存に設定する
        /// </summary>
        protected void SetAnimatorUnscaledTime(in Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        /// <summary>
        /// フェードイン処理
        /// </summary>
        private IEnumerator FadeInRoutine(float time)
        {
            // 完了状態フラグ
            bool isCompleted = false;

            // フェード開始
            _fade.FadeIn(time, () =>
            {
                isCompleted = true;
            });

            // 完了待機
            while (isCompleted == false)
            {
                yield return null;
            }

            // 完了通知
            _onFadeInCompleted.OnNext(Unit.Default);
        }

        /// <summary>
        /// フェードアウト処理
        /// </summary>
        private IEnumerator FadeOutRoutine(float time)
        {
            // 完了状態フラグ
            bool isCompleted = false;

            // フェード開始
            _fade.FadeOut(time, () =>
            {
                isCompleted = true;
            });

            // 完了待機
            while (isCompleted == false)
            {
                yield return null;
            }

            // 完了通知
            _onFadeOutCompleted.OnNext(Unit.Default);
        }
    }
}