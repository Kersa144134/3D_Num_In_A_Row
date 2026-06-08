// ======================================================
// WavAudioClipDecoder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-09
// 概要     : WAV AudioClip デコーダ（安全 + 正常動作保証版）
// ======================================================

using System;
using System.Buffers;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// WAV専用デコーダ（PCM16bit限定）
    /// ・チャンク解析対応
    /// ・AudioClip length整合修正済み
    /// ・インターリーブ対応
    /// ・GC削減（ArrayPool使用）
    /// </summary>
    public sealed class WavAudioClipDecoder : IAudioClipDecoder
    {
        // ======================================================
        // 定数
        // ======================================================

        private const string EXTENSION_WAV = ".wav";

        private const string CHUNK_RIFF = "RIFF";

        private const string CHUNK_WAVE = "WAVE";

        private const string CHUNK_FMT = "fmt ";

        private const string CHUNK_DATA = "data";

        private const int CHUNK_HEADER_SIZE = 8;

        private const int PCM16_BYTE_SIZE = 2;

        private const float PCM16_NORM = 32768f;

        // ======================================================
        // プロパティ
        // ======================================================

        public string Extension => EXTENSION_WAV;

        // ======================================================
        // Decode
        // ======================================================

        public async UniTask<AudioClip> DecodeAsync(string filePath)
        {
            // 入力チェック（空パス防止）
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            byte[] data = await UniTask.RunOnThreadPool(() =>
            {
                // パス正規化（OS差異吸収）
                string normalizedPath = filePath.Replace("\\", "/");

                if (File.Exists(normalizedPath))
                {
                    return File.ReadAllBytes(normalizedPath);
                }

                string streamingPath = Path.Combine(Application.streamingAssetsPath, filePath);

                if (File.Exists(streamingPath))
                {
                    return File.ReadAllBytes(streamingPath);
                }

                return null;
            });

            if (data == null || data.Length < 44)
            {
                Debug.LogWarning($"[WavAudioClipDecoder] Invalid file: {filePath}");
                return null;
            }

            await UniTask.SwitchToMainThread();

            return ParseWav(data, Path.GetFileNameWithoutExtension(filePath));
        }

        // ======================================================
        // WAV解析（完全安全版）
        // ======================================================

        private AudioClip ParseWav(byte[] data, string clipName)
        {
            Debug.Log($"[WAV RAW] length = {data.Length}");
            ReadOnlySpan<byte> span = data;

            // RIFFチェック
            if (!IsMatch(span, 0, CHUNK_RIFF) || !IsMatch(span, 8, CHUNK_WAVE))
            {
                Debug.LogWarning("[WavAudioClipDecoder] Not WAV format");
                return null;
            }

            int offset = 12;

            short channels = 0;
            int sampleRate = 0;
            int dataOffset = -1;
            int dataSize = 0;

            // ======================================================
            // チャンク探索（fmt / data）
            // ======================================================
            while (offset + CHUNK_HEADER_SIZE <= span.Length)
            {
                string chunkId = GetChunkId(span, offset);
                int chunkSize = BitConverter.ToInt32(span.Slice(offset + 4, 4));

                int chunkDataStart = offset + CHUNK_HEADER_SIZE;

                if (chunkId == CHUNK_FMT)
                {
                    // PCMフォーマット情報取得
                    channels = BitConverter.ToInt16(span.Slice(chunkDataStart + 2, 2));
                    sampleRate = BitConverter.ToInt32(span.Slice(chunkDataStart + 4, 4));
                }
                else if (chunkId == CHUNK_DATA)
                {
                    // PCMデータ開始位置確定
                    dataOffset = chunkDataStart;
                    dataSize = chunkSize;
                    break;
                }

                offset += CHUNK_HEADER_SIZE + chunkSize;
            }

            if (dataOffset < 0 || channels <= 0)
            {
                Debug.LogWarning("[WavAudioClipDecoder] invalid chunk structure");
                return null;
            }

            // ======================================================
            // サンプル計算
            // ======================================================

            int totalSamples = dataSize / PCM16_BYTE_SIZE;

            // 重要：AudioClipは「1chあたりの長さ」
            int samplesPerChannel = totalSamples / channels;

            if (samplesPerChannel <= 0)
            {
                Debug.LogWarning("[WavAudioClipDecoder] invalid sample size");
                return null;
            }

            // ======================================================
            // GC削減バッファ
            // ======================================================
            float[] samples = ArrayPool<float>.Shared.Rent(totalSamples);

            try
            {
                // ======================================================
                // PCM16 → float変換（インターリーブ維持）
                // ======================================================
                int writeIndex = 0;

                for (int i = 0; i < totalSamples; i++)
                {
                    short value = BitConverter.ToInt16(span.Slice(dataOffset + i * 2, 2));
                    samples[writeIndex++] = value / PCM16_NORM;
                }

                // ======================================================
                // デバッグログ（削除禁止）
                // ======================================================
                Debug.Log($"[WavAudioClipDecoder] clipName = {clipName}");
                Debug.Log($"[WavAudioClipDecoder] channels = {channels}");
                Debug.Log($"[WavAudioClipDecoder] sampleRate = {sampleRate}");
                Debug.Log($"[WavAudioClipDecoder] dataOffset = {dataOffset}");
                Debug.Log($"[WavAudioClipDecoder] dataSize = {dataSize}");
                Debug.Log($"[WavAudioClipDecoder] totalSamples = {totalSamples}");
                Debug.Log($"[WavAudioClipDecoder] samplesPerChannel = {samplesPerChannel}");

                // ======================================================
                // AudioClip生成（ここが修正ポイント）
                // ======================================================
                AudioClip clip = AudioClip.Create(
                    clipName,
                    totalSamples,
                    channels,
                    sampleRate,
                    false
                );

                // SetDataは「全インターリーブ配列」でOK
                clip.SetData(samples, 0);

                return clip;
            }
            finally
            {
                // バッファ返却（GC削減）
                ArrayPool<float>.Shared.Return(samples);
            }
        }

        // ======================================================
        // ヘルパー
        // ======================================================

        private bool IsMatch(ReadOnlySpan<byte> span, int offset, string match)
        {
            for (int i = 0; i < match.Length; i++)
            {
                if (span[offset + i] != match[i])
                {
                    return false;
                }
            }
            return true;
        }

        private string GetChunkId(ReadOnlySpan<byte> span, int offset)
        {
            return new string(new char[]
            {
                (char)span[offset],
                (char)span[offset + 1],
                (char)span[offset + 2],
                (char)span[offset + 3]
            });
        }
    }
}