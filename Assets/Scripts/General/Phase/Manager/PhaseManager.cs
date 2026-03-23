// ======================================================
// PhaseManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : シーン及びフェーズ遷移条件を管理する
// ======================================================

using PhaseSystem.Data;
using System.Collections.Generic;

namespace PhaseSystem.Manager
{
    /// <summary>
    /// シーン及びフェーズ遷移条件を管理する
    /// </summary>
    public sealed class PhaseManager
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>Play フェーズから Finish フェーズへ遷移するまでのゲームプレイ時間（秒）</summary>
        public const float PLAY_TO_FINISH_WAIT_TIME = 120.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズとステートの対応表</summary>
        private readonly Dictionary<PhaseType, IPhaseState> _stateMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PhaseManager()
        {
            _stateMap = new Dictionary<PhaseType, IPhaseState>
            {
                { PhaseType.Title,  new TitlePhaseState() },
                { PhaseType.Ready,  new ReadyPhaseState() },
                { PhaseType.Play,   new PlayPhaseState() },
                { PhaseType.Pause,  new PausePhaseState() },
                { PhaseType.Finish,  new FinishPhaseState() },
                { PhaseType.Result, new ResultPhaseState() }
            };
        }
        
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        /// <param name="elapsedTime">インゲームの経過時間</param>
        /// <param name="currentScene">現在のシーン名</param>
        /// <param name="currentPhase">現在のフェーズ</param>
        /// <param name="targetScene">遷移先シーン名</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        public void Update(
            in float unscaledDeltaTime,
            in float elapsedTime,
            in string currentScene,
            in PhaseType currentPhase,
            out string targetScene,
            out PhaseType targetPhase
        )
        {
            if (_stateMap.TryGetValue(currentPhase, out IPhaseState state) == false)
            {
                targetScene = currentScene;
                targetPhase = currentPhase;
                return;
            }

            state.Update(
                unscaledDeltaTime,
                elapsedTime,
                currentScene,
                out targetScene,
                out targetPhase
            );
        }
    }
}