// ======================================================
// StreamRunner.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-23
// 更新日時 : 2026-06-23
// 概要     : 指定された IStreamBindable を保持し、
//            イベント購読の開始・解除を実行するランナー
// ======================================================

using System;
using StreamSystem.Domain;
using UpdateSystem.Domain;

namespace StreamSystem.Application
{
    /// <summary>
    /// IStreamBindable を保持し、イベント購読の開始・解除を実行するランナー
    /// </summary>
    public sealed class StreamRunner : IStreamRunner, IStreamRunnerModifier
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>購読対象のキャッシュ配列</summary>
        private IStreamBindable[] _streamBindables = Array.Empty<IStreamBindable>();

        // ======================================================
        // IStreamRunner 実装
        // ======================================================

        /// <summary>
        /// 登録済み Stream の購読を開始する
        /// </summary>
        public void BindStreams()
        {
            for (int i = 0; i < _streamBindables.Length; i++)
            {
                // イベント購読開始
                _streamBindables[i]?.BindStreams();
            }
        }

        /// <summary>
        /// 登録済み Stream の購読を解除する
        /// </summary>
        public void UnbindStreams()
        {
            for (int i = 0; i < _streamBindables.Length; i++)
            {
                // イベント購読解除
                _streamBindables[i]?.UnbindStreams();
            }
        }

        // ======================================================
        // IStreamRunnerModifier 実装
        // ======================================================

        /// <summary>
        /// Stream 配列を差し替える
        /// </summary>
        /// <param name="streamBindables">差し替え対象配列</param>
        void IStreamRunnerModifier.Replace(in IStreamBindable[] streamBindables)
        {
            if (streamBindables == null)
            {
                _streamBindables = Array.Empty<IStreamBindable>();

                return;
            }

            _streamBindables = streamBindables;
        }
    }
}