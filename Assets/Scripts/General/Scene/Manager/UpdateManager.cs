// ======================================================
// UpdateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : Update 処理を管理する
// ======================================================

using System.Collections.Generic;
using PhaseSystem.Data;
using PhaseSystem.Service;
using SceneSystem.Data;
using SceneSystem.Controller;

namespace SceneSystem.Manager
{
    /// <summary>
    /// Update 処理の実行を担当する管理クラス
    /// </summary>
    public sealed class UpdateManager
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ切替制御クラス</summary>
        private AssignUpdatablesService _assignUpdatablesService;

        /// <summary>毎フレーム更新対象を管理するコントローラ</summary>
        private readonly UpdateController _updateController;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在適用中のフェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        /// <summary>フェーズごとの IUpdatable 配列を保持する辞書</summary>
        private readonly Dictionary<PhaseType, IUpdatable[]> _phaseUpdatablesMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// UpdateManager を生成する
        /// </summary>
        /// <param name="updateController">Update 実行用コントローラ</param>
        public UpdateManager(UpdateController updateController, Dictionary<PhaseType, IUpdatable[]> phaseUpdatablesMap)
        {
            _updateController = updateController;
            _phaseUpdatablesMap = phaseUpdatablesMap;

            _assignUpdatablesService = new AssignUpdatablesService(_updateController);
        }

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void Update(in float unscaledDeltaTime, in float elapsedTime)
        {
            _updateController.OnUpdate(unscaledDeltaTime, elapsedTime);
        }

        public void LateUpdate(in float unscaledDeltaTime)
        {
            _updateController.OnLateUpdate(unscaledDeltaTime);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

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

            // UpdateController をリセット
            _updateController.Clear();

            // フェーズに対応する Updatable を取得
            if (_phaseUpdatablesMap.TryGetValue(nextPhase, out IUpdatable[] updatables))
            {
                // UpdateController に反映
                _assignUpdatablesService.AssignUpdatables(updatables);
            }

            // 遷移先フェーズの Enter を呼ぶ
            _updateController.OnPhaseEnter(nextPhase);

            // フェーズを更新
            _currentPhase = nextPhase;
        }
    }
}