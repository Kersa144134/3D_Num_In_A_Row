// ======================================================
// AudioClipMapper.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-15
// 更新日時 : 2026-06-15
// 概要     : AudioClip リストを辞書形式へ変換するクラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// AudioClip リストを辞書形式へ変換するクラス
    /// </summary>
    public sealed class AudioClipMapper
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// AudioClip リストを Dictionary へ変換する
        /// </summary>
        public Dictionary<string, AudioClip> ToDictionary(IList<AudioClip> clips)
        {
            Dictionary<string, AudioClip> map = new Dictionary<string, AudioClip>();

            foreach (AudioClip clip in clips)
            {
                if (clip == null)
                {
                    continue;
                }

                // 同名が存在する場合は上書き
                map[clip.name] = clip;
            }

            return map;
        }
    }
}