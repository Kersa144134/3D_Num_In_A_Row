// ======================================================
// AssignUpdatablesService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-03-23
// 概要     : UpdatableExecutor に更新対象を登録するサービス
// ======================================================

using SceneSystem.Domain;

namespace SceneSystem.Application
{
    /// <summary>
    /// UpdateController に更新対象を登録するサービス
    /// </summary>
    public sealed class AssignUpdatablesService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Update 処理を実行するランナー</summary>
        private readonly UpdatableExecutor _updatableExecutor;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AssignUpdatablesService(in UpdatableExecutor updatableExecutor)
        {
            _updatableExecutor = updatableExecutor;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// IUpdatable を登録する
        /// SceneManager の子オブジェクトの配置順に登録される
        /// </summary>
        /// <param name="updatables">IUpdatable 配列</param>
        public void AssignUpdatables(in IUpdatable[] updatables)
        {
            // UpdateController に登録
            foreach (IUpdatable updatable in updatables)
            {
                _updatableExecutor.Add(updatable);
            }
        }
    }
}