// ======================================================
// WavAudioClipDecoder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-08
// 概要     : WAV AudioClip デコーダ
// ======================================================

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// WAV 専用デコーダ
    /// </summary>
    public sealed class WavAudioClipDecoder : IAudioClipDecoder
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// WAV 拡張子
        /// </summary>
        private const string EXTENSION_WAV = ".wav";

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// このデコーダが対応する拡張子
        /// </summary>
        public string Extension => EXTENSION_WAV;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// WAV ファイルを AudioClip へ変換する
        /// StreamingAssets / ローカル / URL いずれにも対応
        /// </summary>
        /// <returns>生成された AudioClip</returns>
        public async UniTask<AudioClip> DecodeAsync(string filePath)
        {
            // 入力パスチェック
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            // UnityWebRequest 用 URI を生成
            string uri = filePath.Replace("\\", "/");

            if (!uri.Contains("://"))
            {
                uri = "file:///" + uri;
            }

            // Unity標準の AudioClip 取得APIを使用
            using (UnityWebRequest request =
                   UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV))
            {
                // 非同期通信を実行し完了まで待機
                await request.SendWebRequest().ToUniTask();

                // 通信結果の判定
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"WAV decode error : {request.error}");

                    return null;
                }

                // AudioClip 取得
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

                Debug.Log($"URI : {uri}");
                Debug.Log($"Result : {request.result}");
                Debug.Log($"Error  : {request.error}");
                Debug.Log(clip);
                return clip;
            }
        }
    }
}