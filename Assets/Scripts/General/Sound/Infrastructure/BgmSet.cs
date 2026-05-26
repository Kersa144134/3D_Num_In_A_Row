// ======================================================
// BgmSet.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : BGM 情報セット
// ======================================================

using System;
using UnityEngine;

using SoundSystem.Domain;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// BGM 情報セット
    /// </summary>
    [Serializable]
    public sealed class BgmSet
    {
        /// <summary>BGM タイプ</summary>
        public BgmType Type;

        /// <summary>BGM オーディオソース</summary>
        public AudioSource Source;

        /// <summary>BGM クリップ</summary>
        public AudioClip Clip;
    }
}