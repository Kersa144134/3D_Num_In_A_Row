// ======================================================
// UpdatableContextFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : UpdatableContexts の生成を行うファクトリ
// ======================================================

using System;
using System.Reflection;

namespace UpdateSystem.Domain
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
        /// IUpdatable配列からコンテキストを生成する
        /// </summary>
        /// <param name="updatables">収集済みIUpdatable配列</param>
        /// <returns>生成されたコンテキスト</returns>
        public UpdatableContexts Create(IUpdatable[] updatables)
        {
            if (updatables == null)
            {
                updatables = Array.Empty<IUpdatable>();
            }

            // --------------------------------------------------
            // コンテキスト生成
            // --------------------------------------------------
            UpdatableContexts context = new UpdatableContexts();

            // 書き込み専用として扱う
            IUpdatableWriter writer = context;

            // --------------------------------------------------
            // 登録
            // --------------------------------------------------
            for (int i = 0; i < updatables.Length; i++)
            {
                IUpdatable updatable = updatables[i];

                if (updatable == null)
                {
                    continue;
                }

                // 外部定義された UpdatableType で登録する
                writer.Register(GetTypeFrom(updatables[i]), updatable);
            }

            return context;
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// Updatable に対する識別子を決定する
        /// </summary>
        private UpdatableType GetTypeFrom(IUpdatable updatable)
        {
            UpdatableBindAttribute attribute =
                updatable.GetType().GetCustomAttribute<UpdatableBindAttribute>();

            if (attribute == null)
            {
                throw new InvalidOperationException(
                    $"UpdatableBindAttribute not found: {updatable.GetType().Name}");
            }

            return attribute.Type;
        }
    }
}