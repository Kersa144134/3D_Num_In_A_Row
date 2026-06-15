// ======================================================
// AudioClipLoader.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-15
// 概要     : Addressables ベース AudioClip ローダー
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// Addressables から AudioClip をロードするクラス
    /// </summary>
    public sealed class AudioClipLoader
    {
        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// ラベルごとのロードハンドル管理テーブル
        /// </summary>
        private readonly Dictionary<AudioLabelType, AsyncOperationHandle<IList<AudioClip>>> _handleMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AudioClipLoader()
        {
            _handleMap = new Dictionary<AudioLabelType, AsyncOperationHandle<IList<AudioClip>>>();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // ロード
        // --------------------------------------------------
        /// <summary>
        /// 指定ラベルの AudioClip を一括ロード
        /// </summary>
        public async UniTask<IList<AudioClip>> LoadByLabelAsync(AudioLabelType labelType)
        {
            // Addressables ロード用のラベル文字列へ変換
            string label = labelType.ToString();

            // 既にロード済みならキャッシュを返却
            if (_handleMap.TryGetValue(labelType, out AsyncOperationHandle<IList<AudioClip>> cachedHandle))
            {
                return cachedHandle.Result;
            }

            // Addressables からロード開始
            AsyncOperationHandle<IList<AudioClip>> handle = Addressables.LoadAssetsAsync<AudioClip>(label, null);

            // ロード完了待機
            await handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogWarning($"[AudioClipLoader] ロード失敗 : {label}");

                return null;
            }

            _handleMap[labelType] = handle;

            return handle.Result;
        }
        
        // --------------------------------------------------
        // 解放
        // --------------------------------------------------
        /// <summary>
        /// Addressables でロードした指定ラベルの AudioClip を解放
        /// </summary>
        public void Release(in AudioLabelType labelType)
        {
            // ハンドル取得失敗の場合処理なし
            if (!_handleMap.TryGetValue(labelType, out AsyncOperationHandle<IList<AudioClip>> handle))
            {
                return;
            }

            // Addressables 解放
            Addressables.Release(handle);

            // テーブルから削除
            _handleMap.Remove(labelType);
        }

        /// <summary>
        /// Addressables でロードした全ラベルの AudioClip を解放
        /// </summary>
        public void ReleaseAll()
        {
            foreach (AsyncOperationHandle<IList<AudioClip>> handle in _handleMap.Values)
            {
                Addressables.Release(handle);
            }

            // テーブルリセット
            _handleMap.Clear();
        }
    }
}