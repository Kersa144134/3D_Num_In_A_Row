// ======================================================
// PhaseMachine.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-16
// 更新日時 : 2026-04-16
// 概要     : フェーズ状態の実行および遷移制御を行うステートマシン
// ======================================================

using UniRx;
using PhaseSystem.Domain;
using SceneSystem.Application;
using SceneSystem.Domain;

namespace PhaseSystem.Application
{
    public sealed class PhaseMachine
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ状態リポジトリ</summary>
        private readonly PhaseStateRepository _stateRepository;

        /// <summary>遷移設定データ</summary>
        private readonly PhaseTransitionConfig _transitionConfig;

        /// <summary>遷移ルール</summary>
        private readonly PhaseTransitionRule _transitionRule;

        /// <summary>Update を管理するサービス</summary>
        private readonly UpdatableManagement _updatableManagement = new UpdatableManagement();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在のフェーズ状態</summary>
        private IPhaseState _currentState;

        /// <summary>現在のフェーズの制限時間</summary>
        private float _maxLimitTime;

        /// <summary>現在のフェーズ状態</summary>
        private IUpdatable[] _updatables;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>現在のフェーズ</summary>
        private readonly ReactiveProperty<PhaseType> _currentPhaseType =
            new ReactiveProperty<PhaseType>(PhaseType.None);

        /// <summary>現在のフェーズ</summary>
        public IReadOnlyReactiveProperty<PhaseType> CurrentPhaseType => _currentPhaseType;

        /// <summary>フェーズ経過時間</summary>
        private readonly ReactiveProperty<float> _limitTime =
            new ReactiveProperty<float>(0.0f);

        /// <summary>フェーズ経過時間</summary>
        public IReadOnlyReactiveProperty<float> LimitTime => _limitTime;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public PhaseMachine(
            in PhaseType initialPhase,
            in PhaseTransitionConfig transitionConfig,
            in IUpdatable[] updatables)
        {
            _transitionConfig = transitionConfig;
            _updatables = updatables;

            // --------------------------------------------------
            // フェーズ生成
            // --------------------------------------------------
            _stateRepository = new PhaseStateRepository(_transitionConfig);

            // --------------------------------------------------
            // 初期フェーズ設定
            // --------------------------------------------------
            _currentPhaseType.Value = initialPhase;

            _currentState = _stateRepository.GetPhaseState(initialPhase);

            // 開始処理
            _currentState.OnEnter();

            // --------------------------------------------------
            // フェーズ遷移設定
            // --------------------------------------------------
            _transitionRule = new PhaseTransitionRule(_stateRepository, _transitionConfig);
            _maxLimitTime = _transitionConfig.PerPlayerLimitTime;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ進行の更新処理を行う
        /// </summary>
        /// <param name="unscaledDeltaTime">経過時間</param>
        public void OnUpdate(in float unscaledDeltaTime)
        {
            _updatableManagement.ExecuteUpdate(unscaledDeltaTime);
            // 状態更新
            _currentState.OnUpdate(unscaledDeltaTime);

            // Play中のみ制限時間更新
            if (_currentState is PlayPhaseState playState)
            {
                _limitTime.Value = _maxLimitTime - playState.PlayElapsedTime;
            }
        }

        /// <summary>
        /// フェーズ進行の更新処理を行う
        /// </summary>
        /// <param name="unscaledDeltaTime">経過時間</param>
        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            _updatableManagement.ExecuteLateUpdate(unscaledDeltaTime);
            // 状態更新
            _currentState.OnLateUpdate(unscaledDeltaTime);

            // 遷移判定
            PhaseType nextPhase = _transitionRule.Resolve(_currentState);

            if (nextPhase == _currentPhaseType.Value)
            {
                return;
            }

            // フェーズ遷移
            ChangePhase(nextPhase);
        }

        /// <summary>
        /// フェーズ切替を行う
        /// </summary>
        public void ChangePhase(in PhaseType nextPhaseType)
        {
            // 遷移前フェーズを保持
            PhaseType previousPhaseType = _currentPhaseType.Value;

            // 終了処理
            _updatableManagement.ExecutePhaseExit(previousPhaseType);
            _currentState.OnExit();

            // 新しい State 取得
            _currentPhaseType.Value = nextPhaseType;
            _currentState = _stateRepository.GetPhaseState(nextPhaseType);

            // Updatables 再構築
            _updatableManagement.RebuildUpdatables(_updatables);

            // 開始処理
            _updatableManagement.ExecutePhaseEnter(nextPhaseType);
            if (_currentState is IPhaseEnterHandler handler)
            {
                handler.OnEnter(previousPhaseType);
            }
            else
            {
                _currentState.OnEnter();
            }
        }

        /// <summary>
        /// PlayPhaseState を取得する
        /// </summary>
        public PlayPhaseState GetPlayState()
        {
            return _stateRepository.GetPhaseState(PhaseType.Play) as PlayPhaseState;
        }
    }
}