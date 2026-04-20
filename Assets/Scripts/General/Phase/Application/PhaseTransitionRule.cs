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
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在のフェーズ状態と経過時間をもとに遷移先フェーズを決定する
        /// </summary>
        /// <param name="currentState">現在のフェーズ状態</param>
        /// <param name="deltaTime">フレーム経過時間</param>
        /// <returns>遷移先フェーズ種別</returns>
        public PhaseType Resolve(in IPhaseState currentState)
        {
            // --------------------------------------------------
            // Ready
            // --------------------------------------------------
            if (currentState is ReadyPhaseState ready)
            {
                return ResolveReady(ready);
            }

            // --------------------------------------------------
            // Play
            // --------------------------------------------------
            if (currentState is PlayPhaseState play)
            {
                return ResolvePlay(play);
            }

            // --------------------------------------------------
            // ChangePlayer
            // --------------------------------------------------
            if (currentState is ChangePlayerPhaseState changePlayer)
            {
                return ResolveChangePlayer(changePlayer);
            }
            
            // --------------------------------------------------
            // デフォルト
            // --------------------------------------------------
            return _stateRepository.GetPhaseType(currentState);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// Ready フェーズの遷移判定を行う
        /// </summary>
        /// <param name="state">Ready フェーズ状態</param>
        /// <returns>遷移先フェーズ種別</returns>
        private PhaseType ResolveReady(in ReadyPhaseState state)
        {
            if (state.ElapsedTime >= _transitionConfig.ReadyToChangePlayerWaitTime)
            {
                return PhaseType.ChangePlayer;
            }

            return PhaseType.Ready;
        }

        /// <summary>
        /// Play フェーズの遷移および内部進行判定を行う
        /// </summary>
        /// <param name="state">Play フェーズ状態</param>
        /// <returns>遷移先フェーズ種別</returns>
        private PhaseType ResolvePlay(in PlayPhaseState state)
        {
            if (state.ElapsedTime >= _transitionConfig.PerPlayerLimitTime)
            {
                return PhaseType.ChangePlayer;
            }

            return PhaseType.Play;
        }

        /// <summary>
        /// Ready フェーズの遷移判定を行う
        /// </summary>
        /// <param name="state">ChangePlayer フェーズ状態</param>
        /// <returns>遷移先フェーズ種別</returns>
        private PhaseType ResolveChangePlayer(in ChangePlayerPhaseState state)
        {
            if (state.ElapsedTime >= _transitionConfig.ChangePlayerToPlayWaitTime)
            {
                return PhaseType.Play;
            }

            return PhaseType.ChangePlayer;
        }
    }
}