// ======================================================
// PhaseController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-02-02
// 概要     : フェーズ遷移を制御し、UpdateController に更新対象を指示するコントローラ
// ======================================================

using System.Collections.Generic;
using SceneSystem.Data;

namespace SceneSystem.Controller
{
    /// <summary>
    /// フェーズ切替と UpdateController の操作を担当するコントローラ
    /// </summary>
    public class PhaseController
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
        private readonly Dictionary<PhaseType, IUpdatable[]> _phaseUpdatablesMap = new Dictionary<PhaseType, IUpdatable[]>();

        // ======================================================
        // コンストラクタ
        // ======================================================

        public PhaseController(in UpdateController updateController)
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