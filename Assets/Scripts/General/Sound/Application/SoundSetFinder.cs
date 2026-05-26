// ======================================================
// SoundSetFinder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : サウンドセット検索クラス
// ======================================================

using System.Collections.Generic;
using SoundSystem.Domain;
using SoundSystem.Infrastructure;

namespace SoundSystem.Application
{
    /// <summary>
    /// サウンドセット検索クラス
    /// </summary>
    public sealed class SoundSetFinder
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>BGM辞書キャッシュ</summary>
        private readonly Dictionary<BgmType, BgmSet> _bgmMap;

        /// <summary>SE辞書キャッシュ</summary>
        private readonly Dictionary<SeType, SeSet> _seMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタで辞書キャッシュを生成する
        /// </summary>
        /// <param name="bgmSets">BGMセット配列</param>
        /// <param name="seSets">SEセット配列</param>
        public SoundSetFinder(BgmSet[] bgmSets, SeSet[] seSets)
        {
            // BGM 辞書生成
            _bgmMap = new Dictionary<BgmType, BgmSet>();

            if (bgmSets != null)
            {
                for (int i = 0; i < bgmSets.Length; i++)
                {
                    BgmSet set = bgmSets[i];

                    if (set == null)
                    {
                        continue;
                    }

                    if (set.Type == BgmType.None || set.Source == null || set.Clip == null)
                    {
                        continue;
                    }

                    // 後勝ち登録
                    _bgmMap[set.Type] = set;
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
        /// BGM 取得
        /// </summary>
        /// <param name="type">検索するBGMタイプ</param>
        /// <param name="result">取得結果</param>
        /// <returns>取得成功時 true</returns>
        public bool TryFindBgmSet(in BgmType type, out BgmSet result)
        {
            if (_bgmMap == null)
            {
                result = null;

                return false;
            }

            return _bgmMap.TryGetValue(type, out result);
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