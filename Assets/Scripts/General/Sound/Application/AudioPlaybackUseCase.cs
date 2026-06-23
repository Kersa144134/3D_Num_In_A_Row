// ======================================================
// AudioPlaybackUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-11
// 更新日時 : 2026-06-11
// 概要     : オーディオ再生位置・ループ制御ユースケース
// ======================================================

using System;
using UnityEngine;
using UniRx;
using SoundSystem.Infrastructure;

namespace SoundSystem.Application
{
    /// <summary>
    /// オーディオ再生位置およびループ制御を管理するクラス
    /// </summary>
    public sealed class AudioPlaybackUseCase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>BGMごとの現在再生ブロックインデックス</summary>
        private readonly int[] _currentBlockIndex;

        // ======================================================
        // UniRx 関連
        // ======================================================

        /// <summary>
        /// 再生位置更新リクエスト用 Subject
        /// </summary>
        private readonly Subject<AudioPlaybackEvent> _onPlaybackRequested = new Subject<AudioPlaybackEvent>();

        /// <summary>
        /// 再生位置更新リクエストストリーム
        /// </summary>
        public IObservable<AudioPlaybackEvent> OnPlaybackRequested => _onPlaybackRequested;
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="bgmCount">BGM 数</param>
        public AudioPlaybackUseCase(in int bgmCount)
        {
            _currentBlockIndex = new int[bgmCount];

            // 初期値は未設定状態
            for (int i = 0; i < _currentBlockIndex.Length; i++)
            {
                _currentBlockIndex[i] = -1;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// BGM 再生位置を設定する
        /// </summary>
        /// <param name="bgmIndex">対象BGMインデックス</param>
        /// <param name="bgm">BGM設定</param>
        /// <param name="blockIndex">再生ブロック番号（0ベース）</param>
        /// <returns>成功時 true</returns>
        public bool SetPlaybackPosition(
            in int bgmIndex,
            in BgmSet bgm,
            int blockIndex = 0)
        {
            // 再生ブロックチェック
            if (bgm.PlaybackBlocks == null || bgm.PlaybackBlocks.Length == 0)
            {
                return false;
            }

            // BGM インデックス範囲チェック
            if (bgmIndex < 0 || bgmIndex >= _currentBlockIndex.Length)
            {
                return false;
            }

            // ブロックインデックス範囲チェック
            if (blockIndex < 0 || blockIndex >= bgm.PlaybackBlocks.Length)
            {
                return false;
            }

            // 対象ブロック取得
            BgmPlaybackBlock block = bgm.PlaybackBlocks[blockIndex];

            // 状態更新
            _currentBlockIndex[bgmIndex] = blockIndex;

            // 再生位置更新リクエスト通知
            _onPlaybackRequested.OnNext(
                new AudioPlaybackEvent(
                    bgmIndex,
                    blockIndex,
                    block.StartBar
                )
            );

            return true;
        }

        /// <summary>
        /// BGM 再生ブロック番号取得
        /// </summary>
        /// <param name="bgmIndex">対象 BGM インデックス</param>
        /// <param name="blockIndex">再生ブロック番号</param>
        /// <returns>取得に成功した場合は true</returns>
        public bool TryGetPlaybackBlockIndex(in int bgmIndex, out int blockIndex)
        {
            blockIndex = -1;

            // BGM インデックス範囲チェック
            if (bgmIndex < 0 || bgmIndex >= _currentBlockIndex.Length)
            {
                return false;
            }

            blockIndex = _currentBlockIndex[bgmIndex];

            return true;
        }

        /// <summary>
        /// 小節イベントを基に再生制御を行う
        /// </summary>
        public void HandleBarEvent(in AudioBarEvent e, in BgmSet bgm)
        {
            if (bgm.Source == null)
            {
                return;
            }

            // 現在ブロックインデックス取得
            int currentIndex = _currentBlockIndex[e.BgmIndex];

            // 未初期化の場合は先頭ブロックに補正
            if (currentIndex < 0)
            {
                _currentBlockIndex[e.BgmIndex] = 0;
                currentIndex = 0;
            }

            // 再生ブロック未設定の場合通常再生
            if (bgm.PlaybackBlocks == null || bgm.PlaybackBlocks.Length == 0)
            {
                return;
            }

            // 現在ブロック取得
            BgmPlaybackBlock block = bgm.PlaybackBlocks[currentIndex];

            // ---------------------------------------------
            // ループ終了判定
            // ---------------------------------------------
            if (e.BarIndex >= block.LoopEndBar)
            {
                // ループブロックの場合
                if (block.IsLoop)
                {
                    // 再生位置をループ開始へ戻す通知
                    _onPlaybackRequested.OnNext(
                        new AudioPlaybackEvent(e.BgmIndex, currentIndex, block.LoopStartBar));

                    return;
                }

                // 次ブロック遷移処理
                if (bgm.PlaybackBlocks != null && bgm.PlaybackBlocks.Length > 1)
                {
                    int nextIndex = currentIndex + 1;

                    // 次ブロック範囲チェック
                    if (nextIndex < bgm.PlaybackBlocks.Length)
                    {
                        BgmPlaybackBlock nextBlock = bgm.PlaybackBlocks[nextIndex];

                        // インデックス更新
                        _currentBlockIndex[e.BgmIndex] = nextIndex;

                        // 次ブロック開始位置へ移動通知
                        _onPlaybackRequested.OnNext(
                            new AudioPlaybackEvent(e.BgmIndex, nextIndex, nextBlock.StartBar));
                    }
                }
            }
        }

        /// <summary>
        /// BGM 現在再生ブロックを更新する
        /// </summary>
        /// <param name="bgmIndex">対象BGMインデックス</param>
        /// <param name="blockIndex">設定するブロックインデックス</param>
        public void SetCurrentBlock(in int bgmIndex, in int blockIndex)
        {
            if (bgmIndex < 0 || bgmIndex >= _currentBlockIndex.Length)
            {
                return;
            }

            // 不正値チェック
            if (blockIndex < 0)
            {
                return;
            }

            // ブロック位置セット
            _currentBlockIndex[bgmIndex] = blockIndex;
        }
        
        /// <summary>
        /// BGM 再生状態をリセットする
        /// </summary>
        /// <param name="bgmIndex">対象BGMインデックス</param>
        public void ResetCurrentBlock(in int bgmIndex)
        {
            if (bgmIndex < 0 || bgmIndex >= _currentBlockIndex.Length)
            {
                return;
            }

            // ブロック位置リセット
            _currentBlockIndex[bgmIndex] = -1;
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        public void Dispose()
        {
            _onPlaybackRequested?.Dispose();
        }
    }
}