// ======================================================
// GreyScalePostProcessController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-21
// 更新日時 : 2026-01-21
// 概要     : 2 値化エフェクト用のシェーダーパラメータ更新と
//            Full Screen Pass Render Feature の ON / OFF 制御を行うコントローラー
// ======================================================

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ShaderSystem.Controller
{
    public sealed class GreyScalePostProcessController : BasePostProcessController
    {
        // ======================================================
        // シェーダープロパティ定義
        // ======================================================

        /// <summary>グレースケールの強さ</summary>
        public static readonly int GREY_SCALE_STRENGTH =
            Shader.PropertyToID("_GreyScaleStrength");

        /// <summary>歪み中心座標</summary>
        public static readonly int DISTORTION_CENTER =
            Shader.PropertyToID("_DistortionCenter");

        /// <summary>歪みの強さ</summary>
        public static readonly int DISTORTION_STRENGTH =
            Shader.PropertyToID("_DistortionStrength");

        /// <summary>ノイズの強度</summary>
        public static readonly int NOISE_STRENGTH =
            Shader.PropertyToID("_NoiseStrength");

        /// <summary>ポスタライズの境界しきい値</summary>
        public static readonly int POSTERIZATION_THRESHOLD =
            Shader.PropertyToID("_PosterizationThreshold");

        /// <summary>ポスタライズ明部カラー</summary>
        public static readonly int POSTERIZATION_LIGHT_COLOR =
            Shader.PropertyToID("_PosterizationLightColor");

        /// <summary>ポスタライズ暗部カラー</summary>
        public static readonly int POSTERIZATION_DARK_COLOR =
            Shader.PropertyToID("_PosterizationDarkColor");

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>エフェクトが有効かどうか</summary>
        private bool _isEffectEnabled = false;

        /// <summary>グレースケールの強さ</summary>
        private Vector3 _greyScaleStrength = new Vector3(0.25f, 0.25f, 0f);

        /// <summary>歪みの中心座標（UV 空間）</summary>
        private Vector2 _distortionCenter = new Vector2(0.5f, 0.5f);

        /// <summary>歪みエフェクトの強度</summary>
        private float _distortionStrength = 0.02f;

        /// <summary>ノイズエフェクトの強度</summary>
        private float _noiseStrength = 5000.0f;

        /// <summary>ポスタライズ明部に使用する HDR カラー</summary>
        private Color _posterizationLightColor = Color.black;

        /// <summary>ポスタライズ暗部に使用する HDR カラー</summary>
        private Color _posterizationDarkColor = Color.white;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// グレースケールエフェクト用コントローラーを初期化する
        /// </summary>
        public GreyScalePostProcessController(
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
            in Vector3 greyScaleStrength,
            in Vector2 distortionCenter,
            in float distortionStrength,
            in float noiseStrength,
            in Color posterizationLightColor,
            in Color posterizationDarkColor)
        {
            UpdateProperties(
                isEffectEnabled,
                greyScaleStrength,
                distortionCenter,
                distortionStrength,
                noiseStrength,
                posterizationLightColor,
                posterizationDarkColor
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
            in Vector3 greyScaleStrength,
            in Vector2 distortionCenter,
            in float distortionStrength,
            in float noiseStrength,
            in Color posterizationLightColor,
            in Color posterizationDarkColor)
        {
            _isEffectEnabled = isEffectEnabled;
            _greyScaleStrength = greyScaleStrength;
            _distortionCenter = distortionCenter;
            _distortionStrength = distortionStrength;
            _noiseStrength = noiseStrength;
            _posterizationLightColor = posterizationLightColor;
            _posterizationDarkColor = posterizationDarkColor;
        }

        /// <summary>
        /// 内部状態を Effect Material に書き込む
        /// </summary>
        protected override void ApplyPropertiesToMaterial(
            Material material)
        {
            // グレースケール強度を設定する
            material.SetVector(
                GREY_SCALE_STRENGTH,
                _greyScaleStrength);

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

            // 明部カラーを設定する
            material.SetColor(
                POSTERIZATION_LIGHT_COLOR,
                _posterizationLightColor);

            // 暗部カラーを設定する
            material.SetColor(
                POSTERIZATION_DARK_COLOR,
                _posterizationDarkColor);
        }
    }
}