// ======================================================
// SoundManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-06-04
// 概要     : サウンド管理クラス
// ======================================================

using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
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

        /// <summary>オーディオ小節管理ユースケース</summary>
        private AudioBarUseCase _audioBarUseCase;

        /// <summary>オーディオ距離計算ユースケース</summary>
        private AudioDistanceUseCase _audioDistanceUseCase;

        /// <summary>オーディオフェードユースケース</summary>
        private AudioFadeUseCase _audioFadeUseCase;

        /// <summary>オーディオ再生位置管理ユースケース</summary>
        private AudioPlaybackUseCase _audioPlaybackUseCase;

        /// <summary>オーディオ設定検索クラス</summary>
        private AudioSetFinder _audioSetFinder;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>BGM に紐付くローパスフィルター配列</summary>
        private AudioLowPassFilter[] _lowPassFilters;

        /// <summary>再生位置更新予約フラグ</summary>
        private bool _isPlaybackSeekRequested;

        /// <summary>予約された再生位置イベント</summary>
        private AudioPlaybackEvent _pendingPlaybackEvent;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ローパス無効時の周波数</summary>
        private const float MAX_LOW_PASS_FREQUENCY = 22000f;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

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
                UnityEngine.Application.Quit();
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
            _audioBarUseCase = new AudioBarUseCase(bgmCount);
            _audioDistanceUseCase = new AudioDistanceUseCase(_seMinDistance, _seMaxDistance);
            _audioFadeUseCase = new AudioFadeUseCase(bgmCount);
            _audioPlaybackUseCase = new AudioPlaybackUseCase(bgmCount);
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

            // イベント購読
            Subscribe();
        }

        private void Update()
        {
            // 小節制御更新
            _audioBarUseCase?.Update(_bgmSets);
        }

        private void OnDestroy()
        {
            // イベント購読解除
            Dispose();
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
        /// BGM 再生
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        /// <param name="blockIndex">再生ブロック番号</param>
        public void PlayBGM(in BgmType type, int blockIndex = 0)
        {
            // BGM インデックス取得
            if (!_audioSetFinder.TryFindBgmIndex(type, out int bgmIndex))
            {
                return;
            }

            // BGM 設定取得
            BgmSet bgm = _bgmSets[bgmIndex];

            // 再生中の場合
            if (bgm.Source.isPlaying)
            {
                // 再生位置更新
                SetPlaybackPosition(type, blockIndex);

                return;
            }
            
            // AudioClip 取得
            if (!_audioClipRepository.TryGetBgmClip(type, out AudioClip clip))
            {
                return;
            }

            // 再生クリップ設定
            bgm.Source.clip = clip;

            // 再生位置設定
            SetPlaybackPosition(type, blockIndex);

            // BGM 再生
            bgm.Source.Play();
        }

        /// <summary>
        /// BGM 停止
        /// </summary>
        /// <param name="type">BGM タイプ</param>
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

            // 再生ブロック情報リセット
            _audioPlaybackUseCase.ResetCurrentBlock(bgmIndex);
        }

        /// <summary>
        /// BGM 再生位置設定
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        /// <param name="blockIndex">再生ブロック番号</param>
        /// <returns>再生位置設定に成功した場合は true</returns>
        public bool SetPlaybackPosition(in BgmType type, in int blockIndex = 0)
        {
            // BGM インデックス取得
            if (!_audioSetFinder.TryFindBgmIndex(type, out int bgmIndex))
            {
                return false;
            }

            BgmSet bgm = _bgmSets[bgmIndex];

            // AudioClip 取得
            if (!_audioClipRepository.TryGetBgmClip(type, out AudioClip clip))
            {
                return false;
            }

            return _audioPlaybackUseCase.SetPlaybackPosition(
                bgmIndex,
                bgm,
                blockIndex);
        }

        /// <summary>
        /// BGM 再生ブロック番号取得
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        /// <param name="blockIndex">再生ブロック番号</param>
        /// <returns>取得に成功した場合は true</returns>
        public bool TryGetPlaybackBlockIndex(in BgmType type, out int blockIndex)
        {
            blockIndex = -1;

            // BGM インデックス取得
            if (!_audioSetFinder.TryFindBgmIndex(type, out int bgmIndex))
            {
                return false;
            }

            return _audioPlaybackUseCase.TryGetPlaybackBlockIndex(bgmIndex, out blockIndex);
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
            // BGM インデックス取得
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
            float duration = transitionDuration ?? _volumeFadeTransition;

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
        public void SetBGMVolume(in float volume, float? transitionDuration = null)
        {
            // 音量補正
            float targetVolume = Mathf.Clamp01(volume);

            // 補間時間決定
            float duration = transitionDuration ?? _volumeFadeTransition;

            // 全 BGM を走査
            for (int i = 0; i < _bgmSets.Length; i++)
            {
                BgmSet bgm = _bgmSets[i];

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
        public void SetBgmLowPass(in bool enable, float? transitionDuration = null)
        {
            // 目標周波数決定
            float targetFrequency = enable
                ? _lowPassTargetFrequency
                : MAX_LOW_PASS_FREQUENCY;

            // 補間時間決定
            float duration = transitionDuration ?? _lowPassTransition;

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
        public void PlaySE(in SeType type, in float volume = 1.0f)
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
        public void PlayDistanceSE(in SeType type, in Vector3? position = null)
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

            // 再生中でない場合は処理なし
            if (!se.Source.isPlaying)
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
        /// イベント購読
        /// </summary>
        private void Subscribe()
        {
            // 小節更新イベント購読
            _audioBarUseCase.OnBarChanged
                .Subscribe(OnBarChanged)
                .AddTo(_disposables);

            // BGM 一時停止
            _audioFadeUseCase.OnPauseRequested
                .Subscribe(OnPauseBgm)
                .AddTo(_disposables);

            // 再生位置更新リクエスト
            _audioPlaybackUseCase.OnPlaybackRequested
                .Subscribe(OnRequestPlaybackBgm)
                .AddTo(_disposables);
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        private void Dispose()
        {
            _audioBarUseCase?.Dispose();
            _audioFadeUseCase?.Dispose();
            _audioPlaybackUseCase?.Dispose();
        }

        /// <summary>
        /// 小節更新イベント処理
        /// </summary>
        private void OnBarChanged(AudioBarEvent e)
        {
            if (e.BgmIndex < 0 || e.BgmIndex >= _bgmSets.Length)
            {
                return;
            }

            BgmSet bgm = _bgmSets[e.BgmIndex];

            if (bgm.Source == null)
            {
                return;
            }

            // ---------------------------------------------
            // 再生制御判定
            // ---------------------------------------------
            // 予約データがある場合
            if (_isPlaybackSeekRequested)
            {
                // 現在小節と予約小節の偶奇が一致しているか判定
                bool isSameParity = _pendingPlaybackEvent.BarIndex % 2 == e.BarIndex % 2;

                // 先頭小節の場合または偶奇が一致した場合、再生位置更新
                if (e.BarIndex == 0 || isSameParity)
                {
                    // 再生ブロック情報更新
                    _audioPlaybackUseCase.SetCurrentBlock(
                        _pendingPlaybackEvent.BgmIndex,
                        _pendingPlaybackEvent.BlockIndex);

                    OnPlaybackBgm(_pendingPlaybackEvent);

                    _isPlaybackSeekRequested = false;

                    return;
                }
            }

            // 予約データなしまたは偶奇不一致の場合、通常処理
            _audioPlaybackUseCase.HandleBarEvent(e, bgm);
        }
        
        /// <summary>
        /// BGM 一時停止処理
        /// </summary>
        /// <param name="source">対象 AudioSource</param>
        private void OnPauseBgm(AudioSource source)
        {
            if (source == null)
            {
                return;
            }

            source.Pause();
        }

        /// <summary>
        /// BGM 再生位置更新予約処理
        /// </summary>
        /// <param name="e">再生位置イベント</param>
        private void OnRequestPlaybackBgm(AudioPlaybackEvent e)
        {
            if (e.BgmIndex < 0 || e.BgmIndex >= _bgmSets.Length)
            {
                return;
            }

            // 予約データとして保持
            _pendingPlaybackEvent = e;

            _isPlaybackSeekRequested = true;
        }

        /// <summary>
        /// BGM 再生位置更新処理
        /// </summary>
        /// <param name="e">再生位置イベント</param>
        private void OnPlaybackBgm(AudioPlaybackEvent e)
        {
            if (e.BgmIndex < 0 || e.BgmIndex >= _bgmSets.Length)
            {
                return;
            }

            BgmSet bgm = _bgmSets[e.BgmIndex];

            if (bgm.Source == null)
            {
                return;
            }

            // 小節 → 秒変換
            float time = _audioBarUseCase.GetTimeFromBar(bgm, e.BarIndex);

            // 現在のラグを取得
            float lag = _audioBarUseCase.GetPlaybackLagFromBar(bgm, bgm.Source.time);

            // 再生位置反映
            bgm.Source.time = bgm.Offset + time + lag;
        }

        /// <summary>
        /// ループ SE 再生
        /// </summary>
        /// <param name="source">再生ソース</param>
        /// <param name="clip">再生クリップ</param>
        /// <param name="volume">再生音量</param>
        private void PlayLoopSE(
            in AudioSource source,
            in AudioClip clip,
            in float volume)
        {
            // 既に再生中の場合は処理なし
            if (source.isPlaying)
            {
                return;
            }

            source.clip = clip;
            source.volume = volume;
            source.loop = true;

            source.Play();
        }
    }
}