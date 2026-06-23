// ======================================================
// IStreamRunnerModifier.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-23
// 更新日時 : 2026-06-23
// 概要     : Stream 登録内容変更インターフェース
// ======================================================

using UpdateSystem.Domain;

namespace StreamSystem.Domain
{
    /// <summary>
    /// Stream の登録内容を変更する
    /// </summary>
    public interface IStreamRunnerModifier
    {
        /// <summary>
        /// Stream 配列を差し替える
        /// </summary>
        /// <param name="streamBindables">差し替え対象配列</param>
        void Replace(in IStreamBindable[] streamBindables);
    }
}