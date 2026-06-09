// ======================================================
// AudioClipLoader.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-09
// 概要     : Addressables ベース AudioClip ローダー
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// Addressables から AudioClip をロードするクラス
    /// </summary>
    public sealed class AudioClipLoader
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // BGM
        // --------------------------------------------------
        /// <summary>
        /// BGM AudioClip を非同期ロード
        /// </summary>
        public UniTask<AudioClip> LoadBgmAsync(string key)
        {
            return LoadInternalAsync(key);
        }

        // --------------------------------------------------
        // SE
        // --------------------------------------------------
        /// <summary>
        /// SE AudioClip を非同期ロード
        /// </summary>
        public UniTask<AudioClip> LoadSeAsync(string key)
        {
            return LoadInternalAsync(key);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 共通ロード処理
        /// </summary>
        private async UniTask<AudioClip> LoadInternalAsync(string key)
        {
            // --------------------------------------------------
            // 入力チェック
            // --------------------------------------------------
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("[AudioClipLoader] keyが無効");
                return null;
            }

            // --------------------------------------------------
            // 存在チェック
            // --------------------------------------------------
            AsyncOperationHandle<IList<IResourceLocation>> locationHandle =
                Addressables.LoadResourceLocationsAsync(key);

            await locationHandle;

            if (locationHandle.Result == null || locationHandle.Result.Count == 0)
            {
                Debug.LogWarning($"[AudioClipLoader] 存在しないキー: {key}");

                return null;
            }

            // --------------------------------------------------
            // ロード処理
            // --------------------------------------------------
            AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(key);

            await handle;

            // --------------------------------------------------
            // 成功判定
            // --------------------------------------------------
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[AudioClipLoader] ロード失敗: {key}");

                return null;
            }

            return handle.Result;
        }
    }
}