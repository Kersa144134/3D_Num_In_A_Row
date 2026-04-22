// ======================================================
// UpdatableContextFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : UpdatableContexts の生成を行うファクトリ
// ======================================================

using System;

namespace SceneSystem.Domain
{
    /// <summary>
    /// UpdatableContexts 生成クラス
    /// </summary>
    public class UpdatableContextFactory
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// IUpdatable 配列からコンテキストを生成する
        /// </summary>
        /// <param name="updatables">収集済み IUpdatable 配列</param>
        /// <returns>生成されたコンテキスト</returns>
        public UpdatableContexts Create(in IUpdatable[] updatables)
        {
            if (updatables == null)
            {
                return new UpdatableContexts(Array.Empty<IUpdatable>());
            }

            // コンテキスト生成
            UpdatableContexts context = new UpdatableContexts(updatables);

            // 登録専用として扱う
            IUpdatableWriter writer = context;

            // 型ごとに登録
            foreach (IUpdatable updatable in updatables)
            {
                if (updatable == null)
                {
                    continue;
                }

                writer.Register(updatable.GetType(), updatable);
            }

            return context;
        }
    }
}