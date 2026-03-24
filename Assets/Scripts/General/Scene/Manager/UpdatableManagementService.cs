// ======================================================
// UpdateManagementService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : Updatable を管理するサービス
// ======================================================

using System.Collections.Generic;
using PhaseSystem.Data;
using PhaseSystem.Service;
using SceneSystem.Data;
using SceneSystem.Runner;

namespace SceneSystem.Manager
{
    /// <summary>
    /// Updatable を管理するサービス
    /// </summary>
    public sealed class UpdatableManagementService
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>毎フレーム更新対象を管理するコントローラ</summary>
        private readonly UpdatableExecutor _updatableExecutor;

        /// <summary>フェーズ切替制御クラス</summary>
        private AssignUpdatablesService _assignUpdatablesService;

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
        /// UpdateManagementService を生成する
        /// </summary>
        /// <param name="updatableExecutor">Update 実行用コントローラ</param>
        /// <param name="phaseUpdatablesMap">フェーズごとの IUpdatable 配列を保持する辞書</param>
        public UpdatableManagementService(
            in UpdatableExecutor updatableExecutor,
            in Dictionary<PhaseType, IUpdatable[]> phaseUpdatablesMap)
        {
            _updatableExecutor = updatableExecutor;
            _phaseUpdatablesMap = phaseUpdatablesMap;

            _assignUpdatablesService = new AssignUpdatablesService(_updatableExecutor);
        }

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void Update(in float unscaledDeltaTime, in float elapsedTime)
        {
            _updatableExecutor.OnUpdate(unscaledDeltaTime, elapsedTime);
        }

        public void LateUpdate(in float unscaledDeltaTime)
        {
            _updatableExecutor.OnLateUpdate(unscaledDeltaTime);
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
                _updatableExecutor.OnPhaseExit(_currentPhase);
            }

            // UpdateController をリセット
            _updatableExecutor.Clear();

            // フェーズに対応する Updatable を取得
            if (_phaseUpdatablesMap.TryGetValue(nextPhase, out IUpdatable[] updatables))
            {
                // UpdateController に反映
                _assignUpdatablesService.AssignUpdatables(updatables);
            }

            // 遷移先フェーズの Enter を呼ぶ
            _updatableExecutor.OnPhaseEnter(nextPhase);

            // フェーズを更新
            _currentPhase = nextPhase;
        }
    }
}