// ======================================================
// IStreamRunner.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-23
// 更新日時 : 2026-06-23
// 概要     : Stream 実行インターフェース
// ======================================================

namespace StreamSystem.Domain
{
    /// <summary>
    /// Stream の購読開始・解除を実行する
    /// </summary>
    public interface IStreamRunner
    {
        /// <summary>
        /// 登録済み Stream の購読を開始する
        /// </summary>
        void BindStreams();

        /// <summary>
        /// 登録済み Stream の購読を解除する
        /// </summary>
        void UnbindStreams();
    }
}