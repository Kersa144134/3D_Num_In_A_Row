// ======================================================
// AudioPlaybackEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-11
// 更新日時 : 2026-06-11
// 概要     : オーディオ再生位置更新イベントデータ
// ======================================================

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// オーディオ再生位置更新イベントデータ
    /// </summary>
    public readonly struct AudioPlaybackEvent
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 対象 BGM インデックス
        /// </summary>
        public readonly int BgmIndex;

        /// <summary>
        /// 再生位置
        /// </summary>
        public readonly int BarIndex;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bgmIndex">対象 BGM インデックス</param>
        /// <param name="barIndex">再生位置</param>
        public AudioPlaybackEvent(in int bgmIndex, in int barIndex)
        {
            BgmIndex = bgmIndex;
            BarIndex = barIndex;
        }
    }
}