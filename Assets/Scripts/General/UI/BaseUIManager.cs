// ======================================================
// BaseUIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-02-02
// 概要     : 全シーン共通で使用される UI 演出を管理する基底クラス
// ======================================================

using UnityEngine;
using UnityEngine.Rendering.Universal;
using PhaseSystem.Domain;
using SceneSystem.Data;
using ShaderSystem.Controller;

namespace UISystem
{
    /// <summary>
    /// UI の視覚的演出のみを管理する基底クラス
    /// </summary>
    public abstract class BaseUIManager : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // --------------------------------------------------
        // 演出 <2 値化>
        // --------------------------------------------------
        [Header("演出 <2 値化>")]

        /// <summary>
        /// 2 値化エフェクトを適用する Full Screen Pass Render Feature
        /// </summary>
        [SerializeField]
        protected ScriptableRendererFeature _binarizationFullScreenPassFeature;

        /// <summary>
        /// 2 値化エフェクト用マテリアル
        /// </summary>
        [SerializeField]
        protected Material _binarizationEffectMaterial;

        /// <summary>
        /// 2 値化エフェクトが有効かどうか
        /// </summary>
        [SerializeField]
        protected bool _isBinarizationEffectEnabled;

        /// <summary>
        /// 歪みの中心座標
        /// </summary>
        [SerializeField]
        protected Vector2 _binarizationDistortionCenter;

        /// <summary>
        /// 歪みエフェクトの強度
        /// </summary>
        [SerializeField]
        protected float _binarizationDistortionStrength;

        /// <summary>
        /// ノイズエフェクトの強度
        /// </summary>
        [SerializeField]
        protected float _binarizationNoiseStrength;

        /// <summary>
        /// ポスタライズ処理のしきい値
        /// </summary>
        [SerializeField]
        protected float _binarizationPosterizationThreshold;

        /// <summary>
        /// ポスタライズ明部カラー
        /// </summary>
        [SerializeField, ColorUsage(false, true)]
        protected Color _binarizationPosterizationLightColor;

        /// <summary>
        /// ポスタライズ暗部カラー
        /// </summary>
        [SerializeField, ColorUsage(false, true)]
        protected Color _binarizationPosterizationDarkColor;

        // --------------------------------------------------
        // 演出 <グレースケール>
        // --------------------------------------------------
        [Header("演出 <グレースケール>")]

        /// <summary>
        /// グレースケールエフェクトを適用する Full Screen Pass Render Feature
        /// </summary>
        [SerializeField]
        protected ScriptableRendererFeature _greyScaleFullScreenPassFeature;

        /// <summary>
        /// グレースケールエフェクト用マテリアル
        /// </summary>
        [SerializeField]
        protected Material _greyScaleEffectMaterial;

        /// <summary>
        /// グレースケールエフェクトが有効かどうか
        /// </summary>
        [SerializeField]
        protected bool _isGreyScaleEffectEnabled;

        /// <summary>
        /// グレースケールの強度
        /// </summary>
        [SerializeField]
        protected Vector3 _greyScaleStrength;

        /// <summary>
        /// 歪みの中心座標
        /// </summary>
        [SerializeField]
        protected Vector2 _greyScaleDistortionCenter;

        /// <summary>
        /// 歪みエフェクトの強度
        /// </summary>
        [SerializeField]
        protected float _greyScaleDistortionStrength;

        /// <summary>
        /// ノイズエフェクトの強度
        /// </summary>
        [SerializeField]
        protected float _greyScaleNoiseStrength;

        /// <summary>
        /// ポスタライズ明部カラー
        /// </summary>
        [SerializeField, ColorUsage(false, true)]
        protected Color _greyScalePosterizationLightColor;

        /// <summary>
        /// ポスタライズ暗部カラー
        /// </summary>
        [SerializeField, ColorUsage(false, true)]
        protected Color _greyScalePosterizationDarkColor;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// 2 値化ポストプロセス制御クラス
        /// </summary>
        protected BinarizationPostProcessController _binarizationPostProcessController;

        /// <summary>
        /// グレースケールポストプロセス制御クラス
        /// </summary>
        protected GreyScalePostProcessController _greyScalePostProcessController;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>エフェクト用アニメーター</summary>
        protected Animator _effectAnimator;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>フェード演出の時間</summary>
        protected const float FADE_TIME = 0.5f;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            OnEnterInternal();
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            OnLateUpdateInternal(unscaledDeltaTime);
        }

        public void OnPhaseEnter(in PhaseType phase)
        {
            OnPhaseEnterInternal(phase);
        }

        public void OnPhaseExit(in PhaseType phase)
        {
            OnPhaseExitInternal(phase);
        }

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected virtual void OnEnterInternal()
        {
            // エフェクト用 Animator を取得する
            _effectAnimator = GetComponent<Animator>();

            // 2 値化ポストプロセス制御クラスを生成する
            _binarizationPostProcessController =
                new BinarizationPostProcessController(
                    _binarizationFullScreenPassFeature,
                    _binarizationEffectMaterial
                );

            // グレースケールポストプロセス制御クラスを生成する
            _greyScalePostProcessController =
                new GreyScalePostProcessController(
                    _greyScaleFullScreenPassFeature,
                    _greyScaleEffectMaterial
                );

            // 各アニメーターをタイムスケール非依存に設定する
            SetAnimatorUnscaledTime(_effectAnimator);
        }

        protected virtual void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            // 2 値化エフェクトの有効／無効および各種パラメーターを反映する
            _binarizationPostProcessController?.Update(
                _isBinarizationEffectEnabled,
                _binarizationDistortionCenter,
                _binarizationDistortionStrength,
                _binarizationNoiseStrength,
                _binarizationPosterizationThreshold,
                _binarizationPosterizationLightColor,
                _binarizationPosterizationDarkColor
            );

            // グレースケールエフェクトの有効／無効および各種パラメーターを反映する
            _greyScalePostProcessController?.Update(
                _isGreyScaleEffectEnabled,
                _greyScaleStrength,
                _greyScaleDistortionCenter,
                _greyScaleDistortionStrength,
                _greyScaleNoiseStrength,
                _greyScalePosterizationLightColor,
                _greyScalePosterizationDarkColor
            );
        }

        protected virtual void OnPhaseEnterInternal(in PhaseType phase) { }

        protected virtual void OnPhaseExitInternal(in PhaseType phase) { }

        // ======================================================
        // BaseUIManager イベント
        // ======================================================

        /// <summary>
        /// 指定した Animator をタイムスケール非依存で更新するよう設定する
        /// </summary>
        /// <param name="animator">設定対象の Animator</param>
        protected void SetAnimatorUnscaledTime(in Animator animator)
        {
            // Animator が未設定の場合は処理なし
            if (animator == null)
            {
                return;
            }

            // タイムスケールの影響を受けない更新モードに設定する
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }
}