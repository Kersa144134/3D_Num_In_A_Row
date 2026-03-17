// ======================================================
// UpdatableInitializer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-06
// 概要     : シーン上の IUpdatable を収集し、
//            初期化済みコンテキストを生成する
// ======================================================

using UnityEngine;
using SceneSystem.Data;

namespace SceneSystem.Utility
{
    /// <summary>
    /// IUpdatable 初期化処理
    /// </summary>
    public sealed class UpdatableInitializer
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// IUpdatable をシーンから収集するコレクター
        /// </summary>
        private readonly UpdatableCollector _collector;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="collector">IUpdatable を取得するコレクター</param>
        public UpdatableInitializer(in UpdatableCollector collector)
        {
            // コレクター参照を保持
            _collector = collector;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// シーン内の IUpdatable を収集し初期化する
        /// </summary>
        /// <param name="roots">探索対象となるルート GameObject</param>
        /// <returns>初期化済みコンテキスト</returns>
        public UpdatableContext Initialize(in GameObject[] roots)
        {
            // シーンから IUpdatable を収集
            IUpdatable[] updatables = _collector.Collect(roots);

            // コンテキスト生成
            UpdatableContext context = BuildContext(updatables);

            // コンテキスト注入
            InjectContext(context);

            // 初期化処理実行
            InitializeUpdatables(context);

            return context;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// IUpdatable 配列からコンテキストを構築する
        /// </summary>
        /// <param name="updatables">収集済み IUpdatable 配列</param>
        /// <returns>生成された UpdatableContext</returns>
        private UpdatableContext BuildContext(IUpdatable[] updatables)
        {
            // --------------------------------------------------
            // コンテキスト生成
            // --------------------------------------------------
            UpdatableContext context = new UpdatableContext();

            // --------------------------------------------------
            // 全 IUpdatable を保持
            // --------------------------------------------------
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

                // --------------------------------------------------
                // 型をキーにインスタンス登録
                // 同一型が複数ある場合は後登録が優先される
                // --------------------------------------------------
                context.Register(updatable.GetType(), updatable);
            }

            return context;
        }

        /// <summary>
        /// IContextInjectable を実装しているコンポーネントへ
        /// コンテキストを注入する
        /// </summary>
        /// <param name="context">共有コンテキスト</param>
        private void InjectContext(UpdatableContext context)
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
        private void InitializeUpdatables(UpdatableContext context)
        {
            // すべての Updatable を初期化
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