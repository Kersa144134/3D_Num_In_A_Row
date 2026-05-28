// ======================================================
// BaseUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-04-09
// 概要     : UI エフェクトのインスペクタ設定と制御を担うプレゼンター
// ======================================================

using PhaseSystem.Domain;
using System;
using System.Collections;
using System.Collections.Generic;
using UISystem.Application;
using UISystem.Domain;
using UISystem.Infrastructure;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
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

        /// <summary>通常ボタンの参照解決クラス</summary>
        protected NormalButtonResolver _normalButtonResolver;

        /// <summary>ダイアログ UI のイベント購読対象を収集するクラス</summary>
        protected readonly DialogUICollector _dialogUICollector = new DialogUICollector();

        /// <summary>フェードシステムキャッシュ</summary>
        protected Fade _fade;

        /// <summary>EventSystem キャッシュ</summary>
        protected EventSystem _eventSystem;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        protected bool _isInputLock = true;

        /// <summary>ゲームパッド入力状態フラグ</summary>
        protected bool _isGamePadInput = false;

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

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>ルーター用購読管理</summary>
        protected readonly CompositeDisposable _routerDisposables = new CompositeDisposable();

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
        /// <summary>シーン遷移リクエスト通知用 Subject</summary>
        private readonly Subject<Unit> _onSceneChangeRequested = new Subject<Unit>();

        /// <summary>シーン遷移リクエスト通知ストリーム</summary>
        public IObservable<Unit> OnSceneChangeRequested => _onSceneChangeRequested;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>IsTarget パラメータ名</summary>
        protected static readonly int IS_TARGET_HASH = Animator.StringToHash("IsTarget");

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // インスタンスからコンポーネント取得
            _eventSystem = EventSystem.current;
            _fade = Fade.Instance;

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
            // エフェクト更新
            _uiView?.UpdateEffect(
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
        protected virtual void OnNormalButtonClick(NormalButtonEvent buttonEvent) { }

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
        // パブリックメソッド
        // ======================================================

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
                })
                .AddTo(_disposables);

            fadeOutSeconds
                .Subscribe(time =>
                {
                    StartCoroutine(FadeOutRoutine(time));
                })
                .AddTo(_disposables);

            fadeCompleted
                .Subscribe(time =>
                {
                    // UI のイベント購読は画面フェード完了後に実行
                    Subscribe();
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
            for (int i = 0; i < _dialogUICollector.Events.Length; i++)
            {
                DialogEvent dialogEvent = _dialogUICollector.Events[i];

                if (dialogEvent == null)
                {
                    continue;
                }

                dialogEvent.OnEvent
                    .Subscribe(HandleDialogEventReceived)
                    .AddTo(_disposables);
            }

            // クリック通知
            _eventRouter.OnClick
                .Subscribe(clickEvent =>
                {
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
                    OnFocusEventInternal(uiEvent);
                })
                .AddTo(_routerDisposables);

            // フォーカス解除通知
            _eventRouter.OnUnFocus
                .Subscribe(uiEvent =>
                {
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
        /// 通常ボタンイベントを登録する
        /// </summary>
        protected void RegisterNormalButtons(in NormalButton[] argumentButtons)
        {
            // ダイアログボタン数
            int dialogCount = _dialogUICollector.Buttons != null
                ? _dialogUICollector.Buttons.Length
                : 0;
            // 引数ボタン数
            int argumentCount = argumentButtons != null
                ? argumentButtons.Length
                : 0;

            // NormalButton 配列生成
            NormalButton[] normalButtons = new NormalButton[dialogCount + argumentCount];

            // ダイアログボタンコピー
            for (int i = 0; i < dialogCount; i++)
            {
                normalButtons[i] = _dialogUICollector.Buttons[i];
            }
            // 引数ボタンコピー
            for (int i = 0; i < argumentCount; i++)
            {
                normalButtons[dialogCount + i] = argumentButtons[i];
            }

            // 辞書生成
            _normalButtonEventTable = _buttonDictionaryBuilder.BuildNormalButtons(normalButtons);

            // イベント登録
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
        private void HandleDialogEventReceived(DialogEventType eventType)
        {
            // シーン遷移
            if (eventType == DialogEventType.RequestSceneChange)
            {
                _onSceneChangeRequested.OnNext(Unit.Default);

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

            if (panelEvent is NormalPanelEvent)
            {
                // 現在アクティブなキャンバス状態を取得
                CanvasType activeCanvasType = _uiStateController.GetActiveCanvasType();

                // ダイアログの場合
                if (activeCanvasType == CanvasType.Dialog)
                {
                    // ダイアログキャンバス非表示
                    _uiStateController.HideDialogCanvas();

                    // 遷移先のキャンバス状態を取得
                    CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

                    // 最後に選択していたボタンを取得
                    BaseButtonEvent selectedButtonEvent =
                        _uiStateController.GetLastSelectedButtonEvent(nextCanvasType);

                    // 遷移先のキャンバスで最後に選択していたボタンを適用
                    SetSelectionState(nextCanvasType, selectedButtonEvent);

                    // ダイアログ非表示を通知
                    _onDialogVisibleChanged.OnNext(false);
                }

                return;
            }
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
            // 現在アクティブなキャンバス状態を取得
            CanvasType activeCanvasType =
                _uiStateController.GetActiveCanvasType();

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

            // スクリーン座標へ変換
            Vector2 screenPosition =
                RectTransformUtility.WorldToScreenPoint(
                    null,
                    buttonEvent.RectTransform.position);

            // フォーカス座標通知
            _onFocusPosition.OnNext(screenPosition);

            // ターゲット検出演出を有効化
            UpdatePointerTargetAnimation(true);
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

        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        /// <summary>
        /// ポインターのターゲット検出状態アニメーションを更新する
        /// </summary>
        protected void UpdatePointerTargetAnimation(in bool isTarget)
        {
            _pointerAnimator?.SetBool(IS_TARGET_HASH, isTarget);
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