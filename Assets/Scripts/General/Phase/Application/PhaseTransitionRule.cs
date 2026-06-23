// ======================================================
// PhaseTransitionRule.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : フェーズ遷移条件を一元管理するクラス
// ======================================================

using UniRx;
using PhaseSystem.Domain;

namespace PhaseSystem.Application
{
    public sealed class PhaseTransitionRule
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ状態リポジトリ</summary>
        private readonly PhaseStateRepository _stateRepository;

        /// <summary>設定データ</summary>
        private readonly PhaseTransitionConfig _transitionConfig;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Finish フェーズへ遷移するまでの Play フェーズ開始可能回数</summary>
        private int _finishTransitionCount = 0;

        // ======================================================
        // UniRx 関連
        // ======================================================

        /// <summary>Play フェーズへ遷移した回数</summary>
        private readonly ReactiveProperty<int> _playEnterCount =
            new ReactiveProperty<int>(0);

        /// <summary>Play フェーズへ遷移した回数</summary>
        public IReadOnlyReactiveProperty<int> PlayEnterCount => _playEnterCount;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PhaseTransitionRule(
            in PhaseStateRepository stateRepository,
            in PhaseTransitionConfig config)
        {
            _stateRepository = stateRepository;
            _transitionConfig = config;

            // Play フェーズ開始可能回数を算出
            _finishTransitionCount
                = _transitionConfig.PlayerCount * _transitionConfig.TurnCount;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在のフェーズ状態と経過時間をもとに遷移先フェーズを決定する
        /// </summary>
        /// <param name="currentState">現在のフェーズ状態</param>
        /// <returns>遷移先フェーズ種別</returns>
        public PhaseType ResolveTimeTransition(in IPhaseState currentState)
        {
            // --------------------------------------------------
            // Play
            // --------------------------------------------------
            if (currentState is PlayPhaseState play)
            {
                return ResolvePlay(play);
            }

            // --------------------------------------------------
            // デフォルト
            // --------------------------------------------------
            return _stateRepository.GetPhaseType(currentState);
        }

        /// <summary>
        /// 外部からリクエストされたフェーズ遷移を確定する
        /// </summary>
        /// <param name="currentPhaseType">現在フェーズ</param>
        /// <param name="nextPhaseType">要求された遷移先フェーズ</param>
        /// <returns>実際に遷移するフェーズ</returns>
        public PhaseType ResolveRequestedPhase(
            in PhaseType currentPhaseType,
            in PhaseType nextPhaseType)
        {
            // --------------------------------------------------
            // ChangePlayer
            // --------------------------------------------------
            if (nextPhaseType is PhaseType.ChangePlayer)
            {
                // Play フェーズ開始回数を加算
                _playEnterCount.Value++;

                // Finish 遷移条件を満たしている場合
                if (IsFinishTransition())
                {
                    return PhaseType.Finish;
                }
            }

            // --------------------------------------------------
            // Pause
            // --------------------------------------------------
            if (nextPhaseType is PhaseType.Pause)
            {
                // Play フェーズ以外では Pause 遷移を実行しない
                if (currentPhaseType is not PhaseType.Play)
                {
                    return currentPhaseType;
                }
            }

            // --------------------------------------------------
            // デフォルト
            // --------------------------------------------------
            return nextPhaseType;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// Play フェーズ時の遷移判定を行う
        /// </summary>
        /// <param name="state">Play フェーズ状態</param>
        /// <returns>遷移先フェーズ種別</returns>
        private PhaseType ResolvePlay(in PlayPhaseState state)
        {
            // 指定遷移時間を超えた場合
            if (state.PlayElapsedTime >= _transitionConfig.PerPlayerLimitTime)
            {
                // Play フェーズ開始回数を加算
                _playEnterCount.Value++;

                // Finish 遷移条件を満たしている場合
                if (IsFinishTransition())
                {
                    return PhaseType.Finish;
                }

                return PhaseType.ChangePlayer;
            }

            return PhaseType.Play;
        }

        /// <summary>
        /// Finish フェーズへ遷移すべきかを判定する
        /// </summary>
        /// <returns>Finish へ遷移する場合 true</returns>
        private bool IsFinishTransition()
        {
            // 現在の Play フェーズ進行回数が閾値を超えているか判定
            bool isOverThreshold = _playEnterCount.Value > _finishTransitionCount;

            // Finish 条件を満たしているか返却
            return isOverThreshold;
        }
    }
}