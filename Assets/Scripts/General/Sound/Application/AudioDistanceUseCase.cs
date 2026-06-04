// ======================================================
// AudioDistanceUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-04
// 更新日時 : 2026-06-04
// 概要     : オーディオの距離計算を管理するユースケース
// ======================================================

using UnityEngine;

namespace SoundSystem.Application
{
    /// <summary>
    /// オーディオ距離計算ユースケース
    /// </summary>
    public sealed class AudioDistanceUseCase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>リスナー Transform</summary>
        private Transform _listenerTransform;

        /// <summary>音量が最大になる最大音量距離</summary>
        private readonly float _minDistance;

        /// <summary>音声が再生される最大再生距離</summary>
        private readonly float _maxDistance;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="minDistance">最大音量距離</param>
        /// <param name="maxDistance">最大再生距離</param>
        public AudioDistanceUseCase(in float minDistance, in float maxDistance)
        {
            _minDistance = minDistance;
            _maxDistance = maxDistance;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // リスナー
        // --------------------------------------------------
        /// <summary>
        /// リスナー Transform 設定
        /// </summary>
        /// <param name="listener">
        /// リスナーTransform
        /// </param>
        public void SetListenerTransform(in Transform listener)
        {
            _listenerTransform = listener;
        }

        /// <summary>
        /// リスナー Transform 解除
        /// </summary>
        public void ResetListenerTransform()
        {
            _listenerTransform = null;
        }

        /// <summary>
        /// 再生可能距離内か判定する
        /// </summary>
        /// <param name="audioPosition">音源座標</param>
        /// <returns>true: 再生可能</returns>
        public bool IsAudible(in Vector3 audioPosition)
        {
            // リスナーとの距離算出
            float distance = CalculateDistance(audioPosition);

            // 可聴判定
            return distance <= _maxDistance;
        }

        // --------------------------------------------------
        // 音量
        // --------------------------------------------------
        /// <summary>
        /// 音量を算出する
        /// </summary>
        /// <param name="audioPosition">音源座標</param>
        /// <returns>音量</returns>
        public float CalculateVolume(in Vector3 audioPosition)
        {
            // リスナーとの距離算出
            float distance = CalculateDistance(audioPosition);

            // 最小距離以内
            if (distance <= _minDistance)
            {
                return 1f;
            }

            // 最大距離以上
            if (distance >= _maxDistance)
            {
                return 0f;
            }

            // 距離を正規化
            float normalizedDistance = Mathf.InverseLerp(
                _minDistance,
                _maxDistance,
                distance);

            // 音量を反転
            float volume = 1f - normalizedDistance;

            return volume;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // リスナー
        // --------------------------------------------------
        /// <summary>
        /// リスナーとの距離を取得する
        /// </summary>
        /// <param name="audioPosition">音源座標</param>
        /// <returns>距離</returns>
        private float CalculateDistance(in Vector3 audioPosition)
        {
            return Vector3.Distance(_listenerTransform.position, audioPosition);
        }
    }
}