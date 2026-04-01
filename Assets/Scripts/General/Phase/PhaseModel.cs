// ======================================================
// PhaseModel.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-24
// 更新日時 : 2026-03-24
// 概要     : フェーズ情報およびゲームプレイ経過時間を管理する Model
// ======================================================

using System.Collections.Generic;
using PhaseSystem.Data;

namespace PhaseSystem
{
    /// <summary>
    /// フェーズ進行管理用 Model
    /// </summary>
    public sealed class PhaseModel
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズと対応するステート</summary>
        private readonly Dictionary<PhaseType, IPhaseState> _stateMap;

        /// <summary>前フレームのフェーズ</summary>
        private PhaseType _previousPhase = PhaseType.None;

        /// <summary>ゲームプレイ経過時間</summary>
        private float _gamePlayElapsedTime = 0.0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ゲームプレイ経過時間の取得</summary>
        public float GamePlayElapsedTime => _gamePlayElapsedTime;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>Play フェーズから Finish フェーズへ遷移するまでの時間（秒）</summary>
        public const float PLAY_TO_FINISH_WAIT_TIME = 120.0f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// フェーズステートを初期化
        /// </summary>
        public PhaseModel()
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
        /// 指定フェーズに対応するステートを取得
        /// </summary>
        public IPhaseState GetState(in PhaseType phase)
        {
            _stateMap.TryGetValue(phase, out IPhaseState state);
            return state;
        }

        /// <summary>
        /// ゲームプレイ時間をリセット
        /// </summary>
        public void ResetElapsedTime()
        {
            _gamePlayElapsedTime = 0.0f;
        }

        /// <summary>
        /// ゲームプレイ時間を加算
        /// </summary>
        public void AddElapsedTime(in float delta)
        {
            _gamePlayElapsedTime += delta;
        }

        /// <summary>
        /// 前フレームフェーズの取得／設定
        /// </summary>
        public PhaseType PreviousPhase
        {
            get => _previousPhase;
            set => _previousPhase = value;
        }
    }
}