// ======================================================
// UpdatableLifecycleRunner.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-22
// 概要     : シーン上の IUpdatable を収集し、
//            初期化済みコンテキストを生成する
// ======================================================

using UpdateSystem.Domain;

namespace UpdateSystem.Application
{
    /// <summary>
    /// IUpdatable ライフサイクル実行クラス
    /// </summary>
    public sealed class UpdatableLifecycleRunner
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// OnEnter を実行する
        /// </summary>
        /// <param name="updatableEnumerable">列挙専用 Updatable コンテキスト</param>
        public void RunEnter(in IUpdatableEnumerable updatableEnumerable)
        {
            foreach (IUpdatable updatable in updatableEnumerable.GetAllUpdatables())
            {
                if (updatable == null)
                {
                    continue;
                }

                updatable.OnEnter();
            }
        }

        /// <summary>
        /// OnExit を実行する
        /// </summary>
        /// <param name="updatableEnumerable">列挙専用 Updatable コンテキスト</param>
        public void RunExit(in IUpdatableEnumerable updatableEnumerable)
        {
            foreach (IUpdatable updatable in updatableEnumerable.GetAllUpdatables())
            {
                if (updatable == null)
                {
                    continue;
                }

                updatable.OnExit();
            }
        }
    }
}