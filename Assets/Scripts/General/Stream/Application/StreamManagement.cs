// ======================================================
// StreamManagement.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-23
// 更新日時 : 2026-06-23
// 概要     : IStreamBindable を管理するサービス
// ======================================================

using System;
using StreamSystem.Domain;
using UpdateSystem.Domain;

namespace StreamSystem.Application
{
    /// <summary>
    /// IStreamBindable を管理するサービス
    /// </summary>
    public sealed class StreamManagement
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>Stream 実行専用インターフェース</summary>
        private readonly IStreamRunner _runner;

        /// <summary>Stream 変更専用インターフェース</summary>
        private readonly IStreamRunnerModifier _modifier;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// StreamManagement を生成する
        /// </summary>
        public StreamManagement()
        {
            StreamRunner runner = new StreamRunner();

            _runner = runner;
            _modifier = runner;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 登録済み Stream の購読を開始する
        /// </summary>
        public void BindStreams()
        {
            _runner.BindStreams();
        }

        /// <summary>
        /// 登録済み Stream の購読を解除する
        /// </summary>
        public void UnbindStreams()
        {
            _runner.UnbindStreams();
        }

        /// <summary>
        /// Stream の登録内容を再構築する
        /// </summary>
        /// <param name="streamBindables">登録対象 Stream 配列</param>
        public void RebuildStreams(in IStreamBindable[] streamBindables)
        {
            // null の場合は空配列として扱う
            IStreamBindable[] safeArray =
                streamBindables ?? Array.Empty<IStreamBindable>();

            IStreamBindable[] buffer = new IStreamBindable[safeArray.Length];

            // 有効要素数
            int index = 0;

            for (int i = 0; i < safeArray.Length; i++)
            {
                // 対象取得
                IStreamBindable streamBindable = safeArray[i];

                if (streamBindable == null)
                {
                    continue;
                }

                // バッファへ格納
                buffer[index] = streamBindable;

                index++;
            }

            // 有効要素数に合わせた配列生成
            IStreamBindable[] result =
                new IStreamBindable[index];

            for (int i = 0; i < index; i++)
            {
                result[i] = buffer[i];
            }

            // ランナーへ反映
            _modifier.Replace(result);
        }
    }
}