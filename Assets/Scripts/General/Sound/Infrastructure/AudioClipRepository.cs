// ======================================================
// AudioClipRepository.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-09
// 概要     : AudioClip 管理クラス
// ======================================================

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SoundSystem.Domain;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// AudioClip 管理クラス
    /// </summary>
    public sealed class AudioClipRepository
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>BGM Addressables ルートキー</summary>
        private const string BGM_ROOT_KEY = "BGM/";

        /// <summary>SE Addressables ルートキー</summary>
        private const string SE_ROOT_KEY = "SE/";

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>AudioClip ローダー</summary>
        private readonly AudioClipLoader _audioClipLoader;

        /// <summary>ロード済み BGM の AudioClip を保持するテーブル</summary>
        private readonly Dictionary<BgmType, AudioClip> _bgmClipMap;

        /// <summary>ロード済み SE の AudioClip を保持するテーブル</summary>
        private readonly Dictionary<SeType, AudioClip> _seClipMap;

        /// <summary>BgmType → Addressables キー文字列の変換結果を保持するテーブル</summary>
        private readonly Dictionary<BgmType, string> _bgmKeyMap;

        /// <summary>SeType → Addressables キー文字列の変換結果を保持するテーブル</summary>
        private readonly Dictionary<SeType, string> _seKeyMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AudioClipRepository()
        {
            _audioClipLoader = new AudioClipLoader();

            _bgmClipMap = new Dictionary<BgmType, AudioClip>();
            _seClipMap = new Dictionary<SeType, AudioClip>();

            _bgmKeyMap = new Dictionary<BgmType, string>();
            _seKeyMap = new Dictionary<SeType, string>();

            InitializeKeyCache();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // ロード
        // --------------------------------------------------
        /// <summary>
        /// 全 BGM ロード
        /// </summary>
        public async UniTask LoadBgmAsync()
        {
            List<UniTask> tasks = new List<UniTask>(_bgmKeyMap.Count);

            foreach (KeyValuePair<BgmType, string> pair in _bgmKeyMap)
            {
                tasks.Add(LoadBgmClipAsync(pair.Key, pair.Value));
            }

            await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// 全 SE ロード
        /// </summary>
        public async UniTask LoadSeAsync()
        {
            List<UniTask> tasks = new List<UniTask>(_seKeyMap.Count);

            foreach (KeyValuePair<SeType, string> pair in _seKeyMap)
            {
                tasks.Add(LoadSeClipAsync(pair.Key, pair.Value));
            }

            await UniTask.WhenAll(tasks);
        }

        // --------------------------------------------------
        // 取得
        // --------------------------------------------------
        /// <summary>
        /// BGM 取得
        /// </summary>
        public bool TryGetBgmClip(BgmType type, out AudioClip clip)
        {
            return _bgmClipMap.TryGetValue(type, out clip);
        }

        /// <summary>
        /// SE 取得
        /// </summary>
        public bool TryGetSeClip(SeType type, out AudioClip clip)
        {
            return _seClipMap.TryGetValue(type, out clip);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// Enum → Addressables キー変換キャッシュ生成
        /// </summary>
        private void InitializeKeyCache()
        {
            // --------------------------------------------------
            // BGMキー生成
            // --------------------------------------------------
            foreach (BgmType type in Enum.GetValues(typeof(BgmType)))
            {
                if (type == BgmType.None)
                {
                    continue;
                }

                _bgmKeyMap[type] = BGM_ROOT_KEY + type;
            }

            // --------------------------------------------------
            // SEキー生成
            // --------------------------------------------------
            foreach (SeType type in Enum.GetValues(typeof(SeType)))
            {
                if (type == SeType.None)
                {
                    continue;
                }

                _seKeyMap[type] = SE_ROOT_KEY + type;
            }
        }

        /// <summary>
        /// BGM 単体ロード
        /// </summary>
        private async UniTask LoadBgmClipAsync(BgmType type, string key)
        {
            AudioClip clip = await _audioClipLoader.LoadBgmAsync(key);

            if (clip == null)
            {
                return;
            }

            _bgmClipMap[type] = clip;
        }

        /// <summary>
        /// SE 単体ロード
        /// </summary>
        private async UniTask LoadSeClipAsync(SeType type, string key)
        {
            AudioClip clip = await _audioClipLoader.LoadSeAsync(key);

            if (clip == null)
            {
                return;
            }

            _seClipMap[type] = clip;
        }
    }
}