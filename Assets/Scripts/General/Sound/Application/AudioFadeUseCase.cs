// ======================================================
// AudioFadeUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-04
// 更新日時 : 2026-06-04
// 概要     : AudioSource 音量フェードおよび
//            AudioLowPassFilter 周波数フェードを管理する
//            ユースケースクラス
// ======================================================

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SoundSystem.Application
{
    /// <summary>
    /// Audio フェード処理ユースケース
    /// </summary>
    public sealed class AudioFadeUseCase : IDisposable
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>BGM 音量フェード制御用 CancellationTokenSource 配列</summary>
        private readonly CancellationTokenSource[] _bgmVolumeCancellationArray;

        /// <summary>ローパスフェード制御用 CancellationTokenSource</summary>
        private CancellationTokenSource _lowPassCancellationTokenSource;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bgmCount">
        /// BGM 数
        /// </param>
        public AudioFadeUseCase(in int bgmCount)
        {
            // BGMフェード管理配列生成
            _bgmVolumeCancellationArray = new CancellationTokenSource[bgmCount];
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// BGM 音量フェード開始
        /// </summary>
        /// <param name="bgmIndex">BGM インデックス</param>
        /// <param name="audioSource">対象 AudioSource</param>
        /// <param name="targetVolume">目標音量</param>
        /// <param name="duration">補間時間</param>
        public void StartVolumeFade(
            in int bgmIndex,
            in AudioSource audioSource,
            in float targetVolume,
            in float duration)
        {
            if (audioSource == null)
            {
                return;
            }

            // 補間時間が 0 以下の場合は即時反映
            if (duration <= 0f)
            {
                // 既存フェード停止
                _bgmVolumeCancellationArray[bgmIndex]?.Cancel();

                // 音量即時反映
                audioSource.volume = targetVolume;

                return;
            }

            // フェード開始
            FadeVolumeAsync(
                audioSource,
                bgmIndex,
                targetVolume,
                duration).Forget();
        }

        /// <summary>
        /// ローパスフェード開始
        /// </summary>
        /// <param name="filters">対象フィルター配列</param>
        /// <param name="targetFrequency">目標周波数</param>
        /// <param name="duration">補間時間</param>
        public void StartLowPassFade(
            in AudioLowPassFilter[] filters,
            in float targetFrequency,
            in float duration)
        {
            if (filters == null)
            {
                return;
            }

            // 即時反映
            if (duration <= 0f)
            {
                // 全フィルターへ反映
                ApplyLowPassFrequency(filters, targetFrequency);

                return;
            }

            // フェード開始
            FadeLowPassAsync(
                filters,
                targetFrequency,
                duration).Forget();
        }

        /// <summary>
        /// 指定 BGM のフェード停止
        /// </summary>
        /// <param name="bgmIndex">BGM インデックス</param>
        public void StopVolumeFade(in int bgmIndex)
        {
            _bgmVolumeCancellationArray[bgmIndex]?.Cancel();
        }

        /// <summary>
        /// ローパスフェード停止
        /// </summary>
        public void StopLowPassFade()
        {
            _lowPassCancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _bgmVolumeCancellationArray.Length; i++)
            {
                // BGM フェード停止
                _bgmVolumeCancellationArray[i]?.Cancel();

                // BGM リソース解放
                _bgmVolumeCancellationArray[i]?.Dispose();
            }

            // ローパスフェード停止
            _lowPassCancellationTokenSource?.Cancel();

            // ローパスリソース解放
            _lowPassCancellationTokenSource?.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // BGM フェード
        // --------------------------------------------------
        /// <summary>
        /// 音量フェード処理
        /// </summary>
        /// <param name="audioSource">対象 AudioSource</param>
        /// <param name="bgmIndex">BGM インデックス</param>
        /// <param name="targetVolume">目標音量</param>
        /// <param name="duration">補間時間</param>
        private async UniTask FadeVolumeAsync(
            AudioSource audioSource,
            int bgmIndex,
            float targetVolume,
            float duration)
        {
            // 既存フェード停止
            _bgmVolumeCancellationArray[bgmIndex]?.Cancel();

            // 既存リソース解放
            _bgmVolumeCancellationArray[bgmIndex]?.Dispose();

            // 新規 CancellationTokenSource 生成
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            // 配列へ保存
            _bgmVolumeCancellationArray[bgmIndex] = cancellationTokenSource;

            // CancellationToken 取得
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            // 現在音量を開始値に設定
            float startVolume = audioSource.volume;

            // 経過時間
            float elapsedTime = 0f;

            try
            {
                // フェード完了までループ
                while (elapsedTime < duration)
                {
                    // キャンセル確認
                    cancellationToken.ThrowIfCancellationRequested();

                    // 経過時間加算
                    elapsedTime += Time.unscaledDeltaTime;

                    // 補間率算出
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    // 音量補間
                    audioSource.volume = Mathf.Lerp(
                        startVolume,
                        targetVolume,
                        t);

                    // 次フレーム待機
                    await UniTask.Yield(
                        PlayerLoopTiming.Update,
                        cancellationToken);
                }

                // 最終値保証
                audioSource.volume = targetVolume;
            }
            catch (OperationCanceledException) { }
        }

        // --------------------------------------------------
        // ローパスフェード
        // --------------------------------------------------

        /// <summary>
        /// ローパスフェード処理
        /// </summary>
        /// <param name="filters">対象フィルター配列</param>
        /// <param name="targetFrequency">目標周波数</param>
        /// <param name="duration">補間時間</param>
        private async UniTask FadeLowPassAsync(
            AudioLowPassFilter[] filters,
            float targetFrequency,
            float duration)
        {
            // 既存フェード停止
            _lowPassCancellationTokenSource?.Cancel();

            // 既存リソース解放
            _lowPassCancellationTokenSource?.Dispose();

            // 新規 CancellationTokenSource 生成
            _lowPassCancellationTokenSource = new CancellationTokenSource();

            // CancellationToken 取得
            CancellationToken cancellationToken = _lowPassCancellationTokenSource.Token;

            // 開始周波数
            float startFrequency = targetFrequency;

            // 有効なフィルターから現在の周波数取得
            for (int i = 0; i < filters.Length; i++)
            {
                if (filters[i] == null)
                {
                    continue;
                }

                startFrequency = filters[i].cutoffFrequency;

                break;
            }

            // 経過時間
            float elapsedTime = 0f;

            try
            {
                // フェード完了までループ
                while (elapsedTime < duration)
                {
                    // キャンセル確認
                    cancellationToken.ThrowIfCancellationRequested();

                    // 経過時間加算
                    elapsedTime += Time.unscaledDeltaTime;

                    // 補間率算出
                    float t = Mathf.Clamp01(elapsedTime / duration);

                    // 周波数補間
                    float frequency = Mathf.Lerp(
                        startFrequency,
                        targetFrequency,
                        t);

                    // 全フィルターへ反映
                    ApplyLowPassFrequency(filters, frequency);

                    // 次フレーム待機
                    await UniTask.Yield(
                        PlayerLoopTiming.Update,
                        cancellationToken);
                }

                // 最終値保証
                ApplyLowPassFrequency(filters, targetFrequency);
            }
            catch (OperationCanceledException) { }
        }

        /// <summary>
        /// 全ローパスフィルターへ周波数を反映
        /// </summary>
        /// <param name="filters">対象フィルター配列</param>
        /// <param name="frequency">適用周波数</param>
        private void ApplyLowPassFrequency(in AudioLowPassFilter[] filters, in float frequency)
        {
            // 全フィルターへ反映
            for (int i = 0; i < filters.Length; i++)
            {
                // フィルター取得
                AudioLowPassFilter filter = filters[i];

                if (filter == null)
                {
                    continue;
                }

                // 周波数反映
                filter.cutoffFrequency = frequency;
            }
        }
    }
}