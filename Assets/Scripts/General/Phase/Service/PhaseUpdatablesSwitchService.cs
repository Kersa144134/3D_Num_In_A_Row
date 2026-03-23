// ======================================================
// PhaseUpdatablesSwitchService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-03-23
// 概要     : UpdateController に更新対象を登録するサービス
// ======================================================

using System.Collections.Generic;
using PhaseSystem.Data;
using SceneSystem.Controller;
using SceneSystem.Data;

namespace PhaseSystem.Service
{
    /// <summary>
    /// UpdateController に更新対象を登録するサービス
    /// </summary>
    public sealed class PhaseUpdatablesSwitchService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>UpdateController への参照</summary>
        private readonly UpdateController _updateController;

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>フェーズごとの Updatable 配列を保持する辞書</summary>
        private readonly Dictionary<PhaseType, IUpdatable[]> _phaseUpdatablesMap = new();

        // ======================================================
        // コンストラクタ
        // ======================================================

        public PhaseUpdatablesSwitchService(in UpdateController updateController)
        {
            _updateController = updateController;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズに紐づく IUpdatable を登録する
        /// </summary>
        /// <param name="phase">登録対象のフェーズ</param>
        /// <param name="updatables">フェーズに属する IUpdatable 配列</param>
        public void AssignPhaseUpdatables(in PhaseType phase, in IUpdatable[] updatables)
        {
            _phaseUpdatablesMap[phase] = updatables;

            // UpdateController に登録
            foreach (IUpdatable updatable in updatables)
            {
                _updateController.Add(updatable);
            }
        }
    }
}