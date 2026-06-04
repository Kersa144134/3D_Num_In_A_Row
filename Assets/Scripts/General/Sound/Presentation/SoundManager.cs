// ======================================================
// SoundManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-06-04
// 概要     : サウンド管理クラス
// ======================================================

using UnityEngine;
using SoundSystem.Application;
using SoundSystem.Domain;
using SoundSystem.Infrastructure;

namespace SoundSystem.Presentation
{
    /// <summary>
    /// サウンドを管理するクラス
    /// </summary>
    public sealed class SoundManager : MonoBehaviour
    {
        // ======================================================
        // シングルトン
        // ======================================================

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static SoundManager Instance { get; private set; }

        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("AudioSource / Clip 設定")]
        /// <summary>BGM 設定配列</summary>
        [SerializeField]
        private BgmSet[] _bgmSets;

        /// <summary>SE 設定配列</summary>
        [SerializeField]
        private SeSet[] _seSets;

        /// <summary>SE 再生用 AudioSource</summary>
        [SerializeField]
        private AudioSource _seSource;

        [Header("BGM 設定")]
        /// <summary>ローパス適用時の目標周波数</summary>
        [SerializeField]
        private float _lowPassTargetFrequency = 500f;

        /// <summary>ローパス補間時間</summary>
        [SerializeField]
        private float _lowPassTransition = 1.0f;

        [Header("SE 設定")]
        /// <summary>SE 最大音量となる距離</summary>
        [SerializeField]
        private float _seMinDistance = 5f;

        /// <summary>SE 再生可能最大距離</summary>
        [SerializeField]
        private float _seMaxDistance = 80f;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>オーディオ距離計算ユースケース</summary>
        private AudioDistanceUseCase _audioDistanceUseCase;

        /// <summary>オーディオフェードユースケース</summary>
        private AudioFadeUseCase _audioFadeUseCase;

        /// <summary>オーディオ設定検索クラス</summary>
        private AudioSetFinder _audioSetFinder;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>BGM に紐付くローパスフィルター配列</summary>
        private AudioLowPassFilter[] _lowPassFilters;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ローパス無効時の周波数</summary>
        private const float MAX_LOW_PASS_FREQUENCY = 22000f;

