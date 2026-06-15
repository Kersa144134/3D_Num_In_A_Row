// ======================================================
// AudioLabelType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-15
// 更新日時 : 2026-06-15
// 概要     : AudioClip ラベル種別
// ======================================================

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// AudioClip ラベル種別
    /// 種別名を Addressables のラベル名として扱う
    /// </summary>
    public enum AudioLabelType
    {
        /// <summary>BGM ラベル</summary>
        BGM,

        /// <summary>SE ラベル</summary>
        SE,
    }
}