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
using UnityEngine.UI;
using TMPro;
using UniRx;
using AnimationSystem.Infrastructure;
using InputSystem.Presentation;
using OptionSystem.Domain;
using OptionSystem.Infrastructure;
using OptionSystem.Presentation;
using SoundSystem.Domain;
using UISystem.Application;
using UISystem.Domain;
using UISystem.Infrastructure;
using UpdateSystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// タイトルシーンにおける UI 演出を管理するプレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.TitleUIPresenter)]
    public sealed class TitleUIPresenter : BaseUIPresenter, IUpdatable, IStreamBindable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("タイトルシーン固有インスペクタ")]

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
        /// <summary>スタート関連の UI を表示するキャンバス</summary>
        [SerializeField]
        private GameObject _startCanvas;

        /// <summary>オプション関連の UI を表示するキャンバス</summary>
        [SerializeField]
        private GameObject _optionCanvas;

        // --------------------------------------------------
        // ダイアログ
        // --------------------------------------------------
        [Header("ダイアログ")]
        /// <summary>ダイアログのオプション表示テキスト</summary>
        [SerializeField]
        private TextMeshProUGUI _dialogOptionText;

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        [Header("ボタン")]
        /// <summary>タイトルシーン用の通常ボタン配列</summary>
        [SerializeField]
        private NormalButton[] _titleNormalButtons;

        /// <summary>タイトルシーン用のオプションボタン配列</summary>
        [SerializeField]
        private OptionButtonGroup[] _titleOptionButtons;

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

        [Header("オプションボタンカラー")]
        /// <summary>オプション選択時カラー</summary>
        [SerializeField]
        private Color _optionSelectOnColor = Color.white;

        /// <summary>オプション非選択時カラー</summary>
        [SerializeField]
        private Color _optionSelectOffColor = Color.gray;

        /// <summary>オプションフォーカス時カラー</summary>
        [SerializeField]
        private Color _optionFocusOnColor = Color.white;

        /// <summary>オプション非フォーカス時カラー</summary>
        [SerializeField]
        private Color _optionFocusOffColor = Color.gray;

        // --------------------------------------------------
        // 初期選択ボタン
        // --------------------------------------------------
        [Header("初期選択ボタン")]
        /// <summary>スタートキャンバス初期選択ボタン</summary>
        [SerializeField]
        private BaseButtonEvent _initialSelectedStartCanvasButton;

        /// <summary>オプションキャンバス初期選択ボタン</summary>
        [SerializeField]
        private BaseButtonEvent _initialSelectedOptionCanvasButton;

        // --------------------------------------------------
        // オプション初期選択インデックス
        // --------------------------------------------------
        [Header("オプション初期選択インデックス")]
        /// <summary>オプション種別ごとの選択インデックス管理テーブル</summary>
        [SerializeField]
        private OptionSelectionIndexTable _optionIndexTable;

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        [Header("アニメーター")]
        /// <summary>ボードの GameObject ルートアニメーター</summary>
        [SerializeField]
        private Animator _boardAnimator;

        /// <summary>ゲーム開始演出アニメーター</summary>
        [SerializeField]
        private Animator _startPlayAnimator;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI 管理
        // --------------------------------------------------
        /// <summary>ダイアログ表示文字列生成クラス</summary>
        private readonly DialogTextBuilder _dialogTextBuilder =
            new DialogTextBuilder();

        /// <summary>OptionButtonBinder 生成クラス</summary>
        private OptionButtonBinderFactory _optionButtonBinderFactory;

        /// <summary>ゲーム開始演出のアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _startPlayAnimationEventNotifier;

        /// <summary>スタートキャンバスのアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _startCanvasAnimationEventNotifier;

        // --------------------------------------------------
        // システム参照
        // --------------------------------------------------
        /// <summary>オプション選択状態リポジトリ</summary>
        private readonly PlayerPrefsOptionSelectionRepository _repository
            = new PlayerPrefsOptionSelectionRepository();

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>スタートキャンバスのアニメーター</summary>
        private Animator _startCanvasAnimator;

        /// <summary>タイトル表示開始アニメーションのチェックポイントイベントカウント回数</summary>
        private int _startTitleAnimationCheckPointCount = 0;

        /// <summary>ゲーム開始アニメーションのチェックポイントイベントカウント回数</summary>
        private int _startGameAnimationCheckPointCount = 0;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// オプション UI バインダー辞書
        /// </summary>
        private Dictionary<OptionType, OptionButtonBinder> _optionBinders
            = new Dictionary<OptionType, OptionButtonBinder>();

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // オプション
        // --------------------------------------------------
        /// <summary>3 x 3 ボードサイズ</summary>
        private const int BOARD_SIZE_THREE = 3;

        // --------------------------------------------------
        // アニメーション
        // --------------------------------------------------
        /// <summary>IsStart パラメータ名</summary>
        private static readonly int IS_START_HASH = Animator.StringToHash("IsStart");

        /// <summary>IsSkip パラメータ名</summary>
        private static readonly int IS_SKIP_HASH = Animator.StringToHash("IsSkip");

        /// <summary>IsPlay パラメータ名</summary>
        private static readonly int IS_PLAY_HASH = Animator.StringToHash("IsPlay");

        /// <summary>PlayerCount パラメータ名</summary>
        private static readonly int PLAYER_COUNT_HASH = Animator.StringToHash("PlayerCount");

        /// <summary>BoardSize パラメータ名</summary>
        private static readonly int BOARD_SIZE_HASH = Animator.StringToHash("BoardSize");

        /// <summary>ボードアニメーション無効値</summary>
        private const int BOARD_ANIMATION_DISABLED = -1;

        // ======================================================
        // UniRx 関連
        // ======================================================

        // --------------------------------------------------
        // 購読管理
        // --------------------------------------------------
        /// <summary>イベント購読管理</summary>
        private CompositeDisposable _disposables;

        /// <summary>ゲームパッド使用状態通知ストリーム</summary>
        private IObservable<bool> _gamepadUsedStream;

        /// <summary>ゲーム終了入力ストリーム</summary>
        private IObservable<Unit> _exitGameInputStream;

        /// <summary>タイトルスタートアニメーションスキップ通知ストリーム</summary>
        private IObservable<Unit> _titleStartAnimationSkippedStream;

        // --------------------------------------------------
        // イベント
        // --------------------------------------------------
        /// <summary>タイトルスタートアニメーション終了通知用 Subject</summary>
        private readonly Subject<Unit> _onStartTitleAnimationEnd = new Subject<Unit>();

        /// <summary>タイトルスタートアニメーション終了ストリーム</summary>
        public IObservable<Unit> OnStartTitleAnimationEnd => _onStartTitleAnimationEnd;

        /// <summary>ゲーム開始アニメーション終了通知用 Subject</summary>
        private readonly Subject<Unit> _onStartPlayAnimationEnd = new Subject<Unit>();

        /// <summary>ゲーム開始アニメーション終了ストリーム</summary>
        public IObservable<Unit> OnStartPlayAnimationEnd => _onStartPlayAnimationEnd;

        /// <summary>投影切り替え用 Subject</summary>
        private readonly Subject<bool> _onSwitchProjection = new Subject<bool>();

        /// <summary>ゲームオプション更新通知用 Subject</summary>
        private readonly Subject<OptionButtonData> _onUpdateGameOption = new Subject<OptionButtonData>();

        /// <summary>ゲームオプション更新通知ストリーム</summary>
        public IObservable<OptionButtonData> OnUpdateGameOption => _onUpdateGameOption;

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
                _startCanvas == null ||
                _optionCanvas == null ||
                _titleNormalButtons == null ||
                _titleOptionButtons == null ||
                _initialSelectedStartCanvasButton == null ||
                _initialSelectedOptionCanvasButton == null)
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
            _uiView = new TitleUIView(
                _radialAfterimageRenderer,
                _optionSelectOnColor,
                _optionSelectOffColor,
                _optionFocusOnColor,
                _optionFocusOffColor
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
            RegisterNormalButtons(_titleNormalButtons);

            // --------------------------------------------------
            // UI ボタンの参照解決クラス生成
            // --------------------------------------------------
            _uiActionButtonResolver = new UIActionButtonResolver(_dialogButtonEventTable, _normalButtonEventTable);

            // --------------------------------------------------
            // オプションボタン初期化
            // --------------------------------------------------
            // オプション初期選択テーブル初期化
            _optionIndexTable.Initialize();

            // 初期選択インデックス取得用リーダー
            IOptionSelectionIndexReader reader;

            // 保存データが存在する場合
            if (_repository.HasSavedData())
            {
                reader = _repository;
            }
            else
            {
                // ScriptableObject の初期値を使用するため初期化
                _optionIndexTable.Initialize();

                // 未保存データのみを PlayerPrefs に同期
                foreach (OptionType type in Enum.GetValues(typeof(OptionType)))
                {
                    // 保存済みならスキップ
                    if (_repository.Exists(type))
                    {
                        continue;
                    }

                    // ScriptableObject の初期値取得
                    int index = _optionIndexTable.Get(type);

                    // 保存
                    _repository.Save(type, index);
                }

                // 初期データとして Repository を使用
                reader = _repository;
            }

            // バインダーファクトリ生成
            _optionButtonBinderFactory = new OptionButtonBinderFactory(reader);

            // オプションボタン初期化
            RegisterOptionButtons();

            // --------------------------------------------------
            // パネル初期化
            // --------------------------------------------------
            // パネルイベント登録
            RegisterPanelEvents();

            // --------------------------------------------------
            // キャンバス初期化
            // --------------------------------------------------
            // キャンバス状態管理クラス生成
            _uiStateController = new TitleUIStateController(
                _uiActionButtonResolver,
                _dialogCanvasArray,
                _startCanvas,
                _optionCanvas,
                _initialSelectedStartCanvasButton,
                _initialSelectedOptionCanvasButton
            );

            if (_uiStateController is TitleUIStateController titleUIStateController)
            {
                // スタートキャンバスを表示
                titleUIStateController.ShowStartCanvas();
            }

            // --------------------------------------------------
            // アニメーター初期化
            // --------------------------------------------------
            _startCanvasAnimator = _startCanvas.GetComponent<Animator>();

            // アニメーター速度をタイムスケール非依存に設定
            SetAnimatorUnscaledTime(_boardAnimator);
            SetAnimatorUnscaledTime(_startPlayAnimator);
            SetAnimatorUnscaledTime(_startCanvasAnimator);

            // アニメーションイベント通知クラス取得
            _startPlayAnimationEventNotifier = _startPlayAnimator.GetComponent<AnimationEventNotifier>();
            _startCanvasAnimationEventNotifier = _startCanvas.GetComponent<AnimationEventNotifier>();

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
            if (_uiView is TitleUIView titleUIView)
            {
                titleUIView.UpdateRadialAfterimageEffect(
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

            // イベント購読解除
            UnbindStreams();
            
            // BGM 停止
            StopBgm();

            // SE 停止
            _soundManager?.StopLoopSE(SeType.Effect_Title_Rise);
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
                })
                .AddTo(_disposables);

            _exitGameInputStream
                .Subscribe(_ =>
                {
                    // UI イベント購読後かつスタートキャンバスの場合
                    if (_uiEventDisposables != null && _uiStateController.GetActiveCanvasType() == CanvasType.Start)
                    {
                        OnStartCanvasCancelInput();
                    }
                })
                .AddTo(_disposables);

            _titleStartAnimationSkippedStream
                .Subscribe(_ =>
                {
                    // フェードアウト未完了の場合処理なし
                    if (!_onFadeOutEnd)
                    {
                        return;
                    }

                    // タイトルスタートスキップアニメーション起動
                    _startCanvasAnimator?.SetTrigger(IS_SKIP_HASH);

                    OnStartTitleAnimationFinish();
                })
                .AddTo(_disposables);
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

                case CanvasType.Start:
                    OnStartCanvasCancelInput();
                    break;

                case CanvasType.Option:
                    OnOptionCanvasCancelInput();
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

            switch (actionType)
            {
                // --------------------------------------------------
                // ダイアログ：YES
                // --------------------------------------------------
                case UIActionType.DialogYes:
                    // SE 再生
                    _soundManager?.PlaySE(SeType.UI_Decide);
                    
                    switch (dialogType)
                    {
                        // --------------------------------------------------
                        // ゲーム開始
                        // --------------------------------------------------
                        case DialogType.StartGame:
                            // ダイアログイベント実行
                            _onDialogEvent.OnNext(dialogType);

                            // ダイアログキャンバスを非表示にする
                            _uiStateController.HideDialogCanvas();

                            // ダイアログ非表示を通知する
                            _onDialogVisibleChanged.OnNext(false);

                            // ゲーム開始アニメーションを起動
                            _effectAnimator?.SetTrigger(IS_PLAY_HASH);
                            _startCanvasAnimator?.SetTrigger(IS_PLAY_HASH);
                            _startPlayAnimator?.SetInteger(PLAYER_COUNT_HASH, _gameOptionManager.PlayerCount);

                            return;

                        // --------------------------------------------------
                        // ゲーム終了
                        // --------------------------------------------------
                        case DialogType.ExitGame:
                            // ダイアログイベント実行
                            _onDialogEvent.OnNext(dialogType);

                            return;
                    }

                    break;

                // --------------------------------------------------
                // ダイアログ：NO
                // --------------------------------------------------
                case UIActionType.DialogNo:
                    OnDialogCanvasCancelInput();
                    break;
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
            // タイトルスタート
            // --------------------------------------------------
            if (actionType == UIActionType.TitleStart)
            {
                // SE 再生
                _soundManager?.PlaySE(SeType.UI_ShowDialog, 0.5f);

                // スタート画面のボタンを操作不可に更新
                SetButtonInteractable(_uiActionButtonResolver.GetNormalButton(UIActionType.TitleStart), false);
                SetButtonInteractable(_uiActionButtonResolver.GetNormalButton(UIActionType.TitleOption), false);

                // ダイアログ表示内容を更新
                UpdateDialogOptionText();

                // ダイアログキャンバスを表示する
                _uiStateController.ShowDialogCanvas(DialogType.StartGame);

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

                // 入力状態に応じて初期選択を適用する
                SetSelectionState(nextCanvasType);

                // ダイアログ表示を通知する
                _onDialogVisibleChanged.OnNext(true);

                return;
            }

            // --------------------------------------------------
            // タイトルオプション
            // --------------------------------------------------
            if (actionType == UIActionType.TitleOption)
            {
                // SE 再生
                _soundManager?.PlaySE(SeType.UI_Click);

                if (_uiStateController is TitleUIStateController titleUIStateController)
                {
                    // オプションキャンバスを表示する
                    titleUIStateController.ShowOptionCanvas();
                }

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

                // 最後に選択されていたボタンを取得する
                BaseButtonEvent selectedButtonEvent =
                    _uiStateController.GetLastSelectedButtonEvent(nextCanvasType);

                // 直前の選択が OptionButton の場合はそれを復元する
                if (selectedButtonEvent is OptionButtonEvent optionButton)
                {
                    SetSelectionState(nextCanvasType, optionButton);
                }
                else
                {
                    // 入力状態に応じて初期選択を適用する
                    SetSelectionState(nextCanvasType);
                }

                // 現在のボードサイズを取得する
                int boardSize = _gameOptionManager.BoardSize;

                // ConnectCount 同期
                ApplyBoardSizeSynchronizeConnectCount(boardSize);

                // 各オプション種別ごとに状態を復元する
                foreach (KeyValuePair<OptionType, OptionButtonBinder> binder in _optionBinders)
                {
                    // 現在保存されている選択インデックスを取得する
                    int index = _repository.Get(binder.Key);

                    // バインダー内部の選択状態を更新する
                    binder.Value.SelectByIndex(index);

                    if (_uiView is TitleUIView titleUIView)
                    {
                        // 選択状態をビューへ反映
                        titleUIView.ApplyButtonSelectionState(
                            binder.Value.Buttons,
                            binder.Value.SelectStateArray);
                    }
                }

                // ボード変更アニメーションを実行する
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, boardSize);

                return;
            }

            // --------------------------------------------------
            // オプションキャンセル
            // --------------------------------------------------
            if (actionType == UIActionType.OptionCancel)
            {
                OnOptionCanvasCancelInput();
                return;
            }

            // --------------------------------------------------
            // オプション確定
            // --------------------------------------------------
            if (actionType == UIActionType.OptionDecide)
            {
                // SE 再生
                _soundManager?.PlaySE(SeType.UI_Decide);

                if (_uiStateController is TitleUIStateController titleUIStateController)
                {
                    // スタートキャンバスを表示する
                    titleUIStateController.ShowStartCanvas();
                }

                // 次のキャンバス状態を取得する
                CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

                // 初期フォーカスをスタートボタンに設定する
                SetSelectionState(nextCanvasType, _uiActionButtonResolver.GetNormalButton(UIActionType.TitleStart));

                // 各オプションの選択状態を保存する
                foreach (KeyValuePair<OptionType, OptionButtonBinder> binder in _optionBinders)
                {
                    // 現在選択されているインデックスを取得する
                    int currentIndex = binder.Value.GetCurrentSelectedIndex();

                    // 永続データへ保存する
                    _repository.Save(binder.Key, currentIndex);
                }

                // ボードアニメーションをリセットする
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, BOARD_ANIMATION_DISABLED);
            }
        }

        /// <summary>
        /// オプションボタンクリック時
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        protected override void OnOptionButtonClick(OptionButtonEvent buttonEvent)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // SE 再生
            _soundManager?.PlaySE(SeType.UI_Click);

            // 種別取得
            OptionType type = buttonEvent.Data.Type;

            // バインダー取得
            if (!_optionBinders.TryGetValue(type, out OptionButtonBinder binder))
            {
                return;
            }

            // ボタン選択処理
            binder.SelectByButton(buttonEvent.Button);

            if (_uiView is TitleUIView titleUIView)
            {
                // 選択状態をビューへ反映
                titleUIView.ApplyButtonSelectionState(
                    binder.Buttons,
                    binder.SelectStateArray);
            }

            // オプション更新通知
            _onUpdateGameOption.OnNext(buttonEvent.Data);

            // ボードサイズが変更された場合
            if (type == OptionType.BoardSize)
            {
                int boardSize = (int)buttonEvent.Data.BoardSizeType;

                // ConnectCount 同期
                ApplyBoardSizeSynchronizeConnectCount(boardSize);

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, boardSize);
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
            // タイトルスタートアニメーション起動
            _startCanvasAnimator?.SetTrigger(IS_START_HASH);
        }

        // ======================================================
        // サウンド派生イベント
        // ======================================================

        /// <summary>
        /// BGM 再生開始時
        /// </summary>
        protected override void StartBgm()
        {
            _soundManager?.SetBGMVolume(BgmType.Title, 0.2f, 0);
            _soundManager?.PlayBGM(BgmType.Title, 0);
        }

        /// <summary>
        /// BGM 再生停止時
        /// </summary>
        protected override void StopBgm()
        {
            _soundManager?.StopBGM(BgmType.Title);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        /// <param name="gamepadUsed">ゲームパッド使用状態を通知するストリーム</param>
        /// <param name="exitGameInput">ゲーム終了入力を通知するストリーム</param>
        /// <param name="titleStartAnimationSkipped">タイトルスタートアニメーションのスキップを通知するストリーム</param>
        public void SetStreams(
            in IObservable<bool> gamepadUsed,
            in IObservable<Unit> exitGameInput,
            in IObservable<Unit> titleStartAnimationSkipped)
        {
            _gamepadUsedStream = gamepadUsed;
            _exitGameInputStream = exitGameInput;
            _titleStartAnimationSkippedStream = titleStartAnimationSkipped;
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

            // タイトル表示開始アニメーションチェックポイント通知
            _startCanvasAnimationEventNotifier.OnAnimationCheckPoint
                .Subscribe(_ =>
                {
                    _startTitleAnimationCheckPointCount++;

                    switch (_startTitleAnimationCheckPointCount)
                    {
                        case 1:
                            // SE 再生
                            _soundManager?.PlaySE(SeType.Effect_Title_Fall);
                            break;

                        case 2:
                            // SE 再生
                            _soundManager?.PlaySE(SeType.Effect_Impact_Medium, 0.5f);
                            break;

                        case 3:
                            // SE 再生
                            _soundManager?.PlaySE(SeType.Effect_Impact_Medium, 0.5f);
                            break;
                    }
                })
                .AddTo(_disposables);

            // タイトル表示開始アニメーション終了通知
            _startCanvasAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ =>
                {
                    OnStartTitleAnimationFinish();

                    // アニメーション終了通知
                    _onStartTitleAnimationEnd.OnNext(Unit.Default);
                })
                .AddTo(_disposables);

            // ゲーム開始アニメーションチェックポイント通知
            _startPlayAnimationEventNotifier.OnAnimationCheckPoint
                .Subscribe(_ =>
                {
                    _startGameAnimationCheckPointCount++;

                    // プレイヤー表示タイミングのチェックポイント回数
                    int playerDisplayCheckPointCount = _gameOptionManager.PlayerCount;
                    // VS 表示タイミングのチェックポイント回数
                    int vsDisplayCheckPointCount = playerDisplayCheckPointCount + 1;
                    // 画面エフェクト更新タイミングのチェックポイント回数
                    int screenEffectUpdateCheckPointCount = playerDisplayCheckPointCount + 2;
                    // 上昇 SE 停止タイミングのチェックポイント回数
                    int stopRiseSeCheckPointCount = playerDisplayCheckPointCount + 3;

                    if (_startGameAnimationCheckPointCount <= playerDisplayCheckPointCount)
                    {
                        // SE 再生
                        _soundManager?.PlaySE(SeType.Effect_Impact_Small);

                        return;
                    }
                    if (_startGameAnimationCheckPointCount == vsDisplayCheckPointCount)
                    {
                        // SE 再生
                        _soundManager?.PlaySE(SeType.Effect_Title_Rise, 0.75f);

                        return;
                    }
                    if (_startGameAnimationCheckPointCount == screenEffectUpdateCheckPointCount)
                    {
                        // SE 再生
                        _soundManager?.PlaySE(SeType.Effect_Title_PlayerCutIn, 0.75f);

                        return;
                    }
                    if (_startGameAnimationCheckPointCount == stopRiseSeCheckPointCount)
                    {
                        // SE 停止
                        _soundManager?.StopLoopSE(SeType.Effect_Title_Rise);

                        return;
                    }
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// UI イベント購読
        /// </summary>
        protected override void SubscribeUiEvents()
        {
            base.SubscribeUiEvents();

            // ゲーム開始アニメーション終了通知
            _startPlayAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ =>
                {
                    // シーン遷移実行
                    _onStartPlayAnimationEnd.OnNext(Unit.Default);
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
        /// スタートキャンバス表示中のキャンセル入力処理
        /// </summary>
        private void OnStartCanvasCancelInput()
        {
            // SE 再生
            _soundManager?.PlaySE(SeType.UI_ShowDialog, 0.5f);

            // ダイアログキャンバスを表示する
            _uiStateController.ShowDialogCanvas(DialogType.ExitGame);

            // 次のキャンバス状態を取得する
            CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

            // 入力状態に応じて初期選択を適用する
            SetSelectionState(nextCanvasType);

            // ダイアログ表示を通知する
            _onDialogVisibleChanged.OnNext(true);
        }

        /// <summary>
        /// オプションキャンバス表示中のキャンセル入力処理
        /// </summary>
        private void OnOptionCanvasCancelInput()
        {
            // SE 再生
            _soundManager?.PlaySE(SeType.UI_Cancel);

            if (_uiStateController is TitleUIStateController titleUIStateController)
            {
                // スタートキャンバスを表示する
                titleUIStateController.ShowStartCanvas();
            }

            // 次のキャンバス状態を取得する
            CanvasType nextCanvasType = _uiStateController.GetActiveCanvasType();

            // 初期フォーカスをオプションボタンに設定する
            SetSelectionState(
                nextCanvasType,
                _uiActionButtonResolver.GetNormalButton(UIActionType.TitleOption));

            // ボードアニメーションをリセットする
            _boardAnimator?.SetInteger(
                BOARD_SIZE_HASH,
                BOARD_ANIMATION_DISABLED);
        }

        /// <summary>
        /// ダイアログオプション表示テキスト更新
        /// </summary>
        private void UpdateDialogOptionText()
        {
            // プレイヤー人数取得
            int playerCount = _gameOptionManager.PlayerCount;

            // ボードサイズ取得
            int boardSize = _gameOptionManager.BoardSize;

            // 連結数取得
            int connectCount = _gameOptionManager.ConnectCount;

            // 表示文字列生成
            string text = _dialogTextBuilder.OptionTextBuild(
                playerCount,
                boardSize,
                connectCount);

            // テキスト反映
            _dialogOptionText.text = text;
        }

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// GridLayoutGroup 配下のオプションボタンのイベント登録する
        /// </summary>
        private void RegisterOptionButtons()
        {
            // 辞書生成
            _optionBinders = _buttonDictionaryBuilder.BuildOptionButtons(_optionButtonBinderFactory, _titleOptionButtons);

            // イベント登録
            foreach (OptionButtonBinder binder in _optionBinders.Values)
            {
                _eventRouter.RegisterOptionButtons(binder.Events);
            }
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
                _uiView.SetNormalFocus(normalButton.Button, isFocus);

                return;
            }

            // オプションボタンイベント
            if (buttonEvent is OptionButtonEvent optionButton)
            {
                if (_uiView is TitleUIView titleUIView)
                {
                    // オプションボタンのフォーカス状態を有効化
                    titleUIView.SetOptionFocus(optionButton.Button, isFocus);
                }
            }
        }

        /// <summary>
        /// ボードサイズに応じて ConnectCount の表示状態と初期選択を同期する
        /// </summary>
        private void ApplyBoardSizeSynchronizeConnectCount(in int boardSize)
        {
            // ConnectCount バインダー取得
            if (!_optionBinders.TryGetValue(OptionType.ConnectCount, out OptionButtonBinder connectCountBinder))
            {
                return;
            }

            // --------------------------------------------------
            // 3 x 3 判定
            // --------------------------------------------------
            bool isThreeSize = (boardSize == BOARD_SIZE_THREE);

            // 3 x 3 の場合は強制的に先頭を選択状態にする
            if (isThreeSize)
            {
                // インデックス 0 を選択
                connectCountBinder.SelectByIndex(0);

                if (_uiView is TitleUIView titleUIView)
                {
                    // ビューへ選択状態を反映
                    titleUIView.ApplyButtonSelectionState(
                        connectCountBinder.Buttons,
                        connectCountBinder.SelectStateArray);
                }
            }

            for (int i = 1; i < connectCountBinder.Buttons.Length; i++)
            {
                // インデックス 1 以降のボタンオブジェクト非表示
                connectCountBinder.Events[i].gameObject.SetActive(!isThreeSize);
            }
        }

        // --------------------------------------------------
        // アニメーション
        // --------------------------------------------------
        /// <summary>
        /// タイトル表示開始アニメーション終了時の処理
        /// </summary>
        private void OnStartTitleAnimationFinish()
        {
            // UI イベント購読
            SubscribeUiEvents();

            // ポインター表示
            SetPointerVisible(true);

            // BGM 再生
            StartBgm();

            // SE 再生
            _soundManager?.PlaySE(SeType.Effect_Impact_Large, 0.75f);

            // SE 停止
            _soundManager?.StopLoopSE(SeType.Effect_Title_Fall);
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