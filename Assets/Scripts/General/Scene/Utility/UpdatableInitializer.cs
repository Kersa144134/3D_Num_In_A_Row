// ======================================================
// UpdatableInitializer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-24
// 概要     : シーン上の IUpdatable を収集し、
//            初期化済みコンテキストを生成する
// ======================================================

using SceneSystem.Data;

namespace SceneSystem.Utility
{
    /// <summary>
    /// IUpdatable 初期化処理
    /// </summary>
    public sealed class UpdatableInitializer
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// シーン内の IUpdatable を収集し初期化する
        /// </summary>
        /// <param name="updatables">更新対象となる IUpdatable</param>
        /// <returns>初期化済みコンテキスト</returns>
        public UpdatableContext InitializeUpdatables(in IUpdatable[] updatables)
        {
            // コンテキスト生成
            UpdatableContext context = BuildContext(updatables);

            // コンテキスト注入
            InjectContext(context);

            // 初期化処理実行
            Initialize(context);

            return context;
        }

        /// <summary>
        /// 収集済み IUpdatable の OnExit 処理を実行する
        /// </summary>
        /// <param name="context">共有コンテキスト</param>
        public void FinalizeUpdatables(in UpdatableContext context)
        {
            foreach (IUpdatable updatable in context.Updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                updatable.OnEnter();
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// IUpdatable 配列からコンテキストを構築する
        /// </summary>
        /// <param name="updatables">収集済み IUpdatable 配列</param>
        /// <returns>生成された UpdatableContext</returns>
        private UpdatableContext BuildContext(in IUpdatable[] updatables)
        {
            UpdatableContext context = new UpdatableContext();
            context.Updatables = updatables;

            // --------------------------------------------------
            // 型キャッシュ登録
            // --------------------------------------------------
            foreach (IUpdatable updatable in updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                // 型をキーにインスタンス登録
                context.Register(updatable.GetType(), updatable);
            }

            return context;
        }

        /// <summary>
        /// IContextInjectable を実装しているコンポーネントへ
        /// コンテキストを注入する
        /// </summary>
        /// <param name="context">共有コンテキスト</param>
        private void InjectContext(in UpdatableContext context)
        {
            // すべての Updatable を走査
            foreach (IUpdatable updatable in context.Updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                // コンテキスト注入
                if (updatable is IContextInjectable injectable)
                {
                    injectable.InjectContext(context);
                }
            }
        }

        /// <summary>
        /// すべての IUpdatable の OnEnter を呼び出す
        /// </summary>
        /// <param name="context">共有コンテキスト</param>
        private void Initialize(in UpdatableContext context)
        {
            foreach (IUpdatable updatable in context.Updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                updatable.OnEnter();
            }
        }
    }
}