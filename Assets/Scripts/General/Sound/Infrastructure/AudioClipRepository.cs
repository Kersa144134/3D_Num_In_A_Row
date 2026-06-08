// ======================================================
// AudioClipRepository.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-08
// 概要     : AudioClip 管理クラス
// ======================================================

using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using SoundSystem.Domain;

namespace SoundSystem.Infrastructure
{
    /// <summary>
    /// AudioClip 管理クラス
    /// </summary>
    public sealed class AudioClipRepository
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>サウンドルートディレクトリ名</summary>
        private const string SOUND_DIRECTORY_NAME = "Sounds";

        /// <summary>BGM ディレクトリ名</summary>
        private const string BGM_DIRECTORY_NAME = "BGM";

        /// <summary>SE ディレクトリ名</summary>
        private const string SE_DIRECTORY_NAME = "SE";

        /// <summary>WAV 拡張子</summary>
        private const string WAV_EXTENSION = ".wav";

        /// <summary>OGG 拡張子</summary>
        private const string OGG_EXTENSION = ".ogg";

        /// <summary>MP3 拡張子</summary>
        private const string MP3_EXTENSION = ".mp3";

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>AudioClip ローダー</summary>
        private readonly AudioClipLoader _audioClipLoader;

        /// <summary>BGM クリップ辞書</summary>
        private readonly Dictionary<BgmType, AudioClip> _bgmClipMap;

        /// <summary>SE クリップ辞書</summary>
        private readonly Dictionary<SeType, AudioClip> _seClipMap;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AudioClipRepository()
        {
            // AudioClip ローダー生成
            _audioClipLoader = new AudioClipLoader();

            // BGM 辞書生成
            _bgmClipMap = new Dictionary<BgmType, AudioClip>();

            // SE 辞書生成
            _seClipMap = new Dictionary<SeType, AudioClip>();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 全 AudioClip をロードする
        /// </summary>
        public async UniTask LoadAllAsync()
        {
            await UniTask.WhenAll(
                LoadBgmAsync(),
                LoadSeAsync()
            );
        }

        /// <summary>
        /// BGM を取得する
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        /// <param name="clip">取得結果</param>
        public bool TryGetBgmClip(in BgmType type, out AudioClip clip)
        {
            // BGM を取得
            return _bgmClipMap.TryGetValue(type, out clip);
        }

        /// <summary>
        /// SE を取得する
        /// </summary>
        /// <param name="type">SE タイプ</param>
        /// <param name="clip">取得結果</param>
        public bool TryGetSeClip(in SeType type, out AudioClip clip)
        {
            // SE を取得
            return _seClipMap.TryGetValue(type, out clip);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ディレクトリ内の BGM 音声ファイルをロードする
        /// </summary>
        private async UniTask LoadBgmAsync()
        {
            // BGM ディレクトリ取得
            string directoryPath = Path.Combine(
                Application.streamingAssetsPath,
                SOUND_DIRECTORY_NAME,
                BGM_DIRECTORY_NAME);

            // ディレクトリが存在しない
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogWarning($"[AudioClipRepository] サウンドディレクトリが存在しません。 Path : {directoryPath}");

                return;
            }

            // ロードタスク一覧
            List<UniTask> loadTasks = new List<UniTask>();

            // 全ファイル走査
            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                if (!IsAudioFile(filePath))
                {
                    continue;
                }

                // タスク追加
                loadTasks.Add(LoadBgmClipAsync(filePath));
            }

            // 完了待機
            await UniTask.WhenAll(loadTasks);
        }

        /// <summary>
        /// BGM ファイルを読み込み辞書へ登録する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        private async UniTask LoadBgmClipAsync(string filePath)
        {
            // ファイル名取得
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Enum 変換失敗
            if (!Enum.TryParse(fileName, out BgmType type))
            {
                Debug.LogWarning($"[AudioClipRepository] BgmType に存在しないファイルです。 FileName : {fileName}");

                return;
            }

            // AudioClip ロード
            AudioClip clip = await _audioClipLoader.LoadAsync(filePath);

            if (clip == null)
            {
                return;
            }

            // 登録
            _bgmClipMap[type] = clip;
        }

        /// <summary>
        /// ディレクトリ内の SE 音声ファイルをロードする
        /// </summary>
        private async UniTask LoadSeAsync()
        {
            // SE ディレクトリ取得
            string directoryPath = Path.Combine(
                Application.streamingAssetsPath,
                SOUND_DIRECTORY_NAME,
                SE_DIRECTORY_NAME);

            // ディレクトリが存在しない
            if (!Directory.Exists(directoryPath))
            {
                Debug.LogWarning($"[AudioClipRepository] サウンドディレクトリが存在しません。 Path : {directoryPath}");

                return;
            }

            // ロードタスク一覧
            List<UniTask> loadTasks = new List<UniTask>();

            // 全ファイル走査
            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                if (!IsAudioFile(filePath))
                {
                    continue;
                }

                // タスク追加
                loadTasks.Add(LoadSeClipAsync(filePath));
            }

            // 全ロード完了待機
            await UniTask.WhenAll(loadTasks);
        }

        /// <summary>
        /// SE ファイルを読み込み辞書へ登録する
        /// </summary>
        /// <param name="filePath">SE ファイルパス</param>
        private async UniTask LoadSeClipAsync(string filePath)
        {
            // ファイル名取得
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            // Enum 変換失敗
            if (!Enum.TryParse(fileName, out SeType type))
            {
                Debug.LogWarning($"[AudioClipRepository] SeType に存在しないファイルです。 FileName : {fileName}");

                return;
            }

            // AudioClip ロード
            AudioClip clip = await _audioClipLoader.LoadAsync(filePath);

            if (clip == null)
            {
                return;
            }

            // 辞書登録
            _seClipMap[type] = clip;
        }

        /// <summary>
        /// 対応音声ファイルか判定する
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>判定結果</returns>
        private bool IsAudioFile(in string filePath)
        {
            // 拡張子取得
            string extension =
                Path.GetExtension(filePath)
                    .ToLowerInvariant();

            return extension == WAV_EXTENSION ||
                   extension == OGG_EXTENSION ||
                   extension == MP3_EXTENSION;
        }
    }
}