// ======================================================
// AssignUpdatablesService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-03-23
// 概要     : UpdateController に更新対象を登録するサービス
// ======================================================

using SceneSystem.Controller;
using SceneSystem.Data;

namespace PhaseSystem.Service
{
    /// <summary>
    /// UpdateController に更新対象を登録するサービス
    /// </summary>
    public sealed class AssignUpdatablesService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>UpdateController への参照</summary>
        private readonly UpdateController _updateController;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public AssignUpdatablesService(in UpdateController updateController)
        {
            _updateController = updateController;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// IUpdatable を登録する
        /// </summary>
        /// <param name="updatables">IUpdatable 配列</param>
        public void AssignUpdatables(in IUpdatable[] updatables)
        {
            // UpdateController に登録
            foreach (IUpdatable updatable in updatables)
            {
                _updateController.Add(updatable);
            }
        }
    }
}