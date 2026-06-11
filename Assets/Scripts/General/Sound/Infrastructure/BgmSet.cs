// ======================================================
// BgmSet.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : BGM 情報セット
// ======================================================

using System;
using UnityEngine;

using SoundSystem.Domain;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// BGM 情報セット
    /// </summary>
    [Serializable]
    public sealed class BgmSet
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>BGM タイプ</summary>
        [SerializeField]
        [Tooltip("BGM タイプ")]
        private BgmType _type;

        /// <summary>AudioSource</summary>
        [SerializeField]
        [Tooltip("AudioSource")]
        private AudioSource _source;

        /// <summary>BPM</summary>
        [SerializeField, Min(1f)]
        [Tooltip("BPM")]
        private float _bpm = 120f;

        /// <summary>BGM 基準再生位置（秒）</summary>
        [SerializeField, Min(0f)]
        [Tooltip("BGM 基準再生位置（秒）")]
        private float _offset;

        /// <summary>拍子：分子</summary>
        [SerializeField, Min(1f)]
        [Tooltip("拍子の分子")]
        private int _timeSignatureNumerator = 4;

        /// <summary>拍子：分母</summary>
        [SerializeField, Min(1f)]
        [Tooltip("拍子の分母")]
        private int _timeSignatureDenominator = 4;

        /// <summary>再生ブロック配列</summary>
        [SerializeField]
        [Tooltip("BGM 再生ブロック")]
        private BgmPlaybackBlock[] _playbackBlocks;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>BGM タイプ</summary>
        public BgmType Type => _type;
        
        /// <summary>AudioSource</summary>
        public AudioSource Source => _source;

        /// <summary>BPM</summary>
        public float Bpm => _bpm;

        /// <summary>BGM 基準再生位置（秒）</summary>
        public float Offset => _offset;

        /// <summary>拍子：分子</summary>
        public int TimeSignatureNumerator => _timeSignatureNumerator;

        /// <summary>拍子：分母</summary>
        public int TimeSignatureDenominator => _timeSignatureDenominator;

        /// <summary>再生ブロック</summary>
        public BgmPlaybackBlock[] PlaybackBlocks => _playbackBlocks;

        // ======================================================
        // 計算プロパティ
        // ======================================================

        /// <summary>
        /// 1 拍あたりの秒数
        /// </summary>
        public float SecondsPerBeat => 60f / Mathf.Max(_bpm, 1f);

        /// <summary>
        /// 1 小節あたりの秒数
        /// </summary>
        public float SecondsPerBar
        {
            get
            {
                // 拍子分母を 4 分音符基準へ正規化
                float beatUnit = 4f / Mathf.Max(_timeSignatureDenominator, 1);

                // 小節内の拍数（4分音符換算）
                float beatsPerBar = _timeSignatureNumerator * beatUnit;

                // 拍数 × 1 拍時間
                return beatsPerBar * SecondsPerBeat;
            }
        }
    }
}