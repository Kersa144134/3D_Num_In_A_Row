// ======================================================
// DistortionPostProcessController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-21
// 更新日時 : 2026-01-21
// 概要     : 歪みエフェクト用のシェーダーパラメータ更新と
//            Full Screen Pass Render Feature の ON / OFF 制御を行うコントローラー
// ======================================================

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ShaderSystem.Application
{
    public sealed class DistortionPostProcessController : BasePostProcessController
    {
        // ======================================================
        // シェーダープロパティ定義
        // ======================================================

        /// <summary>歪み中心座標</summary>
        public static readonly int DISTORTION_CENTER =
            Shader.PropertyToID("_DistortionCenter");

        /// <summary>歪みの強さ</summary>
        public static readonly int DISTORTION_STRENGTH =
            Shader.PropertyToID("_DistortionStrength");

        /// <summary>ノイズの強度</summary>
        public static readonly int NOISE_STRENGTH =
            Shader.PropertyToID("_NoiseStrength");

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>エフェクトが有効かどうか</summary>
        private bool _isEffectEnabled = false;

        /// <summary>歪みの中心座標</summary>
        private Vector2 _distortionCenter = new Vector2(0.5f, 0.5f);

        /// <summary>歪みエフェクトの強度</summary>
        private float _distortionStrength = 0.1f;

        /// <summary>ノイズエフェクトの強度</summary>
        private float _noiseStrength = 5000.0f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 2 値化エフェクト用パラメータを受け取り初期化する
        /// </summary>
        public DistortionPostProcessController(
            in ScriptableRendererFeature fullScreenPassFeature,
            in Material effectMaterial)
            : base(
                fullScreenPassFeature,
                effectMaterial)
        {
            SetFullScreenPassActive(false);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Full Screen Pass Render Feature の状態を更新する
        /// </summary>
        public void Update(
            in bool isEffectEnabled,
            in Vector2 distortionCenter,
            in float distortionStrength,
            in float noiseStrength)
        {
            UpdateProperties(
                isEffectEnabled,
                distortionCenter,
                distortionStrength,
                noiseStrength
            );

            if (_isEffectEnabled)
            {
                SetFullScreenPassActive(true);
            }
            else
            {
                SetFullScreenPassActive(false);
                return;
            }

            ApplyToMaterial();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// シェーダープロパティの設定を更新する
        /// </summary>
        private void UpdateProperties(
            in bool isEffectEnabled,
            in Vector2 distortionCenter,
            in float distortionStrength,
            in float noiseStrength)
        {
            _isEffectEnabled = isEffectEnabled;
            _distortionCenter = distortionCenter;
            _distortionStrength = distortionStrength;
            _noiseStrength = noiseStrength;
        }

        /// <summary>
        /// 内部状態を Effect Material に書き込む
        /// </summary>
        protected override void ApplyPropertiesToMaterial(
            Material material)
        {
            // 歪み中心座標を設定する
            material.SetVector(
                DISTORTION_CENTER,
                _distortionCenter);

            // 歪み強度を設定する
            material.SetFloat(
                DISTORTION_STRENGTH,
                _distortionStrength);

            // ノイズ強度を設定する
            material.SetFloat(
                NOISE_STRENGTH,
                _noiseStrength);
        }
    }
}