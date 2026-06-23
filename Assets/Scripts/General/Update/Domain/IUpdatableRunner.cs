// ======================================================
// IUpdatableRunner.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : Updatable のフレーム実行およびフェーズ処理を提供するインターフェース
// ======================================================

#nullable enable

using PhaseSystem.Domain;

namespace UpdateSystem.Domain
{
    /// <summary>
    /// Updatable の実行処理を提供する
    /// </summary>
    public interface IUpdatableRunner
    {
        /// <summary>
        /// 毎フレームの Update 処理を実行する
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        void OnUpdate(in float unscaledDeltaTime);

        /// <summary>
        /// 毎フレームの LateUpdate 処理を実行する
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        void OnLateUpdate(in float unscaledDeltaTime);

        /// <summary>
        /// フェーズ開始時の処理を実行する
        /// </summary>
        /// <param name="phase">開始するフェーズ</param>
        void OnPhaseEnter(in PhaseType phase);

        /// <summary>
        /// フェーズ終了時の処理を実行する
        /// </summary>
        /// <param name="phase">終了するフェーズ</param>
        void OnPhaseExit(in PhaseType phase);
    }
}