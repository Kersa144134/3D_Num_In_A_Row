// ======================================================
// AudioClipRepository.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-15
// 概要     : AudioClip 管理クラス
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// AudioClip 管理クラス
    /// </summary>
    public sealed class AudioClipRepository
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// AudioClip ローダー
        /// </summary>
        private readonly AudioClipLoader _audioClipLoader = new AudioClipLoader();

        /// <summary>
        /// AudioClip マッパー
        /// </summary>
        private readonly AudioClipMapper _audioClipMapper = new AudioClipMapper();

        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// ラベルごとの AudioClip キャッシュテーブル
        /// </summary>
        private readonly Dictionary<AudioLabelType, Dictionary<string, AudioClip>> _audioClipMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AudioClipRepository()
        {
            _audioClipMap = new Dictionary<AudioLabelType, Dictionary<string, AudioClip>>();
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
            await LoadByLabelAsync(AudioLabelType.BGM);
        }

        /// <summary>
        /// 全 SE ロード
        /// </summary>
        public async UniTask LoadSeAsync()
        {
            await LoadByLabelAsync(AudioLabelType.SE);
        }

        // --------------------------------------------------
        // 取得
        // --------------------------------------------------
        /// <summary>
        /// AudioClip 取得
        /// </summary>
        public bool TryGetClip(
            in AudioLabelType labelType,
            in string clipName,
            out AudioClip clip)
        {
            clip = null;

            // ラベルが存在しない場合は失敗
            if (!_audioClipMap.TryGetValue(labelType, out Dictionary<string, AudioClip> map))
            {
                return false;
            }

            return map.TryGetValue(clipName, out clip);
        }

        // --------------------------------------------------
        // 解放
        // --------------------------------------------------
        /// <summary>
        /// Addressables でロードした指定ラベルの AudioClip を解放
        /// </summary>
        public void Release(in AudioLabelType labelType)
        {
            _audioClipLoader.Release(labelType);
        }

        /// <summary>
        /// Addressables でロードした全ラベルの AudioClip を解放
        /// </summary>
        public void ReleaseAll()
        {
            _audioClipLoader.ReleaseAll();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ラベル単位で AudioClip を辞書へ登録する
        /// </summary>
        private async UniTask LoadByLabelAsync(AudioLabelType labelType)
        {
            // ラベルに対応する AudioClip 一覧を取得
            IList<AudioClip> clips = await _audioClipLoader.LoadByLabelAsync(labelType);

            // 取得結果を辞書へ変換
            Dictionary<string, AudioClip> map = _audioClipMapper.ToDictionary(clips);

            _audioClipMap[labelType] = map;
        }
    }
}