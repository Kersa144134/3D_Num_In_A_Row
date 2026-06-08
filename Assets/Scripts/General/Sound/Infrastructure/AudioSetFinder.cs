// ======================================================
// AudioSetFinder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : オーディオ設定検索クラス
// ======================================================

using System.Collections.Generic;
using SoundSystem.Domain;

namespace SoundSystem.Infrastructure
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

        /// <summary>SE タイプと配列インデックスの対応表</summary>
        private readonly Dictionary<SeType, int> _seIndexMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bgmSets">BGM セット配列</param>
        /// <param name="seSets">SE セット配列</param>
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
                        set.Source == null)
                    {
                        continue;
                    }

                    // 後勝ち登録
                    _bgmIndexMap[set.Type] = i;
                }
            }

            // SE インデックス辞書生成
            _seIndexMap = new Dictionary<SeType, int>();

            if (seSets != null)
            {
                for (int i = 0; i < seSets.Length; i++)
                {
                    SeSet set = seSets[i];

                    if (set == null)
                    {
                        continue;
                    }

                    if (set.Type == SeType.None)
                    {
                        continue;
                    }

                    // 後勝ち登録
                    _seIndexMap[set.Type] = i;
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
        /// SE インデックス取得
        /// </summary>
        /// <param name="type">検索する SE タイプ</param>
        /// <param name="index">SE 配列インデックス</param>
        public bool TryFindSeIndex(in SeType type, out int index)
        {
            index = -1;

            if (_seIndexMap == null)
            {
                return false;
            }

            return _seIndexMap.TryGetValue(type, out index);
        }
    }
}