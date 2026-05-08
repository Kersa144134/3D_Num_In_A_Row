// ======================================================
// BaseUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-04-09
// 概要     : UI エフェクトのインスペクタ設定と制御を担うプレゼンター
// ======================================================

using PhaseSystem.Domain;
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UniRx;
using UpdateSystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// UI エフェクトの制御を行うプレゼンター
    /// </summary>
    public abstract class BaseUIPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        // --------------------------------------------------
        // 演出 <2 値化>
        // --------------------------------------------------
        /// <summary>
        /// 2 値化エフェクトのRenderFeature
        /// </summary>
        [Header("演出 <2 値化>")]
        [SerializeField]
        private ScriptableRendererFeature _binarizationFeature;

        /// <summary>
        /// 2 値化エフェクト用マテリアル
        /// </summary>
        [SerializeField]
        private Material _binarizationMaterial;

        /// <summary>
        /// 2 値化エフェクトの有効状態
        /// </summary>
        [SerializeField]
        private bool _isBinarizationEnabled;

        /// <summary>
        /// 歪み中心座標
        /// </summary>
        [SerializeField]
        private Vector2 _binarizationDistortionCenter;

        /// <summary>
        /// 歪み強度
        /// </summary>
        [SerializeField]
        private float _binarizationDistortionStrength;

        /// <summary>
        /// ノイズ強度
        /// </summary>
        [SerializeField]
        private float _binarizationNoise;

        /// <summary>
        /// ポスタライズ閾値
        /// </summary>
        [SerializeField]
        private float _binarizationThreshold;

        /// <summary>
        /// 明部カラー
        /// </summary>
        [SerializeField]
        private Color _binarizationLight;

        /// <summary>
        /// 暗部カラー
        /// </summary>
        [SerializeField]
        private Color _binarizationDark;

        // --------------------------------------------------
        // 演出 <グレースケール>
        // --------------------------------------------------
        /// <summary>
        /// グレースケールのRenderFeature
        /// </summary>
        [Header("演出 <グレースケール>")]
        [SerializeField]
        private ScriptableRendererFeature _greyScaleFeature;

        /// <summary>
        /// グレースケール用マテリアル
        /// </summary>
        [SerializeField]
        private Material _greyScaleMaterial;

        /// <summary>
        /// グレースケールの有効状態
        /// </summary>
        [SerializeField]
        private bool _isGreyScaleEnabled;

        /// <summary>
        /// グレースケール強度
        /// </summary>
        [SerializeField]
        private Vector3 _greyScaleStrength;

        /// <summary>
        /// 歪み中心
        /// </summary>
        [SerializeField]
        private Vector2 _greyScaleDistortionCenter;

        /// <summary>
        /// 歪み強度
        /// </summary>
        [SerializeField]
        private float _greyScaleDistortionStrength;

        /// <summary>
        /// ノイズ強度
        /// </summary>
        [SerializeField]
        private float _greyScaleNoise;

        /// <summary>
        /// 明部カラー
        /// </summary>
        [SerializeField]
        private Color _greyScaleLight;

        /// <summary>
        /// 暗部カラー
        /// </summary>
        [SerializeField]
        private Color _greyScaleDark;

        // --------------------------------------------------
        // 演出 <歪み>
        // --------------------------------------------------
        /// <summary>
        /// 歪みのRenderFeature
        /// </summary>
        [Header("演出 <歪み>")]
        [SerializeField]
        private ScriptableRendererFeature _distortionFeature;

        /// <summary>
        /// 歪み用マテリアル
        /// </summary>
        [SerializeField]
        private Material _distortionMaterial;

        /// <summary>
        /// 歪みの有効状態
        /// </summary>
        [SerializeField]
        private bool _isDistortionEnabled;

        /// <summary>
        /// 歪み中心
        /// </summary>
        [SerializeField]
        private Vector2 _distortionCenter;

        /// <summary>
        /// 歪み強度
        /// </summary>
        [SerializeField]
        private float _distortionStrength;

        /// <summary>
        /// ノイズ強度
        /// </summary>
        [SerializeField]
        private float _distortionNoise;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// UI 描画ビュー
        /// </summary>
        private BaseUIView _view;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// エフェクト用アニメーター
        /// </summary>
        protected Animator _effectAnimator;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>シーン遷移リクエスト通知用 Subject</summary>
        private readonly Subject<Unit> _onSceneChangeRequested =
            new Subject<Unit>();


        /// <summary>シーン遷移リクエスト通知ストリーム</summary>
        public IObservable<Unit> OnSceneChangeRequested => _onSceneChangeRequested;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            _effectAnimator = GetComponent<Animator>();

            _view = new BaseUIView(
                _binarizationFeature,
                _binarizationMaterial,
                _greyScaleFeature,
                _greyScaleMaterial,
                _distortionFeature,
                _distortionMaterial
            );

            SetAnimatorUnscaledTime(_effectAnimator);

            OnEnterInternal();
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            _view.UpdateEffect(
                _isBinarizationEnabled,
                _binarizationDistortionCenter,
                _binarizationDistortionStrength,
                _binarizationNoise,
                _binarizationThreshold,
                _binarizationLight,
                _binarizationDark,
                _isGreyScaleEnabled,
                _greyScaleStrength,
                _greyScaleDistortionCenter,
                _greyScaleDistortionStrength,
                _greyScaleNoise,
                _greyScaleLight,
                _greyScaleDark,
                _isDistortionEnabled,
                _distortionCenter,
                _distortionStrength,
                _distortionNoise
            );
            
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

        public void OnExit()
        {
            OnExitInternal();
        }

        // ======================================================
        // 継承メソッド
        // ======================================================

        protected virtual void OnEnterInternal() { }

        protected virtual void OnLateUpdateInternal(in float unscaledDeltaTime) { }

        protected virtual void OnPhaseEnterInternal(in PhaseType phase) { }

        protected virtual void OnPhaseExitInternal(in PhaseType phase) { }

        protected virtual void OnExitInternal() { }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// シーン遷移リクエストを通知する
        /// </summary>
        public virtual void RequestSceneChange()
        {
            _onSceneChangeRequested.OnNext(Unit.Default);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// アニメーターをタイムスケール非依存に設定する
        /// </summary>
        protected void SetAnimatorUnscaledTime(in Animator animator)
        {
            if (animator == null)
            {
                return;
            }

            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }
}