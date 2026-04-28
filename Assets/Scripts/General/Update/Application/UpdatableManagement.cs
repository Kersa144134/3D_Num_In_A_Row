// ======================================================
// UpdatableManagement.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-22
// 概要     : Updatable を管理するサービス
// ======================================================

using System;
using PhaseSystem.Domain;
using UpdateSystem.Domain;

namespace UpdateSystem.Application
{
    /// <summary>
    /// Updatable を管理するサービス
    /// </summary>
    public sealed class UpdatableManagement
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>Updatable 実行専用インターフェース</summary>
        private readonly IUpdatableRunner _runner;

        /// <summary>Updatable 変更専用インターフェース</summary>
        private readonly IUpdatableRunnerModifier _modifier;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// UpdatableManagement を生成する
        /// </summary>
        public UpdatableManagement()
        {
            // Runner を生成する
            UpdatableRunner runner = new UpdatableRunner();
            _runner = runner;
            _modifier = runner;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 毎フレームの Update 処理
        /// </summary>
        /// <param name="unscaledDeltaTime">経過時間</param>
        public void ExecuteUpdate(in float unscaledDeltaTime)
        {
            _runner.OnUpdate(unscaledDeltaTime);
        }

        /// <summary>
        /// 毎フレームの LateUpdate 処理
        /// </summary>
        /// <param name="unscaledDeltaTime">経過時間</param>
        public void ExecuteLateUpdate(in float unscaledDeltaTime)
        {
            _runner.OnLateUpdate(unscaledDeltaTime);
        }

        /// <summary>
        /// フェーズ開始処理を実行する
        /// </summary>
        /// <param name="phase">開始対象フェーズ</param>
        public void ExecutePhaseEnter(in PhaseType phase)
        {
            _runner.OnPhaseEnter(phase);
        }

        /// <summary>
        /// フェーズ終了処理を実行する
        /// </summary>
        /// <param name="phase">終了対象フェーズ</param>
        public void ExecutePhaseExit(in PhaseType phase)
        {
            _runner.OnPhaseExit(phase);
        }

        /// <summary>
        /// Updatable の登録内容を再構築する
        /// </summary>
        /// <param name="updatables">登録対象の Updatable 配列</param>
        public void RebuildUpdatables(in IUpdatable[] updatables)
        {
            // null の場合は空配列として扱う
            IUpdatable[] safeArray = updatables ?? Array.Empty<IUpdatable>();

            IUpdatable[] buffer = new IUpdatable[safeArray.Length];

            int index = 0;

            for (int i = 0; i < safeArray.Length; i++)
            {
                IUpdatable updatable = safeArray[i];

                if (updatable == null)
                {
                    continue;
                }

                // バッファへ格納
                buffer[index] = updatable;

                index++;
            }

            // 有効要素数に合わせた配列を生成
            IUpdatable[] result = new IUpdatable[index];

            for (int i = 0; i < index; i++)
            {
                result[i] = buffer[i];
            }

            _modifier.Replace(result);
        }
    }
}