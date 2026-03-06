// ======================================================
// UpdateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : Update 処理を管理する
// ======================================================

using SceneSystem.Controller;
using SceneSystem.Data;

namespace SceneSystem.Manager
{
    /// <summary>
    /// Update 処理の実行を担当する管理クラス
    /// </summary>
    public sealed class UpdateManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>毎フレーム更新対象を管理するコントローラ</summary>
        private readonly UpdateController _updateController;

        /// <summary>現在適用中のフェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// UpdateManager を生成する
        /// </summary>
        /// <param name="updateController">Update 実行用コントローラ</param>
        public UpdateManager(UpdateController updateController)
        {
            // UpdateController を保持
            _updateController = updateController;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        public void Update(in float unscaledDeltaTime, in float elapsedTime)
        {
            _updateController.OnUpdate(unscaledDeltaTime, elapsedTime);
        }

        public void LateUpdate(in float unscaledDeltaTime)
        {
            _updateController.OnLateUpdate(unscaledDeltaTime);
        }

        // --------------------------------------------------
        // フェーズ 管理
        // --------------------------------------------------
        /// <summary>
        /// フェーズ変更時に Exit / Enter を実行する
        /// </summary>
        /// <param name="nextPhase">遷移先フェーズ</param>
        public void ChangePhase(in PhaseType nextPhase)
        {
            // 現在フェーズの Exit を呼ぶ
            if (_currentPhase != PhaseType.None)
            {
                _updateController.OnPhaseExit(_currentPhase);
            }

            // フェーズを更新
            _currentPhase = nextPhase;

            // 遷移先フェーズの Enter を呼ぶ
            _updateController.OnPhaseEnter(_currentPhase);
        }
    }
}