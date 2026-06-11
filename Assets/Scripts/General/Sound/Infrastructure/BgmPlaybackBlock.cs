// ======================================================
// BgmPlaybackBlock.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-11
// 更新日時 : 2026-06-11
// 概要     : BGM 再生ブロック情報
// ======================================================

using System;

using UnityEngine;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// BGM 再生ブロック情報
    /// </summary>
    [Serializable]
    public struct BgmPlaybackBlock
    {
        /// <summary>
        /// ループを有効にするか
        /// </summary>
        [Tooltip("ループを有効にするか")]
        public bool IsLoop;

        /// <summary>
        /// ブロック開始小節
        /// </summary>
        [Min(1)]
        [Tooltip("ブロック開始小節")]
        public int StartBar;

        /// <summary>
        /// ループ開始小節
        /// </summary>
        [Min(1)]
        [Tooltip("ループ開始小節")]
        public int LoopStartBar;

        /// <summary>
        /// ループ終了小節
        /// </summary>
        [Min(1)]
        [Tooltip("ループ終了小節")]
        public int LoopEndBar;
    }
}