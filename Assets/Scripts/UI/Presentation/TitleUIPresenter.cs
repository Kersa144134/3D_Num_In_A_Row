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
using UnityEngine.EventSystems;
using UniRx;
using InputSystem.Presentation;
using OptionSystem.Domain;
using OptionSystem.Presentation;
using UISystem.Application;
using UISystem.Infrastructure;
using UpdateSystem.Domain;
using UISystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// タイトルシーンにおける UI 演出を管理するプレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.TitleUIPresenter)]
    public sealed class TitleUIPresenter : BaseUIPresenter, IUpdatable
    {
        // ======================================================
        // 列挙型
        // ======================================================

        /// <summary>
        /// 現在アクティブな UI キャンバス種別
        /// </summary>
        private enum ActiveCanvasType
        {
            /// <summary>未選択</summary>
            None,

            /// <summary>スタートキャンバス</summary>
            Start,

            /// <summary>オプションキャンバス</summary>
            Option,

            /// <summary>ダイアログキャンバス</summary>
            Dialogue
        }
        
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

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI 管理
        // --------------------------------------------------
        /// <summary>ビュー</summary>
        private TitleUIView _titleUIView;

        /// <summary>イベントを仲介するクラス</summary>
        private readonly UIEventRouter _eventRouter = new UIEventRouter();

        /// <summary>通常ボタンの参照解決クラス</summary>
        private NormalButtonResolver _normalButtonResolver;

        /// <summary>OptionButtonBinder 生成クラス</summary>
        private OptionButtonBinderFactory _optionButtonBinderFactory;

        /// <summary>タイトル UI のキャンバス状態と初期ボタン選択状態を管理するクラス</summary>
        private TitleUIStateController _titleUIStateController;

        // --------------------------------------------------
        // ボタンイベント
        // --------------------------------------------------
        /// <summary>最後に選択したボタンイベント</summary>
        private BaseButtonEvent _lastSelectedButtonEvent;

        /// <summary>最後にホバー選択中のボタンイベント</summary>
        private BaseButtonEvent _lastHoveredButtonEvent;

        // --------------------------------------------------
        // システム参照
        // --------------------------------------------------
        /// <summary>EventSystem キャッシュ</summary>
        private EventSystem _eventSystem;

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

        /// <summary>ゲームパッド入力状態フラグ</summary>
        private bool _isGamePadInput = false;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 通常ボタンイベント辞書
        /// </summary>
        private Dictionary<UIActionType, NormalButtonEvent> _normalButtonEventTable
            = new Dictionary<UIActionType, NormalButtonEvent>();

        /// <summary>
        /// オプション UI バインダー辞書
        /// </summary>
        private Dictionary<OptionType, OptionButtonBinder> _optionBinders
            = new Dictionary<OptionType, OptionButtonBinder>();

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>BoardSize パラメータ名</summary>
        private static readonly int BOARD_SIZE_HASH = Animator.StringToHash("BoardSize");

        /// <summary>IsTarget パラメータ名</summary>
        private static readonly int IS_TARGET_HASH = Animator.StringToHash("IsTarget");

        /// <summary>3 x 3 ボードサイズ</summary>
        private const int BOARD_SIZE_THREE = 3;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>ルーター用購読管理</summary>
        private readonly CompositeDisposable _routerDisposables = new CompositeDisposable();

        /// <summary>投影切り替え用 Subject</summary>
        private readonly Subject<bool> _onSwitchProjection = new Subject<bool>();

        /// <summary>フォーカス座標通知用 Subject</summary>
        private readonly Subject<Vector2> _onFocusPosition =  new Subject<Vector2>();

        /// <summary>フォーカス座標通知ストリーム</summary>
        public IObservable<Vector2> OnFocusPosition => _onFocusPosition;

        /// <summary>ゲームオプション更新通知用 Subject</summary>
        private readonly Subject<OptionButtonData> _onUpdateGameOption = new Subject<OptionButtonData>();

        /// <summary>ゲームオプション更新通知ストリーム</summary>
        public IObservable<OptionButtonData> OnUpdateGameOption => _onUpdateGameOption;

        /// <summary>入力ロック状態購読</summary>
        private IDisposable _inputLockSubscription;

        /// <summary>ゲームパッド入力状態購読</summary>
        private IDisposable _gamePadInputSubscription;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // インスタンスからコンポーネント取得
            _eventSystem = EventSystem.current;
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;

            if (_eventSystem == null ||
                _gameOptionManager == null ||
                _inputManager == null ||
                _pointer == null ||
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

            // ビュー生成
            _titleUIView = new TitleUIView(
                _pointer,
                _normalFocusOnColor,
                _normalFocusOffColor,
                _optionSelectOnColor,
                _optionSelectOffColor,
                _optionFocusOnColor,
                _optionFocusOffColor
            );

            // 通常ボタン初期化
            InitializeNormalButtons();

            // 通常ボタンの参照解決クラス生成
            _normalButtonResolver = new NormalButtonResolver(_normalButtonEventTable);

            // オプション初期選択テーブル初期化
            _optionIndexTable.Initialize();

            // オプションバインダーファクトリ生成
            _optionButtonBinderFactory = new OptionButtonBinderFactory(_optionIndexTable);

            // オプションボタン初期化
            InitializeOptionButtons();

            // キャンバス状態管理クラス生成
            _titleUIStateController = new TitleUIStateController(
                _startCanvas,
                _optionCanvas,
                _dialogueCanvas,
                _initialSelectedStartCanvasButton,
                _initialSelectedOptionCanvasButton,
                _normalButtonResolver.Get(UIActionType.DialogueYes)
            );

            // スタートキャンバスを表示
            _titleUIStateController.ShowStartCanvas();

            // イベント購読
            Subscribe();
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

        protected override void OnExitInternal()
        {
            base.OnExitInternal();

            // イベント購読解除
            Dispose();
            _disposables?.Dispose();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        /// <param name="inputLock">入力ロック状態ストリーム</param>
        /// <param name="gamePadInput">ゲームパッド入力状態ストリーム</param>
        public void BindStreams(
            in IObservable<bool> inputLock,
            in IObservable<bool> gamePadInput)
        {
            inputLock
                .Subscribe(isLock =>
                {
                    _isInputLock = isLock;
                })
                .AddTo(_disposables);


            gamePadInput
                .DistinctUntilChanged()
                .Subscribe(isGamePadInput =>
                {
                    _isGamePadInput = isGamePadInput;

                    // --------------------------------------------------
                    // ゲームパッド入力時
                    // --------------------------------------------------
                    if (isGamePadInput)
                    {
                        // 最後に入力した選択状態を復元
                        SetSelectionState(_lastSelectedButtonEvent);

                        return;
                    }

                    // --------------------------------------------------
                    // マウス入力時
                    // --------------------------------------------------
                    if (_lastHoveredButtonEvent != null)
                    {
                        // ホバー中のボタンを選択状態にする
                        OnSelectButton(_lastHoveredButtonEvent);
                        return;
                    }

                    // 選択解除
                    OnUnSelectButton();

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
        private void Subscribe()
        {
            // 通常ボタンクリック
            _eventRouter.OnNormalButtonClick
                .Subscribe(buttonEvent => OnNormalButtonClick(buttonEvent))
                .AddTo(_routerDisposables);

            // オプションボタンクリック
            _eventRouter.OnOptionButtonClick
                .Subscribe(buttonEvent => OnOptionButtonClick(buttonEvent))
                .AddTo(_routerDisposables);

            // ホバー通知
            _eventRouter.OnHover
                .Subscribe(buttonEvent =>
                {
                    _lastHoveredButtonEvent = buttonEvent;

                    OnSelectButton(buttonEvent);
                })
                .AddTo(_routerDisposables);

            // ホバー解除通知
            _eventRouter.OnUnHover
                .Subscribe(buttonEvent =>
                {
                    _lastHoveredButtonEvent = null;

                    // ホバー対象を選択
                    OnUnSelectButton();
                })
                .AddTo(_routerDisposables);

            // フォーカス通知
            _eventRouter.OnFocus
                .Subscribe(buttonEvent => OnFocusButton(buttonEvent))
                .AddTo(_routerDisposables);

            // フォーカス解除通知
            _eventRouter.OnUnFocus
                .Subscribe(buttonEvent => OnUnFocusButton(buttonEvent))
                .AddTo(_routerDisposables);
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void Dispose()
        {
            _routerDisposables?.Dispose();
            _eventRouter?.Dispose();
        }

        // --------------------------------------------------
        // ボタン
        // --------------------------------------------------
        /// <summary>
        /// 通常ボタンイベントを初期化する
        /// </summary>
        private void InitializeNormalButtons()
        {
            // --------------------------------------------------
            // NormalButton 配列生成
            // --------------------------------------------------
            NormalButton[] normalButtons = new NormalButton[_baseNormalButtons.Length + _titleNormalButtons.Length];

            // ベースボタン配列コピー
            Array.Copy(_baseNormalButtons, 0,  normalButtons, 0, _baseNormalButtons.Length);

            // タイトルボタン配列コピー
            Array.Copy( _titleNormalButtons, 0, normalButtons, _baseNormalButtons.Length, _titleNormalButtons.Length);

            // --------------------------------------------------
            // 辞書生成
            // --------------------------------------------------
            _normalButtonEventTable = _buttonDictionaryBuilder.BuildNormalButtons(normalButtons);

            // --------------------------------------------------
            // イベント登録
            // --------------------------------------------------
            foreach (NormalButtonEvent buttonEvent in _normalButtonEventTable.Values)
            {
                _eventRouter.RegisterNormalButton(buttonEvent);
            }
        }

        /// <summary>
        /// Grid Layout Group 配下のオプションボタンを初期化する
        /// </summary>
        private void InitializeOptionButtons()
        {
            // --------------------------------------------------
            // 辞書生成
            // --------------------------------------------------
            _optionBinders = _buttonDictionaryBuilder.BuildOptionButtons(_optionButtonBinderFactory, _titleOptionButtons);

            // --------------------------------------------------
            // イベント登録
            // --------------------------------------------------
            foreach (OptionButtonBinder binder in _optionBinders.Values)
            {
                _eventRouter.RegisterOptionButtons(binder.Events);
            }
        }

        /// <summary>
        /// EventSystem の選択状態を更新する
        /// </summary>
        /// <param name="target">選択対象</param>
        private void SetSelectedGameObject(in GameObject target)
        {
            if (target == null)
            {
                return;
            }

            // 現在選択中のオブジェクト取得
            GameObject currentSelectedObject = _eventSystem.currentSelectedGameObject;

            // 同一オブジェクトが選択されている場合
            if (currentSelectedObject == target)
            {
                return;
            }

            // 選択状態を更新
            _eventSystem.SetSelectedGameObject(target);
        }

        /// <summary>
        /// ボタンイベントに応じてフォーカス状態を設定する
        /// </summary>
        /// <param name="buttonEvent">対象のボタンイベント</param>
        /// <param name="isFocus">フォーカス状態かどうか</param>
        private void SetFocusState(in BaseButtonEvent buttonEvent, in bool isFocus)
        {
            // 通常ボタンイベント
            if (buttonEvent is NormalButtonEvent normalButton)
            {
                // 通常ボタンのフォーカス状態を有効化
                _titleUIView.SetNormalFocus(normalButton.Button, isFocus);

                return;
            }

            // オプションボタンイベント
            if (buttonEvent is OptionButtonEvent optionButton)
            {
                // オプションボタンのフォーカス状態を有効化
                _titleUIView.SetOptionFocus(optionButton.Button, isFocus);

                return;
            }
        }

        /// <summary>
        /// 入力状態に応じて選択状態を設定する
        /// </summary>
        /// <param name="buttonEvent">再選択対象</param>
        private void SetSelectionState(BaseButtonEvent buttonEvent = null)
        {
            // ゲームパッド入力ではない場合
            if (!_isGamePadInput)
            {
                // EventSystem の選択解除
                OnUnSelectButton();

                return;
            }

            // 再選択対象が存在しない場合
            if (buttonEvent == null)
            {
                // 現在キャンバスに対応する初期選択ボタンを取得
                BaseButtonEvent initialButtonEvent = _titleUIStateController.GetInitialSelectedButton();

                // 初期選択ボタンを選択
                OnSelectButton(initialButtonEvent);

                return;
            }

            // 選択状態を設定
            OnSelectButton(buttonEvent);
        }

        /// <summary>
        /// ボードサイズに応じて ConnectCount の表示状態と初期選択を制御する
        /// </summary>
        private void ApplyBoardSizeDependentConnectCount(in int boardSize)
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

                // ビューへ選択状態を反映
                _titleUIView.ApplyButtonSelectionState(
                    connectCountBinder.Buttons,
                    connectCountBinder.SelectStateArray);
            }

            for (int i = 1; i < connectCountBinder.Buttons.Length; i++)
            {
                // インデックス 1 以降のボタンオブジェクト非表示
                connectCountBinder.Events[i].gameObject.SetActive(!isThreeSize);
            }
        }

        // --------------------------------------------------
        // イベントハンドラ
        // --------------------------------------------------
        /// <summary>
        /// NormalButton クリック時
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnNormalButtonClick(NormalButtonEvent buttonEvent)
        {
            // --------------------------------------------------
            // スタートボタン押下時
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeStart)
                && typeStart == UIActionType.TitleStart)
            {
                // ダイアログキャンバス表示
                _titleUIStateController.ShowDialogueCanvas();

                // ダイアログ YES ボタンを初期選択
                SetSelectionState(_normalButtonResolver.Get(UIActionType.DialogueYes));

                return;
            }

            // --------------------------------------------------
            // オプションボタン押下時
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeOption)
                && typeOption == UIActionType.TitleOption)
            {
                // オプションキャンバス表示
                _titleUIStateController.ShowOptionCanvas();

                // 最後に選択したボタンが OptionButton の場合
                if (_lastSelectedButtonEvent is OptionButtonEvent optionButton)
                {
                    // 最後に選択したオプションボタンを初期選択
                    SetSelectionState(optionButton);
                }
                else
                {
                    // 入力状態に応じて初期選択を適用
                    SetSelectionState();
                }

                // --------------------------------------------------
                // オプション UI 状態リセット
                // --------------------------------------------------
                foreach (OptionButtonBinder binder in _optionBinders.Values)
                {
                    // バインダー種別に対応する初期インデックスを辞書から取得
                    int index = _optionIndexTable.Get(binder.Type);

                    // インデックスを適用して選択状態を更新
                    binder.SelectByIndex(index);

                    // ビューへ選択状態を反映
                    _titleUIView.ApplyButtonSelectionState(
                        binder.Buttons,
                        binder.SelectStateArray);
                }

                // 盤面サイズ取得
                int boardSize = _gameOptionManager.BoardSize;

                // ConnectCount 制御
                ApplyBoardSizeDependentConnectCount(boardSize);

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, boardSize);

                return;
            }

            // --------------------------------------------------
            // オプションキャンセルボタン押下時
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeCancel)
                && typeCancel == UIActionType.TitleOptionCancel)
            {
                // スタートキャンバス表示
                _titleUIStateController.ShowStartCanvas();

                // オプションボタンを初期選択
                SetSelectionState(_normalButtonResolver.Get(UIActionType.TitleOption));

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, -1);

                return;
            }

            // --------------------------------------------------
            // オプション決定ボタン押下時
            // --------------------------------------------------
            if (_normalButtonResolver.TryGetType(buttonEvent, out UIActionType typeDecide)
                && typeDecide == UIActionType.TitleOptionDecide)
            {
                // スタートキャンバス表示
                _titleUIStateController.ShowStartCanvas();

                // オプション選択インデックスを更新
                foreach (KeyValuePair<OptionType, OptionButtonBinder> binder in _optionBinders)
                {
                    // 現在の選択インデックスを取得
                    int currentIndex = binder.Value.GetCurrentSelectedIndex();

                    // テーブルへ反映
                    _optionIndexTable.Set(binder.Key, currentIndex);
                }

                // スタートボタンを初期選択
                SetSelectionState(_normalButtonResolver.Get(UIActionType.TitleStart));

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, -1);
            }
        }

        /// <summary>
        /// オプションボタンクリック時
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnOptionButtonClick(OptionButtonEvent buttonEvent)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // 種別取得
            OptionType type = buttonEvent.Data.Type;

            // バインダー取得
            if (!_optionBinders.TryGetValue(type, out OptionButtonBinder binder))
            {
                return;
            }

            // ボタン選択処理
            binder.SelectByButton(buttonEvent.Button);

            // 選択状態をビューへ反映
            _titleUIView.ApplyButtonSelectionState(
                binder.Buttons,
                binder.SelectStateArray);

            // オプション更新通知
            _onUpdateGameOption.OnNext(buttonEvent.Data);

            // ボードサイズが変更された場合
            if (type == OptionType.BoardSize)
            {
                int boardSize = (int)buttonEvent.Data.BoardSizeType;

                // ConnectCount 制御
                ApplyBoardSizeDependentConnectCount(boardSize);

                // ボード変更アニメーションを実行
                _boardAnimator?.SetInteger(BOARD_SIZE_HASH, boardSize);
            }
        }

        /// <summary>
        /// ボタンへフォーカス状態を適用し、フォーカス座標を通知する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnFocusButton(BaseButtonEvent buttonEvent)
        {
            if (buttonEvent == null ||
                buttonEvent.RectTransform == null)
            {
                return;
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
            _pointerAnimator?.SetBool(IS_TARGET_HASH, true);
        }

        /// <summary>
        /// ボタンのフォーカス状態を解除する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnUnFocusButton(BaseButtonEvent buttonEvent)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // フォーカス状態非表示
            SetFocusState(buttonEvent, false);

            // ターゲット検出状態を解除
            _pointerAnimator?.SetBool(IS_TARGET_HASH, false);
        }

        /// <summary>
        /// EventSystem の選択状態を変更する
        /// </summary>
        /// <param name="buttonEvent">対象ボタンイベント</param>
        private void OnSelectButton(BaseButtonEvent buttonEvent)
        {
            if (buttonEvent == null)
            {
                return;
            }

            // 選択対象のボタンイベントをキャッシュ
            _lastSelectedButtonEvent = buttonEvent;

            // 選択状態を更新
            SetSelectedGameObject(buttonEvent.gameObject);
        }

        /// <summary>
        /// EventSystem の選択状態を解除する
        /// </summary>
        private void OnUnSelectButton()
        {
            // 選択解除
            _eventSystem.SetSelectedGameObject(null);
        }

        // ======================================================
        // ボタンイベント
        // ======================================================

        /// <summary>
        /// シーン遷移リクエストを通知する
        /// </summary>
        public override void RequestSceneChange()
        {
            base.RequestSceneChange();
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