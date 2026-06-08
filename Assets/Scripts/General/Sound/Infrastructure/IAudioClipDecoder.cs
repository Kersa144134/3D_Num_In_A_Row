// ======================================================
// IAudioClipDecoder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-08
// 概要     : AudioClip デコーダインターフェース
// ======================================================

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// AudioClip変換インターフェース
    /// </summary>
    public interface IAudioClipDecoder
    {
        /// <summary>
        /// 対応拡張子
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// AudioClipへ変換
        /// </summary>
        UniTask<AudioClip> DecodeAsync(string filePath);
    }
}