// ======================================================
// UpdatableContextFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : UpdatableContexts の生成を行うファクトリ
// ======================================================

using System;

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

            return context;
        }
    }
}