        // ======================================================
        // Unityイベント
        // ======================================================

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Awake()
        {
            // --------------------------------------------------
            // シングルトン
            // --------------------------------------------------
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            DontDestroyOnLoad(gameObject);

            // --------------------------------------------------
            // 初期参照
            // --------------------------------------------------
            if (_bgmSets == null ||
                _seSets == null ||
                _seSource == null)
            {
                Debug.LogError(
                    "[SoundManager] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif

                return;
            }

            // BGM 数取得
            int bgmCount = _bgmSets.Length;

            // --------------------------------------------------
            // クラス初期化
            // --------------------------------------------------
            // オーディオ距離計算ユースケース生成
            _audioDistanceUseCase = new AudioDistanceUseCase(
                _seMinDistance,
                _seMaxDistance);

            // オーディオフェードユースケース生成
            _audioFadeUseCase = new AudioFadeUseCase(bgmCount);
            
            // オーディオ検索クラス生成
            _audioSetFinder = new AudioSetFinder(_bgmSets, _seSets);

            // --------------------------------------------------
            // ローパス設定初期化
            // --------------------------------------------------
            // ローパス配列生成
            _lowPassFilters = new AudioLowPassFilter[bgmCount];

            // 各 BGM 設定から AudioLowPassFilter 取得
            for (int i = 0; i < bgmCount; i++)
            {
                AudioSource source = _bgmSets[i].Source;

                if (source == null)
                {
                    continue;
                }

                _lowPassFilters[i] = source.GetComponent<AudioLowPassFilter>();
            }
        }

        /// <summary>
        /// 終了時クリーンアップ
        /// </summary>
        private void OnDestroy()
        {
            // 購読破棄
            _audioFadeUseCase?.Dispose();
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
            _audioDistanceUseCase.SetListenerTransform(listener);
        }

        /// <summary>
        /// リスナー Transform 解除
        /// </summary>
        public void ResetListenerTransform()
        {
            _audioDistanceUseCase.ResetListenerTransform();
        }

        // --------------------------------------------------
        // BGM
        // --------------------------------------------------
        /// <summary>
        /// BGM再生
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        public void PlayBGM(in BgmType type)
        {
            // BGM インデックス取得
            if (!_audioSetFinder.TryFindBgmIndex(type, out int bgmIndex))
            {
                return;
            }

            BgmSet bgm = _bgmSets[bgmIndex];

            bgm.Source.clip = bgm.Clip;

            bgm.Source.Play();
        }

        /// <summary>
        /// BGM停止
        /// </summary>
        /// <param name="type">停止対象</param>
        public void StopBGM(in BgmType type = BgmType.None)
        {
            // None の場合、全 BGM 停止
            if (type == BgmType.None)
            {
                for (int i = 0; i < _bgmSets.Length; i++)
                {
                    if (_bgmSets[i].Source == null)
                    {
                        continue;
                    }

                    _bgmSets[i].Source.Stop();
                }

                return;
            }

            // BGM インデックス取得
            if (!_audioSetFinder.TryFindBgmIndex(type, out int bgmIndex))
            {
                return;
            }

            BgmSet bgm = _bgmSets[bgmIndex];

            bgm.Source.Stop();
        }

        /// <summary>
        /// BGM音量設定
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        /// <param name="volume">目標音量</param>
        /// <param name="transitionDuration">補間時間</param>
        public void SetBGMVolume(
            in BgmType type,
            in float volume,
            in float transitionDuration = 0f)
        {
            // BGMインデックス取得
            if (!_audioSetFinder.TryFindBgmIndex(type, out int bgmIndex))
            {
                return;
            }

            BgmSet bgm = _bgmSets[bgmIndex];

            if (bgm.Source == null)
            {
                return;
            }

            // 音量補正
            float targetVolume = Mathf.Clamp01(volume);

            // 音量フェード実行
            _audioFadeUseCase.StartVolumeFade(
                bgmIndex,
                bgm.Source,
                targetVolume,
                transitionDuration);
        }

        /// <summary>
        /// ローパスフィルター ON / OFF
        /// </summary>
        /// <param name="enable">true: ON / false: OFF</param>
        public void SetBgmLowPass(in bool enable)
        {
            // 目標周波数決定
            float targetFrequency = enable
                ? _lowPassTargetFrequency
                : MAX_LOW_PASS_FREQUENCY;

            // ローパスフェード開始
            _audioFadeUseCase.StartLowPassFade(
                _lowPassFilters,
                targetFrequency,
                _lowPassTransition);
        }

        // --------------------------------------------------
        // SE
        // --------------------------------------------------
        /// <summary>
        /// SE 再生
        /// </summary>
        /// <param name="type">SE タイプ</param>
        /// <param name="volume">再生音量</param>
        public void PlaySE(in SeType type, in float volume = 1.0f)
        {
            // SE 取得
            if (!_audioSetFinder.TryFindSeSet(type, out SeSet se))
            {
                return;
            }

            _seSource.PlayOneShot(se.Clip, volume);
        }

        /// <summary>
        /// 距離減衰付き SE 再生
        /// </summary>
        /// <param name="type">SE タイプ</param>
        /// <param name="position">音源位置</param>
        public void PlayDistanceSE(
            in SeType type,
            in Vector3? position = null)
        {
            // SE 取得
            if (!_audioSetFinder.TryFindSeSet(type, out SeSet se))
            {
                return;
            }

            // 座標未指定の場合は最大音量で再生
            if (position == null)
            {
                _seSource.PlayOneShot(se.Clip, 1f);

                return;
            }

            // 再生可能距離判定
            if (!_audioDistanceUseCase.IsAudible(position.Value))
            {
                return;
            }

            // 距離減衰音量算出
            float volume = _audioDistanceUseCase.CalculateVolume(position.Value);

            _seSource.PlayOneShot(se.Clip, volume);
        }
    }
}