// ======================================================
// Mp3AudioClipDecoder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-08
// 概要     : MP3 AudioClip デコーダ
// ======================================================

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// MP3 専用デコーダ
    /// </summary>
    public sealed class Mp3AudioClipDecoder : IAudioClipDecoder
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// MP3 拡張子
        /// </summary>
        private const string EXTENSION_WAV = ".mp3";

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
        /// MP3 ファイルを AudioClip へ変換する
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
            string uri =
                filePath.StartsWith("http") || filePath.StartsWith("file://")
                    ? filePath
                    : "file://" + filePath;

            // Unity標準の AudioClip 取得APIを使用
            using (UnityWebRequest request =
                   UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG))
            {
                // 非同期通信を実行し完了まで待機
                await request.SendWebRequest().ToUniTask();

                // 通信結果判定
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"MP3 decode error : {request.error}");

                    return null;
                }

                // AudioClip を取得
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

                Debug.Log(clip);
                return clip;
            }
        }
    }
}