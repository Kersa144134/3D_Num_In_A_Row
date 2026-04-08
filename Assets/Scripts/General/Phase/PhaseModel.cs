// ======================================================
// PhaseModel.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-24
// 更新日時 : 2026-03-24
// 概要     : フェーズ情報およびゲームプレイ経過時間を管理するモデル
// ======================================================

using System.Collections.Generic;
using PhaseSystem.Data;

namespace PhaseSystem
{
    /// <summary>
    /// フェーズ進行管理用モデル
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

        /// <summary>1P ID</summary>
        private const int PLAYER_ONE = 1;

        /// <summary>2P ID</summary>
        private const int PLAYER_TWO = 2;

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
                { PhaseType.Play_1, new PlayPhaseState(PLAYER_ONE) },
                { PhaseType.Play_2, new PlayPhaseState(PLAYER_TWO) },
                { PhaseType.Event,  new EventPhaseState() },
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