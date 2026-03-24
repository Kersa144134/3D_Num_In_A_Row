// ======================================================
// UpdatableInitializer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-24
// 概要     : シーン上の IUpdatable を収集し、
//            初期化済みコンテキストを生成する
// ======================================================

using System;
using System.Collections.Generic;
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
        /// <param name="updatables">更新対象となる IUpdatable 配列</param>
        /// <returns>初期化済みコンテキスト</returns>
        public UpdatableContext InitializeUpdatables(in IUpdatable[] updatables)
        {
            // --------------------------------------------------
            // コンテキスト生成: 型ごとに複数登録可能
            // --------------------------------------------------
            UpdatableContext context = BuildContext(updatables);

            // --------------------------------------------------
            // コンテキスト注入: IContextInjectable を実装している場合のみ
            // --------------------------------------------------
            InjectContext(context);

            // --------------------------------------------------
            // 初期化処理実行: OnEnter を呼ぶ
            // --------------------------------------------------
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

                // フェーズ終了処理
                updatable.OnExit();
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// IUpdatable 配列からコンテキストを構築する
        /// 型ごとに複数オブジェクトを登録可能
        /// </summary>
        /// <param name="updatables">収集済み IUpdatable 配列</param>
        /// <returns>生成された UpdatableContext</returns>
        private UpdatableContext BuildContext(in IUpdatable[] updatables)
        {
            // コンテキスト生成
            UpdatableContext context = new UpdatableContext
            {
                Updatables = updatables
            };

            // --------------------------------------------------
            // 型キャッシュ登録: 型ごとに複数のオブジェクトをリスト化
            // --------------------------------------------------
            foreach (IUpdatable updatable in updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                // 型ごとのリストに登録
                context.Register(updatable.GetType(), updatable);
            }

            return context;
        }

        /// <summary>
        /// IContextInjectable を実装しているコンポーネントへコンテキストを注入する
        /// </summary>
        /// <param name="context">共有コンテキスト</param>
        private void InjectContext(in UpdatableContext context)
        {
            foreach (IUpdatable updatable in context.Updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                // --------------------------------------------------
                // IContextInjectable を実装しているならコンテキストを注入
                // --------------------------------------------------
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

                // フェーズ開始処理
                updatable.OnEnter();
            }
        }
    }
}