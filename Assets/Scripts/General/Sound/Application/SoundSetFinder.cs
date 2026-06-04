// ======================================================
// SoundSetFinder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : オーディオ設定検索クラス
// ======================================================

using System.Collections.Generic;
using SoundSystem.Domain;
using SoundSystem.Infrastructure;

namespace SoundSystem.Application
{
    /// <summary>
    /// オーディオ設定検索クラス
    /// </summary>
    public sealed class AudioSetFinder
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>BGM タイプと配列インデックスの対応表</summary>
        private readonly Dictionary<BgmType, int> _bgmIndexMap;

        /// <summary>SE 辞書キャッシュ</summary>
        private readonly Dictionary<SeType, SeSet> _seMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bgmSets">BGMセット配列</param>
        /// <param name="seSets">SEセット配列</param>
        public AudioSetFinder(BgmSet[] bgmSets, SeSet[] seSets)
        {
            // BGM インデックス辞書生成
            _bgmIndexMap = new Dictionary<BgmType, int>();

            if (bgmSets != null)
            {
                for (int i = 0; i < bgmSets.Length; i++)
                {
                    BgmSet set = bgmSets[i];

                    if (set == null)
                    {
                        continue;
                    }

                    if (set.Type == BgmType.None ||
                        set.Source == null ||
                        set.Clip == null)
                    {
                        continue;
                    }

                    // 後勝ち登録
                    _bgmIndexMap[set.Type] = i;
                }
            }
            
            // SE 辞書生成
            _seMap = new Dictionary<SeType, SeSet>();

            if (seSets != null)
            {
                for (int i = 0; i < seSets.Length; i++)
                {
                    SeSet set = seSets[i];

                    if (set == null)
                    {
                        continue;
                    }

                    if (set.Type == SeType.None || set.Clip == null)
                    {
                        continue;
                    }

                    // 後勝ち登録
                    _seMap[set.Type] = set;
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// BGM インデックス取得
        /// </summary>
        /// <param name="type">検索する BGM タイプ</param>
        /// <param name="index">BGM 配列インデックス</param>
        /// <returns>取得成功時 true</returns>
        public bool TryFindBgmIndex(in BgmType type, out int index)
        {
            index = -1;

            if (_bgmIndexMap == null)
            {
                return false;
            }

            return _bgmIndexMap.TryGetValue(type, out index);
        }

        /// <summary>
        /// SE 取得
        /// </summary>
        /// <param name="type">検索するSEタイプ</param>
        /// <param name="result">取得結果</param>
        /// <returns>取得成功時 true</returns>
        public bool TryFindSeSet(in SeType type, out SeSet result)
        {
            if (_seMap == null)
            {
                result = null;
                return false;
            }

            return _seMap.TryGetValue(type, out result);
        }
    }
}