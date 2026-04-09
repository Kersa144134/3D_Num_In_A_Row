// ======================================================
// SceneEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-06
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using System;
using UniRx;
using BoardSystem.Domain;
using BoardSystem.Presentation;
using InputSystem;
using PhaseSystem.Domain;
using PhaseSystem.Presentation;
using SceneSystem.Data;
using UISystem;

namespace SceneSystem.Utility
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed class SceneEventRouter
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン内で共有されるコンテキスト</summary>
        private readonly UpdatableContext _context;

        /// <summary>InputManager キャッシュ</summary>
        private readonly InputManager _inputManager;

        /// <summary>SceneObjectContainer キャッシュ配列</summary>
        private readonly BoardPresenter[] _boardPresenters;

        /// <summary>MainUIManager キャッシュ</summary>
        private readonly MainUIManager _mainUIManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Play フェーズ配列</summary>
        private readonly PhaseType[] _playPhases;

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

        /// <summary>入力マッピング変更ストリーム</summary>
        public IObservable<int> OnMappingChanged => _onMappingChanged;

        /// <summary>ライン成立通知用 Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete = new Subject<LineCompleteEvent>();

        /// <summary>ライン成立ストリーム</summary>
        public IObservable<LineCompleteEvent> OnLineComplete => _onLineComplete;

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
            _mainUIManager = _context.Get<MainUIManager>();

            // Play フェーズを定義
            _playPhases = new[]
            {
                PhaseType.Play_1,
                PhaseType.Play_2
            };
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベント購読
        /// </summary>
        public void Subscribe(in PhasePresenter phasePresenter)
        {
            // --------------------------------------------------
            // フェーズ
            // --------------------------------------------------
            phasePresenter.OnStartButtonPressed
                .Subscribe(e => OnStartButtonPressed(e))
                .AddTo(_disposables);

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            _inputManager.BindMappingStream(_onMappingChanged);
            
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

                boardPresenter.OnLineComplete
                    .Subscribe(e => _onLineComplete.OnNext(e))
                    .AddTo(_disposables);

                boardPresenter.OnPhaseEnd
                    .Subscribe(_ => TogglePlayPhase())
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            phasePresenter.OnLimitTimeUpdated
                .Subscribe(e => _mainUIManager?.UpdateLimitTimeDisplay(e.RemainingTime))
                .AddTo(_disposables);
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
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================
        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>
        /// Playフェーズをトグルする
        /// </summary>
        private void TogglePlayPhase()
        {
            // 現在フェーズ取得
            PhaseType current = _currentPhase.Value;

            // トグル後のフェーズ
            PhaseType nextPhase;

            // Play_1 → Play_2
            if (current == PhaseType.Play_1)
            {
                nextPhase = PhaseType.Play_2;
            }
            // Play_2 → Play_1
            else if (current == PhaseType.Play_2)
            {
                nextPhase = PhaseType.Play_1;
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

            for (int i = 0; i < _playPhases.Length; i++)
            {
                if (e.Phase == _playPhases[i])
                {
                    isPlayPhases = true;

                    // 現在のアクティブ状態フェーズをキャッシュ
                    _cachedActivePhase = e.Phase;
                    break;
                }
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