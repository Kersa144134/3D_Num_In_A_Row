// ======================================================
// SoundManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-06-04
// 概要     : サウンド管理クラス
// ======================================================

using Cysharp.Threading.Tasks;
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

        [Header("BGM 設定")]
        /// <summary>音量フェード補間基準値（秒）</summary>
        [SerializeField]
        private float _volumeFadeTransition = 1.0f;

        /// <summary>ローパス補間基準値（秒）</summary>
        [SerializeField]
        private float _lowPassTransition = 1.0f;

        /// <summary>ローパス適用時の目標周波数</summary>
        [SerializeField]
        private float _lowPassTargetFrequency = 500f;

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

        /// <summary>AudioClip リポジトリ</summary>
        private readonly AudioClipRepository _audioClipRepository = new AudioClipRepository();

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

            if (_bgmSets == null ||
                _seSets == null)
            {
                Debug.LogError("[SoundManager] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif

                return;
            }

            // BGM 数取得
            int bgmCount = _bgmSets.Length;

            // SE 数取得
            int seCount = _seSets.Length;

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
            _lowPassFilters = new AudioLowPassFilter[bgmCount + seCount];

            // 全オーディオ対象を一括処理
            for (int i = 0; i < _lowPassFilters.Length; i++)
            {
                // 対象ソースの切り替え
                AudioSource source =
                    (i < bgmCount)
                    ? _bgmSets[i].Source
                    : _seSets[i - bgmCount].Source;

                if (source == null)
                {
                    continue;
                }

                // AudioLowPassFilter 取得
                _lowPassFilters[i] = source.GetComponent<AudioLowPassFilter>();
            }
        }

        private void OnDestroy()
        {
            // イベント購読解除
            _audioFadeUseCase?.Dispose();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // 初期化
        // --------------------------------------------------
        /// <summary>
        /// オーディオ初期化処理
        /// </summary>
        public async UniTask InitializeAudioAsync()
        {
            // 並列ロード実行
            await UniTask.WhenAll(
                _audioClipRepository.LoadBgmAsync(),
                _audioClipRepository.LoadSeAsync()
            );
        }
        
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
        /// BGM を再生する
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        public void PlayBGM(in BgmType type)
        {
            // BGM インデックス取得
            if (!_audioSetFinder.TryFindBgmIndex(type, out int bgmIndex))
            {
                return;
            }

            // BGM 設定取得
            BgmSet bgm = _bgmSets[bgmIndex];

            // AudioClip 取得
            if (!_audioClipRepository.TryGetBgmClip(type, out AudioClip clip))
            {
                return;
            }

            // 再生クリップ設定
            bgm.Source.clip = clip;

            // オフセット値をクリップ長範囲へ補正
            float offset = Mathf.Clamp(
                bgm.Offset,
                0f,
                clip.length);

            // 再生開始位置設定
            bgm.Source.time = offset;

            // BGM 再生
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
        /// BGM 音量設定
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        /// <param name="volume">目標音量</param>
        /// <param name="transitionDuration">補間時間</param>
        public void SetBGMVolume(
            in BgmType type,
            in float volume,
            float? transitionDuration = null)
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

            // 補間時間決定
            float duration =
                transitionDuration ??
                _volumeFadeTransition;

            // 音量フェード実行
            _audioFadeUseCase.StartVolumeFade(
                bgmIndex,
                bgm.Source,
                targetVolume,
                duration);
        }

        /// <summary>
        /// 全 BGM 音量設定
        /// </summary>
        /// <param name="volume">目標音量</param>
        /// <param name="transitionDuration">補間時間</param>
        public void SetBGMVolume(
            in float volume,
            float? transitionDuration = null)
        {
            // 音量補正
            float targetVolume = Mathf.Clamp01(volume);

            // 補間時間決定
            float duration =
                transitionDuration ??
                _volumeFadeTransition;

            // 全 BGM を走査
            for (int i = 0; i < _bgmSets.Length; i++)
            {
                BgmSet bgm = _bgmSets[i];

                // AudioSource 未設定・未再生なら処理なし
                if (bgm.Source == null ||
                    !bgm.Source.isPlaying)
                {
                    continue;
                }

                // 音量フェード実行
                _audioFadeUseCase.StartVolumeFade(
                    i,
                    bgm.Source,
                    targetVolume,
                    duration);
            }
        }

        /// <summary>
        /// ローパスフィルター ON / OFF
        /// </summary>
        /// <param name="enable">true: ON / false: OFF</param>
        /// <param name="transitionDuration">補間時間</param>
        public void SetBgmLowPass(
            in bool enable,
            float? transitionDuration = null)
        {
            // 目標周波数決定
            float targetFrequency = enable
                ? _lowPassTargetFrequency
                : MAX_LOW_PASS_FREQUENCY;

            // 補間時間決定
            float duration =
                transitionDuration ??
                _lowPassTransition;

            // ローパスフェード開始
            _audioFadeUseCase.StartLowPassFade(
                _lowPassFilters,
                targetFrequency,
                duration);
        }

        // --------------------------------------------------
        // SE
        // --------------------------------------------------
        /// <summary>
        /// 指定した SE を再生する
        /// </summary>
        /// <param name="type">SE タイプ</param>
        /// <param name="volume">再生音量</param>
        public void PlaySE(
            in SeType type,
            in float volume = 1.0f)
        {
            // SE インデックス取得
            if (!_audioSetFinder.TryFindSeIndex(type, out int index))
            {
                return;
            }

            SeSet se = _seSets[index];

            // AudioClip 取得
            if (!_audioClipRepository.TryGetSeClip(type, out AudioClip clip))
            {
                return;
            }

            // ループ SE 再生
            if (se.IsLoop)
            {
                PlayLoopSE(se.Source, clip, volume);

                return;
            }

            // 通常 SE 再生
            se.Source.PlayOneShot(clip, volume);
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
            // SE インデックス取得
            if (!_audioSetFinder.TryFindSeIndex(type, out int index))
            {
                return;
            }

            SeSet se = _seSets[index];

            // AudioClip 取得
            if (!_audioClipRepository.TryGetSeClip(type, out AudioClip clip))
            {
                return;
            }

            // 座標未指定の場合は最大音量で再生
            if (position == null)
            {
                se.Source.PlayOneShot(clip, 1f);

                return;
            }

            // 再生可能距離判定
            if (!_audioDistanceUseCase.IsAudible(position.Value))
            {
                return;
            }

            // 距離減衰音量算出
            float volume = _audioDistanceUseCase.CalculateVolume(position.Value);

            // ループ SE 再生
            if (se.IsLoop)
            {
                PlayLoopSE(se.Source, clip, volume);

                return;
            }

            // 通常 SE 再生
            se.Source.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// ループ SE 停止
        /// </summary>
        /// <param name="type">SE タイプ</param>
        public void StopLoopSE(in SeType type)
        {
            // SE インデックス取得
            if (!_audioSetFinder.TryFindSeIndex(type, out int index))
            {
                return;
            }

            SeSet se = _seSets[index];

            // ループ SE でない場合処理なし
            if (!se.IsLoop)
            {
                return;
            }

            se.Source.Stop();

            se.Source.clip = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ループ SE 再生
        /// </summary>
        /// <param name="se">SE 設定</param>
        /// <param name="clip">再生クリップ</param>
        /// <param name="volume">再生音量</param>
        private void PlayLoopSE(
            in AudioSource source,
            in AudioClip clip,
            in float volume)
        {
            source.Stop();

            source.clip = clip;
            source.volume = volume;
            source.loop = true;

            source.Play();
        }
    }
}