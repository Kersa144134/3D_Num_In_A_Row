// ======================================================
// SeSet.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : SE 情報セット
// ======================================================

using System;
using UnityEngine;

using SoundSystem.Domain;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// SE 情報セット
    /// </summary>
    [Serializable]
    public sealed class SeSet
    {
        /// <summary>SE タイプ</summary>
        public SeType Type;

        /// <summary>SE クリップ</summary>
        public AudioClip Clip;
    }
}