// ======================================================
// ResultUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-29
// 更新日時 : 2026-05-29
// 概要     : リザルトシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using AnimationSystem.Infrastructure;
using InputSystem.Presentation;
using OptionSystem.Presentation;
using ScoreSystem.Domain;
using ScoreSystem.Presentation;
using SoundSystem.Domain;
using System;
using System.Collections.Generic;
using TMPro;
using UISystem.Application;
using UISystem.Domain;
using UISystem.Infrastructure;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
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
        // エフェクト
        // --------------------------------------------------
        [Header("エフェクト")]
        /// <summary>画面中心からの残像エフェクト用レンダラー</summary>
        [SerializeField]
        private RawImage _radialAfterimageRenderer;

        /// <summary>エフェクト全体の強さ</summary>
        [SerializeField, Range(0.0f, 1.0f)]
        private float _radialAfterimageEffectStrength = 1.0f;

        /// <summary>表示する残像数</summary>
        [SerializeField, Range(1, 8)]
        private int _radialAfterimageSampleCount = 3;

        /// <summary>残像の拡大率間隔</summary>
        [SerializeField, Range(0.01f, 1.0f)]
        private float _radialAfterimageScaleStep = 0.15f;

        /// <summary>残像が外側へ流れる速度</summary>
        [SerializeField, Range(0.0f, 10.0f)]
        private float _radialAfterimageScaleSpeed = 1.0f;

        /// <summary>透明度が 0 になる距離</summary>
        [SerializeField, Range(0.01f, 10.0f)]
        private float _radialAfterimageAlphaFadeDistance = 1.0f;
        
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

        // --------------------------------------------------
        // 駒 GameObject
        // --------------------------------------------------
        [Header("駒 GameObject")]
        /// <summary>駒の GameObject 配列</summary>
        [SerializeField]
        private GameObject[] _pieceObjectArray;

        // --------------------------------------------------
        // 駒マテリアル
        // --------------------------------------------------
        [Header("駒マテリアル")]
        /// <summary>駒のマテリアル配列</summary>
        [SerializeField]
        private Material[] _pieceMaterialArray;

        // --------------------------------------------------
        // プレイヤーカラー
        // --------------------------------------------------
        [Header("プレイヤーカラー")]
        /// <summary>プレイヤーのカラー配列</summary>
        [SerializeField]
        private Color[] _playerColorArray;

        // --------------------------------------------------
        // ランキング情報
        // --------------------------------------------------
        [Header("ランキング情報")]
        /// <summary>1 位のプレイヤー ID 表示用テキスト</summary>
        [SerializeField]
        private TextMeshPro _firstRankingPlayerIdText;

        /// <summary>ランキングのプレイヤー ID 表示用テキスト</summary>
        [SerializeField]
        private TextMeshProUGUI[] _rankingPlayerIdTexts;

        /// <summary>ランキングのスコア表示用テキスト</summary>
        [SerializeField]
        private TextMeshProUGUI[] _rankingScoreTexts;

        // --------------------------------------------------
        // リザルト背景 Renderer
        // --------------------------------------------------
        [Header("リザルト背景 Renderer")]
        /// <summary>リザルト背景用の Renderer</summary>
        [SerializeField]
        private Renderer _resultBackgroundRenderer;

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        [Header("アニメーター")]
        /// <summary>リザルト順位表示アニメーター</summary>
        [SerializeField]
        private Animator _resultRankAnimator;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI 管理
        // --------------------------------------------------
        /// <summary>リザルト順位表示のアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _resultRankAnimationEventNotifier;

        /// <summary>リザルトキャンバスのアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _resultCanvasAnimationEventNotifier;

        // --------------------------------------------------
        // システム
        // --------------------------------------------------
        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        /// <summary>ScoreManager キャッシュ</summary>
        private ScoreManager _scoreManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>リザルトキャンバスのアニメーター</summary>
        private Animator _resultCanvasAnimator;

        /// <summary>ランクアニメーションのチェックポイントイベントカウント回数</summary>
        private int _rankAnimationCheckPointCount = 0;

        /// <summary>ランクアニメーションの終了イベントカウント回数</summary>
        private int _rankAnimationEndCount = 0;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // アニメーション
        // --------------------------------------------------
        /// <summary>IsStart パラメータ名</summary>
        private static readonly int IS_START_HASH = Animator.StringToHash("IsStart");

        /// <summary>IsFlash パラメータ名</summary>
        private static readonly int IS_FLASH_HASH = Animator.StringToHash("IsFlash");

        /// <summary>IsEnd パラメータ名</summary>
        private static readonly int IS_END_HASH = Animator.StringToHash("IsEnd");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>リザルトスタートアニメーション終了通知用 Subject</summary>
        private readonly Subject<Unit> _onStarttResultAnimationEnd = new Subject<Unit>();

        /// <summary>リザルトスタートアニメーション終了ストリーム</summary>
        public IObservable<Unit> OnStartResultAnimationEnd => _onStarttResultAnimationEnd;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;
            _scoreManager = ScoreManager.Instance;

            if (_gameOptionManager == null||
                _inputManager == null ||
                _scoreManager == null ||
                _resultCanvas == null ||
                _resultNormalButtons == null ||
                _initialSelectedResultCanvasButton == null ||
                _resultRankAnimator == null)
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
            _uiView = new ResultUIView(
                _radialAfterimageRenderer,
                _pieceObjectArray,
                _pieceMaterialArray,
                _playerColorArray,
                _firstRankingPlayerIdText,
                _rankingPlayerIdTexts,
                _rankingScoreTexts,
                _resultBackgroundRenderer
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

            // --------------------------------------------------
            // ダイアログボタン初期化
            // --------------------------------------------------
            RegisterDialogButtons();

            // --------------------------------------------------
            // 通常ボタン初期化
            // --------------------------------------------------
            // 通常ボタンイベント登録
            RegisterNormalButtons(_resultNormalButtons);

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
            _uiStateController = new ResultUIStateController(
                _uiActionButtonResolver,
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
            _resultCanvasAnimator = _resultCanvas.GetComponent<Animator>();

            // アニメーター速度をタイムスケール非依存に設定
            SetAnimatorUnscaledTime(_resultRankAnimator);
            SetAnimatorUnscaledTime(_resultCanvasAnimator);

            // アニメーションイベント通知クラス取得
            _resultRankAnimationEventNotifier = _resultRankAnimator.gameObject.GetComponent<AnimationEventNotifier>();
            _resultCanvasAnimationEventNotifier = _resultCanvas.GetComponent<AnimationEventNotifier>();

            // ポインター非表示
            SetPointerVisible(false);

            // --------------------------------------------------
            // スコア取得
            // --------------------------------------------------
            List<RankingData> ranking = _scoreManager.GetRanking();

            if (_uiView is ResultUIView resultUIView)
            {
                resultUIView.UpdateRankingInfo(ranking);
            }
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (_isPointerLock)
            {
                return;
            }

            // --------------------------------------------------
            // ポインター更新
            // --------------------------------------------------
            // ポインター取得
            Vector2 screenPos = _inputManager.Pointer;

            // ビューへ反映
            _uiView.UpdatePointer(screenPos);

            // --------------------------------------------------
            // エフェクト更新
            // --------------------------------------------------
            if (_uiView is ResultUIView resultUIView)
            {
                resultUIView.UpdateRadialAfterimageEffect(
                    _radialAfterimageEffectStrength,
                    _radialAfterimageSampleCount,
                    _radialAfterimageScaleStep,
                    _radialAfterimageScaleSpeed,
                    _radialAfterimageAlphaFadeDistance
                );
            }
        }

        protected override void OnExitInternal()
        {
            base.OnExitInternal();

            // BGM 停止
            StopBgm();
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
            // UI アクション種別へ変換できない場合は処理なし
            if (!_uiActionButtonResolver.TryGetNormalType(buttonEvent, out UIActionType actionType))
            {
                return;
            }

            // --------------------------------------------------
            // リザルト終了
            // --------------------------------------------------
            if (actionType == UIActionType.ResultEnd)
            {
                // SE 再生
                _soundManager?.PlaySE(SeType.UI_Decide);

                // シーン遷移リクエスト
                _onSceneChangeRequested.OnNext(Unit.Default);

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
            // BGM 再生
            StartBgm();
        }

        /// <summary>
        /// フェードアウト終了時
        /// </summary>
        protected override void OnFadeOutFinish()
        {
            // リザルト順位表示アニメーション開始
            _resultRankAnimator?.SetTrigger(IS_START_HASH);
            _resultCanvasAnimator?.SetTrigger(IS_START_HASH);

            // BGM フェード
            _soundManager?.SetBGMVolume(BgmType.Result, 0.2f, 5);

            // SE 再生
            _soundManager?.PlaySE(SeType.Effect_Title_Fall);
        }

        // ======================================================
        // サウンド派生イベント
        // ======================================================

        /// <summary>
        /// BGM 再生開始時
        /// </summary>
        protected override void StartBgm()
        {
            _soundManager?.SetBGMVolume(BgmType.Result, 0.1f, 0);
            _soundManager?.PlayBGM(BgmType.Result, 0);
        }

        /// <summary>
        /// BGM 再生停止時
        /// </summary>
        protected override void StopBgm()
        {
            _soundManager?.StopBGM(BgmType.Result);
        }

        /// <summary>
        /// BGM 再生位置更新時
        /// </summary>
        /// <param name="block">対象再生ブロック</param>
        protected override void SetPlaybackPosition(in int block)
        {
            _soundManager?.SetPlaybackPosition(BgmType.Result, block);
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
                    // フェードアウト未完了の場合処理なし
                    if (!_onFadeOutEnd)
                    {
                        return;
                    }

                    OnRankAnimationFinish();

                    // BGM 音量更新
                    _soundManager?.SetBGMVolume(BgmType.Result, 0.2f, 0.25f);

                    // BGM 再生位置更新
                    SetPlaybackPosition(1);
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

            // 順位アニメーションチェックポイント通知
            _resultRankAnimationEventNotifier.OnAnimationCheckPoint
                .Subscribe(_ => OnRankAnimationCheckPoint())
                .AddTo(_disposables);

            // 順位アニメーション終了通知
            _resultRankAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ => OnRankAnimationFinish())
                .AddTo(_disposables);
            
            // キャンバスアニメーション終了通知
            _resultCanvasAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ =>
                {
                    // UI イベント購読
                    SubscribeUiEvents();

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

        // --------------------------------------------------
        // アニメーション
        // --------------------------------------------------
        /// <summary>
        /// 順位アニメーションのチェックポイント通過時の処理
        /// </summary>
        private void OnRankAnimationCheckPoint()
        {
            _rankAnimationCheckPointCount++;

            switch (_rankAnimationCheckPointCount)
            {
                case 1:
                    // SE 停止
                    _soundManager?.StopLoopSE(SeType.Effect_Title_Fall);

                    // SE 再生
                    _soundManager?.PlaySE(SeType.Effect_Result_Rise, 0.75f);
                    break;

                case 2:
                    if (_gameOptionManager.PlayerCount >= 4)
                    {
                        // SE 再生
                        _soundManager?.PlaySE(SeType.Effect_Result_4th, 0.6f);
                    }
                        
                    break;

                case 3:
                    if (_gameOptionManager.PlayerCount >= 3)
                    {
                        // SE 再生
                        _soundManager?.PlaySE(SeType.Effect_Result_3rd, 0.8f);
                    }

                    break;

                case 4:
                    if (_gameOptionManager.PlayerCount >= 2)
                    {
                        // SE 再生
                        _soundManager?.PlaySE(SeType.Effect_Result_2nd);
                    }

                    break;

                case 5:
                    // フラッシュアニメーション起動
                    _resultCanvasAnimator?.SetTrigger(IS_FLASH_HASH);
                    break;
            }
        }

        /// <summary>
        /// 順位アニメーション終了時の処理
        /// </summary>
        private void OnRankAnimationFinish()
        {
            _rankAnimationEndCount++;

            // 順位アニメーション終了
            _effectAnimator?.SetTrigger(IS_END_HASH);
            _resultRankAnimator?.SetTrigger(IS_END_HASH);
            _resultCanvasAnimator?.SetTrigger(IS_END_HASH);

            // SE 停止
            _soundManager?.StopLoopSE(SeType.Effect_Result_Rise);
            _soundManager?.StopLoopSE(SeType.Effect_Title_Fall);

            switch (_rankAnimationEndCount)
            {
                case 1:
                    // SE 再生
                    _soundManager?.PlaySE(SeType.Effect_Result_1st);
                    break;

                case 2:
                    // アニメーション終了通知
                    _onStarttResultAnimationEnd.OnNext(Unit.Default);
                    break;
            }
        }
    }
}