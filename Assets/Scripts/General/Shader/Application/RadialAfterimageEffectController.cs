// ======================================================
// RadialAfterimageEffectController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-01
// 更新日時 : 2026-06-01
// 概要     : 画面中心からの残像エフェクト用のシェーダーパラメータ更新を行うコントローラー
// ======================================================

using UnityEngine;

namespace ShaderSystem.Application
{
    /// <summary>
    /// 画面中心からの残像エフェクト用のシェーダーパラメータ更新を行うコントローラー
    /// </summary>
    public class RadialAfterimageEffectController
    {
        // ======================================================
        // シェーダープロパティ定義
        // ======================================================

        /// <summary>エフェクト強度</summary>
        private const string PROP_EFFECT_STRENGTH = "_EffectStrength";

        /// <summary>残像サンプル数</summary>
        private const string PROP_SAMPLE_COUNT = "_SampleCount";

        /// <summary>拡大間隔</summary>
        private const string PROP_SCALE_STEP = "_ScaleStep";

        /// <summary>拡大速度</summary>
        private const string PROP_SCALE_SPEED = "_ScaleSpeed";

        /// <summary>フェード距離</summary>
        private const string PROP_ALPHA_FADE_DISTANCE = "_AlphaFadeDistance";

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>対象マテリアル</summary>
        private readonly Material _material;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// Material を受け取り、MaterialPropertyBlock を初期化する
        /// </summary>
        /// <param name="material">対象マテリアル</param>
        public RadialAfterimageEffectController(in Material material)
        {
            if (material == null)
            {
                return;
            }

            _material = material;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// マテリアルの状態を更新する
        /// </summary>
        public void Update(
            in float propEffectStrength,
            in float propSampleCount,
            in float propScaleStep,
            in float propScaleSpeed,
            in float propAlphaFadeDistance)
        {
            if (_material == null)
            {
                return;
            }

            UpdateProperties(
                propEffectStrength,
                propSampleCount,
                propScaleStep,
                propScaleSpeed,
                propAlphaFadeDistance
            );
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// シェーダープロパティの設定を更新する
        /// </summary>
        private void UpdateProperties(
            in float propEffectStrength,
            in float propSampleCount,
            in float propScaleStep,
            in float propScaleSpeed,
            in float propAlphaFadeDistance)
        {
            SetEffectStrength(propEffectStrength);
            SetSampleCount(propSampleCount);
            SetScaleStep(propScaleStep);
            SetScaleSpeed(propScaleSpeed);
            SetAlphaFadeDistance(propAlphaFadeDistance);
        }

        /// <summary>
        /// エフェクト強度を設定
        /// </summary>
        private void SetEffectStrength(in float value)
        {
            // 0 ～ 1 に制限
            float clamped = Mathf.Clamp01(value);

            _material.SetFloat(PROP_EFFECT_STRENGTH, clamped);
        }

        /// <summary>
        /// 残像サンプル数を設定
        /// </summary>
        private void SetSampleCount(in float value)
        {
            // 最低値保証
            float clamped = Mathf.Max(0f, value);

            _material.SetFloat(PROP_SAMPLE_COUNT, clamped);
        }

        /// <summary>
        /// 拡大間隔を設定
        /// </summary>
        private void SetScaleStep(in float value)
        {
            // 下限制限
            float clamped = Mathf.Max(0.01f, value);

            _material.SetFloat(PROP_SCALE_STEP, clamped);
        }

        /// <summary>
        /// 拡大速度を設定
        /// </summary>
        private void SetScaleSpeed(in float value)
        {
            // 負値防止
            float clamped = Mathf.Max(0.0f, value);

            _material.SetFloat(PROP_SCALE_SPEED, clamped);
        }

        /// <summary>
        /// フェード距離を設定
        /// </summary>
        private void SetAlphaFadeDistance(in float value)
        {
            // 最低値保証
            float clamped = Mathf.Max(0.01f, value);

            _material.SetFloat(PROP_ALPHA_FADE_DISTANCE, clamped);
        }
    }
}