// ======================================================
// AudioBarEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-11
// 更新日時 : 2026-06-11
// 概要     : BGM 小節単位イベントデータ
// ======================================================

namespace SoundSystem.Application
{
    /// <summary>
    /// BGM の小節進行を通知するイベントデータ
    /// </summary>
    public readonly struct AudioBarEvent
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>対象 BGM インデックス</summary>
        private readonly int _bgmIndex;

        /// <summary>現在の小節番号</summary>
        private readonly int _barIndex;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 対象 BGM インデックス
        /// </summary>
        public int BgmIndex => _bgmIndex;

        /// <summary>
        /// 現在の小節番号
        /// </summary>
        public int BarIndex => _barIndex;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bgmIndex">対象 BGM インデックス</param>
        /// <param name="barIndex">現在の小節番号</param>
        public AudioBarEvent(in int bgmIndex, in int barIndex)
        {
            _bgmIndex = bgmIndex;
            _barIndex = barIndex;
        }
    }
}