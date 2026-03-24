// ======================================================
// PhaseManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-24
// 概要     : シーン及びフェーズ遷移条件を管理する
// ======================================================

using System.Collections.Generic;
using PhaseSystem.Data;

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
        public const float PLAY_TO_FINISH_WAIT_TIME = 5.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズとステートの対応表</summary>
        private readonly Dictionary<PhaseType, IPhaseState> _stateMap;

        /// <summary>前フレームのフェーズ</summary>
        private PhaseType _previousPhase = PhaseType.None;

        /// <summary>ゲームプレイ経過時間</summary>
        private float _gamePlayElapsedTime = 0.0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ゲームプレイ経過時間</summary>
        public float GamePlayElapsedTime => _gamePlayElapsedTime;

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
                { PhaseType.Finish, new FinishPhaseState() },
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
        /// <param name="currentPhase">現在のフェーズ</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        public void Update(
            in float unscaledDeltaTime,
            in PhaseType currentPhase,
            out PhaseType targetPhase)
        {
            targetPhase = currentPhase;

            // --------------------------------------------------
            // State 取得
            // --------------------------------------------------
            if (!_stateMap.TryGetValue(currentPhase, out IPhaseState currentState))
            {
                return;
            }

            // --------------------------------------------------
            // フェーズ遷移検知
            // --------------------------------------------------
            if (currentPhase != _previousPhase)
            {
                // 前フェーズの終了時処理
                if (_stateMap.TryGetValue(_previousPhase, out IPhaseState prevState))
                {
                    prevState.OnExit();
                }

                // Ready フェーズ開始時にリセット
                if (currentPhase == PhaseType.Ready)
                {
                    _gamePlayElapsedTime = 0.0f;
                }

                // 現フェーズの開始時処理
                currentState.OnEnter();
            }

            // --------------------------------------------------
            // State 更新
            // --------------------------------------------------
            currentState.OnUpdate(unscaledDeltaTime);

            if (currentPhase == PhaseType.Play)
            {
                _gamePlayElapsedTime += unscaledDeltaTime;
            }

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------
            switch (currentPhase)
            {
                case PhaseType.Play:

                    if (_gamePlayElapsedTime > PLAY_TO_FINISH_WAIT_TIME)
                    {
                        targetPhase = PhaseType.Finish;
                    }

                    break;
            }

            // --------------------------------------------------
            // フェーズ履歴更新
            // --------------------------------------------------
            _previousPhase = currentPhase;
        }
    }
}