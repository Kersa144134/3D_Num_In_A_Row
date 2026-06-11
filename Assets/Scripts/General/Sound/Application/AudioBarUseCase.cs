// ======================================================
// AudioBarUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-11
// 更新日時 : 2026-06-11
// 概要     : BGM小節単位イベント管理ユースケース
// ======================================================

using SoundSystem.Infrastructure;
using System;
using UniRx;
using UnityEngine;

namespace SoundSystem.Application
{
    /// <summary>
    /// BGMの小節進行を監視し、UniRxでイベント通知する
    /// </summary>
    public sealed class AudioBarUseCase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>BGM の現在小節インデックス</summary>
        private readonly int[] _currentBarIndex;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>小節更新通知用 Subject</summary>
        private readonly Subject<AudioBarEvent> _onBarChanged = new Subject<AudioBarEvent>();

        /// <summary>小節更新通知ストリーム</summary>
        public IObservable<AudioBarEvent> OnBarChanged => _onBarChanged;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AudioBarUseCase(in int bgmCount)
        {
            _currentBarIndex = new int[bgmCount];
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 全 BGM の小節制御を更新する
        /// </summary>
        /// <param name="bgmSets">BGM 設定配列</param>
        public void Update(BgmSet[] bgmSets)
        {
            if (bgmSets == null)
            {
                return;
            }

            for (int i = 0; i < bgmSets.Length; i++)
            {
                UpdateBar(i, bgmSets[i]);
            }
        }

        /// <summary>
        /// 指定小節番号の再生時間（秒）を取得する
        /// </summary>
        /// <param name="bgmSet">対象BGM設定</param>
        /// <param name="bar">小節番号（1ベース）</param>
        /// <returns>再生時間（秒）</returns>
        public float GetTimeFromBar(BgmSet bgm, int bar)
        {
            if (bgm == null)
            {
                return 0f;
            }

            // 0 小節は例外扱いで必ず 0 秒
            if (bar <= 0)
            {
                return 0f;
            }

            // 1 ベース → 0 ベース補正
            int zeroBasedBar = Mathf.Max(bar - 1, 0);

            return zeroBasedBar * bgm.SecondsPerBar;
        }

        /// <summary>
        /// 再生位置のラグ（秒差）を算出
        /// </summary>
        /// <param name="bgm">BGM 情報</param>
        /// <param name="currentTime">変更前の再生時間</param>
        /// <returns>+: 遅れている, -: 先行している</returns>
        public float GetPlaybackLagFromBar(in BgmSet bgm, in float currentTime)
        {
            // 不正値チェック
            if (bgm == null || bgm.SecondsPerBar <= 0f)
            {
                return 0f;
            }

            // 現在の小節を取得
            int currentBar = GetBarFromTime(bgm, currentTime);

            if (currentBar < 0)
            {
                return 0f;
            }

            // 曲内時間に変換
            float relativeTime = currentTime - bgm.Offset;

            // 目標時間（小節の開始位置）
            float targetTime = (currentBar - 1) * bgm.SecondsPerBar;

            // ラグ算出
            return relativeTime - targetTime;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        public void Dispose()
        {
            _onBarChanged?.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 小節更新処理
        /// </summary>
        /// <param name="bgmIndex">BGM インデックス</param>
        /// <param name="bgm">BGM 設定</param>
        private void UpdateBar(in int bgmIndex, in BgmSet bgm)
        {
            if (bgm.Source == null)
            {
                return;
            }

            // 再生中でない場合は処理なし
            if (!bgm.Source.isPlaying)
            {
                return;
            }
            
            // 現在時刻を基準オフセットからの相対時間に変換
            float relativeTime = bgm.Source.time - bgm.Offset;

            // 基準より前の時間は対象外
            if (relativeTime < 0f)
            {
                return;
            }

            // 0 ベース小節
            int zeroBar = Mathf.FloorToInt(relativeTime / bgm.SecondsPerBar);

            // 1 ベース変換
            int oneBasedBar = zeroBar + 1;

            // 初回は強制イベント発火
            if (_currentBarIndex[bgmIndex] == -1)
            {
                _currentBarIndex[bgmIndex] = oneBasedBar;

                _onBarChanged.OnNext(new AudioBarEvent(bgmIndex, oneBasedBar));

                return;
            }

            // 変化がない場合は処理なし
            if (oneBasedBar == _currentBarIndex[bgmIndex])
            {
                return;
            }

            // 状態更新
            _currentBarIndex[bgmIndex] = oneBasedBar;

            // 小節更新イベント発火
            _onBarChanged.OnNext(new AudioBarEvent(bgmIndex, oneBasedBar));
        }

        /// <summary>
        /// 再生時間（秒）から小節番号を取得する
        /// </summary>
        /// <param name="bgm">対象BGM設定</param>
        /// <param name="time">再生時間（秒）</param>
        /// <returns>小節番号（1ベース）</returns>
        private int GetBarFromTime(BgmSet bgm, float time)
        {
            // 不正値チェック
            if (bgm == null || bgm.SecondsPerBar <= 0f)
            {
                return -1;
            }

            // オフセットを除外した時間を算出
            float relativeTime = time - bgm.Offset;

            if (relativeTime < 0f)
            {
                return 1;
            }

            // 0 ベースの小節番号を算出
            int zeroBasedBar = Mathf.FloorToInt(relativeTime / bgm.SecondsPerBar);

            // 1 ベース変換
            return zeroBasedBar + 1;
        }
    }
}