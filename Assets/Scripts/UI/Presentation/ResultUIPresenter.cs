// ======================================================
// ResultUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-29
// 更新日時 : 2026-05-29
// 概要     : リザルトシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using System;
using UnityEngine;
using UniRx;
using AnimationSystem.Infrastructure;
using InputSystem.Presentation;
using ScoreSystem.Presentation;
using UISystem.Application;
using UISystem.Domain;
using UISystem.Infrastructure;
using UpdateSystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// タイトルシーンにおける UI 演出を管理するプレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.ResultUIPresenter)]
    public sealed class ResultUIPresenter : BaseUIPresenter, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("リザルトシーン固有インスペクタ")]

        // --------------------------------------------------
        // キャンバス
        // --------------------------------------------------
        [Header("キャンバス")]
        /// <summary>リザルト関連の UI を表示するキャンバス</summary>
        [SerializeField]
        private GameObject _resultCanvas;

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        [Header("ボタン")]
        /// <summary>タイトルシーン用の通常ボタン配列</summary>
        [SerializeField]
        private NormalButton[] _resultNormalButtons;

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
        /// <summary>リザルトキャンバス初期選択ボタン</summary>
        [SerializeField]
        private BaseButtonEvent _initialSelectedResultCanvasButton;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI 管理
        // --------------------------------------------------
        /// <summary>リザルトキャンバスのアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _resultCanvasAnimationEventNotifier;

        // --------------------------------------------------
        // システム参照
        // --------------------------------------------------
        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        /// <summary>ScoreManager キャッシュ</summary>
        private ScoreManager _scoreManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>リザルトキャンバスのアニメーター</summary>
        private Animator _resultAnimator;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // アニメーション
        // --------------------------------------------------
        /// <summary>IsStart パラメータ名</summary>
        private static readonly int IS_START_HASH = Animator.StringToHash("IsStart");

        /// <summary>IsSkip パラメータ名</summary>
        private static readonly int IS_SKIP_HASH = Animator.StringToHash("IsSkip");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // インスタンスからコンポーネント取得
            _inputManager = InputManager.Instance;
            _scoreManager = ScoreManager.Instance;

            if (_inputManager == null ||
                _scoreManager == null ||
                _resultCanvas == null ||
                _resultNormalButtons == null ||
                _initialSelectedResultCanvasButton == null)
            {
                Debug.LogError("[TitleUIPresenter] クラスの初期化に失敗しました。");

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
            _uiView = new ResultUIView();
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

            // --------------------------------------------------
            // 通常ボタン初期化
            // --------------------------------------------------
            // 通常ボタンイベント登録
            RegisterNormalButtons(_resultNormalButtons);

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
            _uiStateController = new ResultUIStateController(
                _dialogCanvasArray,
                _initialSelectedResultCanvasButton
            );

            if (_uiStateController is ResultUIStateController resultUIStateController)
            {
                // リザルトキャンバスを表示
                resultUIStateController.ShowResultCanvas();
            }

            // --------------------------------------------------
            // アニメーター初期化
            // --------------------------------------------------
            _resultAnimator = _resultCanvas.GetComponent<Animator>();

            // アニメーター速度をタイムスケール非依存に設定
            SetAnimatorUnscaledTime(_resultAnimator);

            // アニメーションイベント通知クラス取得
            _resultCanvasAnimationEventNotifier = _resultCanvas.GetComponent<AnimationEventNotifier>();

            // ポインター非表示
            SetPointerVisible(false);
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

        protected override void OnExitInternal()
        {
            base.OnExitInternal();
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
            // オプションボタン
            // --------------------------------------------------
            if (clickEvent.UIEvent is OptionButtonEvent optionButton)
            {
                // 左クリックのみ処理
                if (clickEvent.ClickType == UIClickType.Left)
                {
                    OnOptionButtonClick(optionButton);
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
            // UI アクション種別へ変換できない場合は処理なし
            if (!_normalButtonResolver.TryGetType(buttonEvent, out UIActionType actionType))
            {
                return;
            }

            // --------------------------------------------------
            // リザルト終了
            // --------------------------------------------------
            if (actionType == UIActionType.ResultEnd)
            {
                // シーン遷移リクエスト
                _onSceneChangeRequested.OnNext(Unit.Default);

                // シーン遷移中フラグを有効化
                _isSceneTransitioning = true;

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

        // ======================================================
        // 画面フェード派生イベント
        // ======================================================

        /// <summary>
        /// フェードアウト開始時
        /// </summary>
        protected override void OnFadeOutStart()
        {
            // リザルトスタートアニメーション起動
            _resultAnimator.SetTrigger(IS_START_HASH);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        /// <param name="gamepadUsed">ゲームパッド使用状態を通知するストリーム</param>
        /// <param name="resultStartAnimationSkiped">リザルトスタートアニメーションのスキップを通知するストリーム</param>
        public void BindStreams(
            in IObservable<bool> gamepadUsed,
            in IObservable<Unit> resultStartAnimationSkiped)
        {
            gamepadUsed
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
                })
                .AddTo(_disposables);

            resultStartAnimationSkiped
                .Subscribe(_ =>
                {
                    // リザルトスタートスキップアニメーション起動
                    _resultAnimator.SetTrigger(IS_SKIP_HASH);

                    // シーン遷移状態解除
                    _isSceneTransitioning = false;

                    // ポインター表示
                    SetPointerVisible(true);
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
        protected override void Subscribe()
        {
            base.Subscribe();

            // アニメーション終了通知
            _resultCanvasAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ =>
                {
                    // シーン遷移状態解除
                    _isSceneTransitioning = false;

                    // ポインター表示
                    SetPointerVisible(true);
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
        // ボタン
        // --------------------------------------------------
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
                _uiView.SetNormalFocus(normalButton.Button, isFocus);

                return;
            }
        }
    }
}