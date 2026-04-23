// ======================================================
// PhaseStateRepository.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : フェーズステートを保持・提供するクラス
// ======================================================

using System;
using System.Collections.Generic;

namespace PhaseSystem.Domain
{
    /// <summary>
    /// フェーズステートを生成せず保持し、再利用するクラス
    /// </summary>
    public sealed class PhaseStateRepository
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズ状態マップ</summary>
        private readonly Dictionary<PhaseType, IPhaseState> _stateMap;

        /// <summary>型→フェーズ逆引きマップ</summary>
        private readonly Dictionary<Type, PhaseType> _typeMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="config">フェーズ遷移設定</param>
        public PhaseStateRepository()
        {
            // --------------------------------------------------
            // フェーズインスタンス生成
            // --------------------------------------------------
            _stateMap = new Dictionary<PhaseType, IPhaseState>
            {
                { PhaseType.None,         new NonePhaseState() },
                { PhaseType.Title,        new TitlePhaseState() },
                { PhaseType.Ready,        new ReadyPhaseState() },
                { PhaseType.Play,         new PlayPhaseState() },
                { PhaseType.Event,        new EventPhaseState() },
                { PhaseType.ChangePlayer, new ChangePlayerPhaseState() },
                { PhaseType.Pause,        new PausePhaseState() },
                { PhaseType.Finish,       new FinishPhaseState() },
                { PhaseType.Result,       new ResultPhaseState() }
            };

            // --------------------------------------------------
            // 型 → フェーズ逆引きマップ生成
            // --------------------------------------------------
            _typeMap = new Dictionary<Type, PhaseType>();

            foreach (KeyValuePair<PhaseType, IPhaseState> pair in _stateMap)
            {
                Type type = pair.Value.GetType();
                _typeMap[type] = pair.Key;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定フェーズのステートを取得
        /// </summary>
        /// <param name="phaseType">フェーズ種別</param>
        /// <returns>対応するステート</returns>
        public IPhaseState GetPhaseState(in PhaseType phaseType)
        {
            // マップから取得
            if (_stateMap.TryGetValue(phaseType, out IPhaseState state))
            {
                return state;
            }

            return null;
        }

        /// <summary>
        /// ステートからフェーズ種別を取得
        /// </summary>
        /// <param name="state">フェーズステート</param>
        /// <returns>フェーズ種別</returns>
        public PhaseType GetPhaseType(in IPhaseState state)
        {
            // 型取得
            Type type = state.GetType();

            // 逆引き
            if (_typeMap.TryGetValue(type, out PhaseType phase))
            {
                return phase;
            }

            return PhaseType.None;
        }
    }
}