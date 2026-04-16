// ======================================================
// SceneEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-06
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using BoardSystem.Domain;
using BoardSystem.Presentation;
using InputSystem;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using SceneSystem.Domain;
using System;
using UISystem.Presentation;
using UniRx;

namespace SceneSystem.Application
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed class SceneEventRouter
    {
        // ======================================================
        // 構造体
        // ======================================================

        /// <summary>
        /// 入力アクション種別
        /// </summary>
        private enum InputActionType
        {
            /// <summary>
            /// 未定義
            /// </summary>
            None,

            /// <summary>
            /// 駒を配置するアクション
            /// </summary>
            Drop,

            /// <summary>
            /// 盤面を回転するアクション
            /// </summary>
            Rotate
        }
        
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン内で共有されるコンテキスト</summary>
        private readonly UpdatableContext _context;

        /// <summary>InputManager キャッシュ</summary>
        private readonly InputManager _inputManager;

        /// <summary>SceneObjectContainer キャッシュ配列</summary>
        private readonly BoardPresenter[] _boardPresenters;

        /// <summary>MainUIPresenter キャッシュ</summary>
        private readonly MainUIPresenter _mainUIPresenter;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>直前のアクティブ状態フェーズキャッシュ</summary>
        private PhaseType _cachedActivePhase;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>フェーズ変更通知用 Subject</summary>
        private readonly Subject<PhaseType> _onPhaseChanged = new Subject<PhaseType>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseType> OnPhaseChanged => _onPhaseChanged;

        /// <summary>入力マッピング変更用 Subject</summary>
        private readonly Subject<int> _onMappingChanged = new Subject<int>();

        /// <summary>駒配置入力用 Subject</summary>
        private readonly Subject<Unit> _onDropRequested =
            new Subject<Unit>();

        /// <summary>駒配置入力ストリーム</summary>
        public IObservable<Unit> OnDropRequested =>
            _onDropRequested;

        /// <summary>回転入力用 Subject</summary>
        private readonly Subject<RotationCommand> _onRotateRequested =
            new Subject<RotationCommand>();

        /// <summary>回転入力ストリーム</summary>
        public IObservable<RotationCommand> OnRotateRequested =>
            _onRotateRequested;

        /// <summary>ライン成立通知用 Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete = new Subject<LineCompleteEvent>();

        /// <summary>現在フェーズストリーム参照</summary>
        private readonly IReadOnlyReactiveProperty<PhaseType> _currentPhase;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// SceneEventRouter を生成
        /// </summary>
        public SceneEventRouter(
            in UpdatableContext context,
            in IReadOnlyReactiveProperty<PhaseType> currentPhase)
        {
            _context = context;
            _currentPhase = currentPhase;

            // インスタンスからコンポーネントを取得
            _inputManager = InputManager.Instance;

            // Context からコンポーネントを取得
            _boardPresenters = _context.GetAll<BoardPresenter>();
            _mainUIPresenter = _context.Get<MainUIPresenter>();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベント購読
        /// </summary>
        public void Subscribe()
        {
            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            _inputManager.BindMappingStream(_onMappingChanged);

            // ボタン A 押下
            InputManager.Instance.ButtonA.OnUp
                .Subscribe(_ =>
                {
                    // 駒配置イベント発火
                    _onDropRequested.OnNext(Unit.Default);
                })
                .AddTo(_disposables);

            // ボタン X 押下
            InputManager.Instance.ButtonX.OnDown
                .Subscribe(_ =>
                {
                    // 回転イベント発火（X+）
                    _onRotateRequested.OnNext(
                        new RotationCommand(
                            RotationAxis.X,
                            RotationDirection.Positive
                        )
                    );
                })
                .AddTo(_disposables);

            // ボタン Y 押下
            InputManager.Instance.ButtonY.OnDown
                .Subscribe(_ =>
                {
                    // 回転イベント発火（X-）
                    _onRotateRequested.OnNext(
                        new RotationCommand(
                            RotationAxis.X,
                            RotationDirection.Negative
                        )
                    );
                })
                .AddTo(_disposables);

            // スタートボタン押下
            InputManager.Instance.StartButton.OnDown
                .Subscribe(e => OnStartButtonPressed(new StartButtonEvent(_currentPhase.Value)))
                .AddTo(_disposables);

            // --------------------------------------------------
            // ボード
            // --------------------------------------------------
            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.BindPhaseStream(_currentPhase);
                boardPresenter.BindInputStream(_onDropRequested, _onRotateRequested);

                boardPresenter.OnLineComplete
                    .Subscribe(e => _onLineComplete.OnNext(e))
                    .AddTo(_disposables);

                boardPresenter.OnPhaseEnd
                    .Subscribe(e => ChangePlayPhase(e))
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            //phaseMachine.OnLimitTimeUpdated
            //    .Subscribe(e => _mainUIPresenter?.UpdateLimitTimeDisplay(e.RemainingTime))
            //    .AddTo(_disposables);
        }

        /// <summary>
        /// イベント解除
        /// </summary>
        public void Dispose()
        {
            // 購読解除
            _disposables.Dispose();

            // サブジェクト終了
            _onPhaseChanged.OnCompleted();
            _onPhaseChanged.Dispose();

            _onLineComplete.OnCompleted();
            _onLineComplete.Dispose();

            _inputManager.UnbindMappingStream();

            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.UnbindPhaseStream();
                boardPresenter.UnbindInputStream();
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================
        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>
        /// Playフェーズを変更する
        /// </summary>
        private void ChangePlayPhase(in int player)
        {
            // トグル後のフェーズ
            PhaseType nextPhase;

            // Play_1 → Play_2
            if (player == 1)
            {
                nextPhase = PhaseType.Play;
            }
            // Play_2 → Play_1
            else if (player == 2)
            {
                nextPhase = PhaseType.Play;
            }
            // Play 以外は無視
            else
            {
                return;
            }

            // フェーズ変更通知
            _onPhaseChanged.OnNext(nextPhase);
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// スタートボタン押下時の処理を行う
        /// </summary>
        /// <param name="e">スタートボタンイベント</param>
        private void OnStartButtonPressed(in StartButtonEvent e)
        {
            // フェーズに応じてマッピングと遷移先を決定
            int mappingIndex;
            PhaseType nextPhase;

            // Play フェーズ判定
            bool isPlayPhases = false;

            if (e.Phase == PhaseType.Play)
            {
                isPlayPhases = true;

                // 現在のアクティブ状態フェーズをキャッシュ
                _cachedActivePhase = e.Phase;
            }

            if (isPlayPhases)
            {
                mappingIndex = 1;
                nextPhase = PhaseType.Pause;
            }
            else if (e.Phase == PhaseType.Pause)
            {
                mappingIndex = 0;

                // キャッシュしていたアクティブ状態フェーズへ復帰
                nextPhase = _cachedActivePhase;
            }
            else
            {
                return;
            }

            // 入力マッピング変更通知
            _onMappingChanged.OnNext(mappingIndex);

            // フェーズ変更通知
            _onPhaseChanged.OnNext(nextPhase);
        }
    }
}