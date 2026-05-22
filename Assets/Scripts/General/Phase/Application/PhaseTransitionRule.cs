// ======================================================
// PhaseTransitionRule.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : フェーズ遷移条件を一元管理するクラス
// ======================================================

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

        /// <summary>Play フェーズへ遷移した回数</summary>
        private int _playEnterCount = 0;

        /// <summary>Finish フェーズへ遷移するまでの Play フェーズ開始可能回数</summary>
        private int _finishTransitionCount = 0;

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
        /// <param name="nextPhaseType">要求された遷移先フェーズ</param>
        /// <returns>実際に遷移するフェーズ</returns>
        public PhaseType ResolveRequestedPhase(in PhaseType nextPhaseType)
        {
            // --------------------------------------------------
            // ChangePlayer
            // --------------------------------------------------
            if (nextPhaseType is PhaseType.ChangePlayer)
            {
                // Play フェーズ開始回数を加算
                _playEnterCount++;

                // Play フェーズ開始回数が指定回数を超えた場合
                if (_playEnterCount > _finishTransitionCount)
                {
                    return PhaseType.Finish;
                }
            }

            // 指定フェーズをそのまま返却
            return nextPhaseType;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// Play フェーズの遷移および内部進行判定を行う
        /// </summary>
        /// <param name="state">Play フェーズ状態</param>
        /// <returns>遷移先フェーズ種別</returns>
        private PhaseType ResolvePlay(in PlayPhaseState state)
        {
            // 指定遷移時間を超えた場合
            if (state.PlayElapsedTime >= _transitionConfig.PerPlayerLimitTime)
            {
                // Play フェーズ開始回数を加算
                _playEnterCount++;

                // Play フェーズ開始回数が指定回数を超えた場合
                if (_playEnterCount > _finishTransitionCount)
                {
                    return PhaseType.Finish;
                }

                return PhaseType.ChangePlayer;
            }

            return PhaseType.Play;
        }
    }
}