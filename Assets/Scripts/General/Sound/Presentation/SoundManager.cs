// ======================================================
// SoundManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-15
// 更新日時 : 2026-05-26
// 概要     : サウンド管理クラス
// ======================================================

using SoundSystem.Application;
using SoundSystem.Domain;
using SoundSystem.Infrastructure;
using System;
using UnityEngine;

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

        /// <summary>シングルトンインスタンス</summary>
        public static SoundManager Instance { get; private set; }

        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("AudioSource / Clip 設定")]
        /// <summary>BGM 配列</summary>
        [SerializeField] private BgmSet[] _bgmSets;

        /// <summary>SE 配列</summary>
        [SerializeField] private SeSet[] _seSets;

        /// <summary>SE オーディオソース</summary>
        [SerializeField] private AudioSource _seSource;

        [Header("BGM 設定")]
        /// <summary>BGM フェードにかかる時間（秒）</summary>
        [SerializeField] private float _fadeDuration = 1.5f;

        /// <summary>ローパスフィルター ON 時の目標周波数</summary>
        [SerializeField] private float _lowPassTargetFrequency = 500f;

        /// <summary>ローパスフィルター補間時間（秒）</summary>
        [SerializeField] private float _lowPassTransition = 1.0f;

        [Header("SE 設定")]
        /// <summary>SE 再生距離の最小値</summary>
        [SerializeField] private float _seMinDistance = 5f;

        /// <summary>SE 再生距離の最大値</summary>
        [SerializeField] private float _seMaxDistance = 80f;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>サウンドセット検索クラス</summary>
        private SoundSetFinder _soundSetFinder;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>SE に使用するサウンドリスナー基準 Transform</summary>
        private Transform _listenerTransform;
        
        /// <summary>BGM オーディオソースにアタッチされた AudioLowPassFilter 配列</summary>
        private AudioLowPassFilter[] _lowPassFilters;

        /// <summary>BGM セットごとのフェード中フラグ</summary>
        private bool[] _isFadingArray;

        /// <summary>BGM セットごとのフェード開始音量</summary>
        private float[] _fadeStartVolumeArray;

        /// <summary>BGM セットごとのフェード目標音量</summary>
        private float[] _fadeTargetVolumeArray;

        /// <summary>BGM セットごとのフェード経過時間（秒）</summary>
        private float[] _fadeElapsedArray;

        /// <summary>ローパス補間中フラグ</summary>
        private bool _isLowPassActive = false;

        /// <summary>ローパス補間開始時の周波数</summary>
        private float _lowPassStartFreq;

        /// <summary>ローパス補間目標周波数</summary>
        private float _lowPassTargetFreq;

        /// <summary>ローパス補間経過時間（秒）</summary>
        private float _lowPassElapsed;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>BGM ローパスフィルター OFF 時の最大周波数</summary>
        private const float MAX_LOW_PASS_FREQUENCY = 22000f;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_bgmSets == null ||
                _seSets == null ||
                _seSource == null)
            {
                Debug.LogError("[SoundManager] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            // --------------------------------------------------
            // サウンドセット検索クラス初期化
            // --------------------------------------------------
            _soundSetFinder = new SoundSetFinder(_bgmSets, _seSets);

            // --------------------------------------------------
            // LowPassFilter 配列初期化
            // --------------------------------------------------
            // BGM セットの要素数取得
            int bgmCount = _bgmSets.Length;

            _lowPassFilters = new AudioLowPassFilter[bgmCount];

            for (int i = 0; i < bgmCount; i++)
            {
                AudioSource source = _bgmSets[i].Source;

                if (source == null)
                {
                    continue;
                }

                // LowPass取得
                _lowPassFilters[i] = source.GetComponent<AudioLowPassFilter>();
            }

            // --------------------------------------------------
            // フェード配列初期化
            // --------------------------------------------------
            _isFadingArray = new bool[bgmCount];
            _fadeStartVolumeArray = new float[bgmCount];
            _fadeTargetVolumeArray = new float[bgmCount];
            _fadeElapsedArray = new float[bgmCount];
        }

        private void Update()
        {
            float unscaledDeltaTime = Time.unscaledDeltaTime; ;

            // --------------------------------------------------
            // BGM フェード更新
            // 各 BGM セットごとに個別の値を適用
            // --------------------------------------------------
            for (int i = 0; i < _bgmSets.Length; i++)
            {
                BgmSet bgm = _bgmSets[i];

                if (bgm.Source == null)
                {
                    continue;
                }

                // 対象の BGM セットがフェード中か確認
                if (_isFadingArray[i])
                {
                    // 経過時間を加算
                    _fadeElapsedArray[i] += unscaledDeltaTime;

                    // 0 ～ 1 の補間係数に変換
                    float t = Mathf.Clamp01(_fadeElapsedArray[i] / _fadeDuration);

                    // フェード補間
                    bgm.Source.volume = Mathf.Lerp(_fadeStartVolumeArray[i], _fadeTargetVolumeArray[i], t);

                    // フェード完了時にフラグを解除
                    if (t >= 1f)
                    {
                        _isFadingArray[i] = false;
                    }
                }
            }

            // --------------------------------------------------
            // ローパス補間更新
            // 全 BGM に同じローパス値を適用
            // --------------------------------------------------
            if (_isLowPassActive && _lowPassFilters != null)
            {
                // 経過時間を加算
                _lowPassElapsed += unscaledDeltaTime;

                // 補間係数
                float t = (_lowPassTransition > 0f)
                    ? Mathf.Clamp01(_lowPassElapsed / _lowPassTransition)
                    : 1f;

                // 補間値計算
                float cutoff = Mathf.Lerp(_lowPassStartFreq, _lowPassTargetFreq, t);

                // 全フィルターに適用
                for (int i = 0; i < _lowPassFilters.Length; i++)
                {
                    AudioLowPassFilter filter = _lowPassFilters[i];

                    if (filter == null)
                    {
                        continue;
                    }

                    filter.cutoffFrequency = cutoff;
                }

                // 補間完了
                if (t >= 1f)
                {
                    _isLowPassActive = false;
                }
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // リスナー
        // --------------------------------------------------
        /// <summary>
        /// サウンドリスナーの基準 Transform を設定
        /// </summary>
        /// <param name="listener">リスナーとなるTransform</param>
        public void SetListenerTransform(in Transform listener)
        {
            _listenerTransform = listener;
        }

        /// <summary>
        /// サウンドリスナーの基準 Transform をリセット
        /// </summary>
        public void ResetListenerTransform()
        {
            _listenerTransform = null;
        }

        // --------------------------------------------------
        // BGM
        // --------------------------------------------------
        /// <summary>
        /// 指定タイプの BGM を再生
        /// </summary>
        /// <param name="type">再生する BGM タイプ</param>
        public void PlayBGM(in BgmType type)
        {
            // 指定タイプに一致する BGM セット取得
            if (!_soundSetFinder.TryFindBgmSet(type, out BgmSet bgm))
            {
                return;
            }
            
            // Clip を設定
            bgm.Source.clip = bgm.Clip;

            bgm.Source.Play();
        }

        /// <summary>
        /// BGM 停止
        /// </summary>
        /// <param name="type">
        /// 停止する BGM タイプ
        /// None の場合は全 BGM 停止
        /// </param>
        public void StopBGM(in BgmType type = BgmType.None)
        {
            // 全 BGM 停止
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

            // 指定タイプに一致する BGM セット取得
            if (!_soundSetFinder.TryFindBgmSet(type, out BgmSet bgm))
            {
                return;
            }

            bgm.Source.Stop();
        }

        /// <summary>
        /// BGM フェード開始
        /// </summary>
        /// <param name="type">フェード対象の BGM タイプ</param>
        /// <param name="fadeType">フェードタイプ</param>
        public void FadeBGM(in BgmType type, in FadeType fadeType)
        {
            for (int i = 0; i < _bgmSets.Length; i++)
            {
                if (_bgmSets[i].Type != type)
                {
                    continue;
                }

                if (_bgmSets[i].Source == null)
                {
                    return;
                }

                // フェード開始音量を保存
                _fadeStartVolumeArray[i] = _bgmSets[i].Source.volume;

                // フェード目標音量設定
                _fadeTargetVolumeArray[i] =
                    (fadeType == FadeType.FadeIn)
                    ? 1f
                    : 0f;

                // フェード経過時間初期化
                _fadeElapsedArray[i] = 0f;

                // フェード開始
                _isFadingArray[i] = true;

                return;
            }
        }

        /// <summary>
        /// 指定タイプの BGM 音量を設定
        /// </summary>
        /// <param name="type">BGM タイプ</param>
        /// <param name="volume">設定音量</param>
        public void SetBGMVolume(in BgmType type, in float volume)
        {
            // 指定タイプに一致する BGM セット取得
            if (!_soundSetFinder.TryFindBgmSet(type, out BgmSet bgm))
            {
                return;
            }

            bgm.Source.volume = volume;
        }

        /// <summary>
        /// ローパスフィルター ON / OFF
        /// </summary>
        /// <param name="enable">true: ON / false: OFF</param>
        public void SetBgmLowPass(in bool enable)
        {
            if (_lowPassFilters == null)
            {
                return;
            }

            // 補間開始値を現在の周波数で設定
            if (_lowPassFilters.Length > 0 && _lowPassFilters[0] != null)
            {
                _lowPassStartFreq = _lowPassFilters[0].cutoffFrequency;
            }

            // 補間目標値を設定
            _lowPassTargetFreq = enable ? _lowPassTargetFrequency : MAX_LOW_PASS_FREQUENCY;

            // 補間開始
            _isLowPassActive = true;
            _lowPassElapsed = 0f;
        }

        // --------------------------------------------------
        // SE
        // --------------------------------------------------
        /// <summary>
        /// SE 再生
        /// </summary>
        /// <param name="type">再生する SE タイプ</param>
        /// <param name="position">音発生位置、null ならリスナー位置扱い</param>
        public void PlaySE(in SeType type, in Vector3? position = null)
        {
            // 指定タイプに一致する BGM セット取得
            if (!_soundSetFinder.TryFindSeSet(type, out SeSet se))
            {
                return;
            }

            // 音発生位置が未指定の場合は最大音量で再生
            if (position == null)
            {
                _seSource.PlayOneShot(se.Clip, 1f);
                return;
            }

            // リスナー位置取得
            Vector3 listenerPos = _listenerTransform != null
                ? _listenerTransform.position
                : Vector3.zero;

            // リスナーと音発生位置の距離計算
            float distance = Vector3.Distance(listenerPos, position.Value);

            // 最大距離外は再生しない
            if (distance > _seMaxDistance)
            {
                return;
            }

            // 距離に応じた音量取得
            float volume = CalculateSEVolume(distance);

            // SE 再生
            _seSource.PlayOneShot(se.Clip, volume);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 距離から SE 音量を算出
        /// </summary>
        /// <param name="distance">リスナーとの距離</param>
        /// <returns>再生音量（0 ～ 1）</returns>
        private float CalculateSEVolume(in float distance)
        {
            // 最小距離以内は最大音量
            if (distance <= _seMinDistance)
            {
                return 1f;
            }

            // 最大距離以上は無音
            if (distance >= _seMaxDistance)
            {
                return 0f;
            }

            // 距離の上限と下限を 0 ～ 1 に正規化
            float normalized = Mathf.InverseLerp(_seMinDistance, _seMaxDistance, distance);

            // 距離が遠くなるほど音量が下がるように反転
            float volume = 1f - normalized;

            return volume;
        }
    }
}