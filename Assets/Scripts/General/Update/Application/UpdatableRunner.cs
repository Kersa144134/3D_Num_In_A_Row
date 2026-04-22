// ======================================================
// UpdatableRunner.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-04-22
// 概要     : 指定された IUpdatable を保持し、
//            毎フレーム処理を実行するランナー
// ======================================================

using System;
using PhaseSystem.Domain;
using UpdateSystem.Domain;

namespace UpdateSystem.Application
{
    /// <summary>
    /// IUpdatable を保持し、毎フレーム処理を実行するランナー
    /// </summary>
    public sealed class UpdatableRunner : IUpdatableRunner, IUpdatableRunnerModifier
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>実行対象のキャッシュ配列</summary>
        private IUpdatable[] _updatables = Array.Empty<IUpdatable>();

        // ======================================================
        // IUpdatableRunner 実装
        // ======================================================

        /// <summary>
        /// Update を実行する
        /// </summary>
        public void OnUpdate(in float unscaledDeltaTime)
        {
            for (int i = 0; i < _updatables.Length; i++)
            {
                _updatables[i]?.OnUpdate(unscaledDeltaTime);
            }
        }

        /// <summary>
        /// LateUpdate を実行する
        /// </summary>
        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            for (int i = 0; i < _updatables.Length; i++)
            {
                _updatables[i]?.OnLateUpdate(unscaledDeltaTime);
            }
        }

        /// <summary>
        /// フェーズ開始通知
        /// </summary>
        public void OnPhaseEnter(in PhaseType phase)
        {
            for (int i = 0; i < _updatables.Length; i++)
            {
                _updatables[i]?.OnPhaseEnter(phase);
            }
        }

        /// <summary>
        /// フェーズ終了通知
        /// </summary>
        public void OnPhaseExit(in PhaseType phase)
        {
            for (int i = 0; i < _updatables.Length; i++)
            {
                _updatables[i]?.OnPhaseExit(phase);
            }
        }

        // ======================================================
        // IUpdatableRunnerModifier 実装
        // ======================================================

        /// <summary>
        /// Updatable 配列を差し替える
        /// </summary>
        void IUpdatableRunnerModifier.Replace(in IUpdatable[] updatables)
        {
            if (updatables == null)
            {
                _updatables = Array.Empty<IUpdatable>();
                return;
            }

            _updatables = updatables;
        }
    }
}