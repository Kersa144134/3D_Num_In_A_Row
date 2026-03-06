// ======================================================
// IUpdatable.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-01-23
// 概要     : UpdateController から呼び出される更新処理用インターフェース
// ======================================================

namespace SceneSystem.Data
{
    /// <summary>
    /// UpdateController によって管理される更新対象のためのインターフェース
    /// シーンおよびフェーズに対応したイベントを提供する
    /// </summary>
    public interface IUpdatable
    {
        // ======================================================
        // メソッド
        // ======================================================

        /// <summary>
        /// シーン開始時に 1 度だけ呼ばれる初期化処理
        /// </summary>
        void OnEnter() { }

        /// <summary>
        /// 毎フレーム実行される更新処理
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        /// <param name="elapsedTime">ゲームの経過時間</param>
        void OnUpdate(in float unscaledDeltaTime, in float elapsedTime) { }

        /// <summary>
        /// LateUpdate 相当で毎フレーム実行される処理
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        void OnLateUpdate(in float unscaledDeltaTime) { }

        /// <summary>
        /// シーン終了時に 1 度だけ呼ばれる終了処理
        /// </summary>
        void OnExit() { }

        /// <summary>
        /// フェーズ突入時に 1 度だけ呼ばれる初期化処理
        /// </summary>
        /// <param name="phase">遷移先のフェーズ</param>
        void OnPhaseEnter(in PhaseType phase) { }

        /// <summary>
        /// フェーズ離脱時に 1 度だけ呼ばれる終了処理
        /// </summary>
        /// <param name="phase">現在のフェーズ</param>
        void OnPhaseExit(in PhaseType phase) { }
    }
}