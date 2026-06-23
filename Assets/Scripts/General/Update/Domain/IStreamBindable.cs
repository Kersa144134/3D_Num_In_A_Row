// ======================================================
// IStreamBindable.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-23
// 更新日時 : 2026-06-23
// 概要     : ストリームの購読開始および購読解除を行うインターフェース
// ======================================================

namespace UpdateSystem.Domain
{
    /// <summary>
    /// ストリームの購読開始および購読解除を行うインターフェース
    /// </summary>
    public interface IStreamBindable
    {
        /// <summary>
        /// イベントストリームの購読を開始する
        /// </summary>
        void BindStreams();

        /// <summary>
        /// イベントストリームの購読を解除する
        /// </summary>
        void UnbindStreams();
    }
}