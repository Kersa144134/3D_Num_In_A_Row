// ======================================================
// AudioClipLoader.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-08
// 概要     : AudioClip ロードクラス
// ======================================================

using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// AudioClip ロードクラス
    /// </summary>
    public sealed class AudioClipLoader
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// デコーダ登録テーブル
        /// </summary>
        private readonly Dictionary<string, IAudioClipDecoder> _decoders
            = new Dictionary<string, IAudioClipDecoder>();

        // ======================================================
        // コンストラクタ
        // ======================================================

        public AudioClipLoader()
        {
            // WAV 登録
            Register(new WavAudioClipDecoder());

            // MP3 登録
            Register(new Mp3AudioClipDecoder());
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// デコーダ登録
        /// </summary>
        public void Register(IAudioClipDecoder decoder)
        {
            // 拡張子をキーとして登録
            _decoders[decoder.Extension] = decoder;
        }

        /// <summary>
        /// AudioClip ロード
        /// </summary>
        public async UniTask<AudioClip> LoadAsync(string filePath)
        {
            // 入力チェック
            if (string.IsNullOrWhiteSpace(filePath))
            {
                Debug.LogWarning("[AudioClipLoader] filePathが無効");
                return null;
            }

            // 拡張子取得
            string extension = Path.GetExtension(filePath).ToLowerInvariant();

            // 対応デコーダ取得
            if (_decoders.TryGetValue(extension, out IAudioClipDecoder decoder))
            {
                // デコード実行
                return await decoder.DecodeAsync(filePath);
            }

            // 非対応形式
            Debug.LogWarning($"[AudioClipLoader] 非対応形式: {filePath}");

            return null;
        }
    }
}