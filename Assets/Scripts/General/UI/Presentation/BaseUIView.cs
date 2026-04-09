// ======================================================
// BaseUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : UI エフェクトの描画処理を担当するビュー
// ======================================================

using UnityEngine;
using UnityEngine.Rendering.Universal;
using ShaderSystem.Controller;

namespace UISystem.Presentation
{
    /// <summary>
    /// UI エフェクトの描画を担当するビュー
    /// </summary>
    public sealed class BaseUIView
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 2 値化エフェクト制御クラス
        /// </summary>
        private readonly BinarizationPostProcessController _binarization;

        /// <summary>
        /// グレースケール制御クラス
        /// </summary>
        private readonly GreyScalePostProcessController _greyScale;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BaseUIView(
            ScriptableRendererFeature binFeature,
            Material binMaterial,
            ScriptableRendererFeature greyFeature,
            Material greyMaterial)
        {
            // 2 値化エフェクト制御クラスを生成する
            _binarization = new BinarizationPostProcessController(binFeature, binMaterial);

            // グレースケール制御クラスを生成する
            _greyScale = new GreyScalePostProcessController(greyFeature, greyMaterial);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// エフェクトを更新する
        /// </summary>
        public void UpdateEffect(
            bool binEnabled,
            Vector2 binCenter,
            float binDistortion,
            float binNoise,
            float binThreshold,
            Color binLight,
            Color binDark,
            bool greyEnabled,
            Vector3 greyStrength,
            Vector2 greyCenter,
            float greyDistortion,
            float greyNoise,
            Color greyLight,
            Color greyDark)
        {
            // 2 値化エフェクトの状態とパラメータを反映する
            _binarization?.Update(
                binEnabled,
                binCenter,
                binDistortion,
                binNoise,
                binThreshold,
                binLight,
                binDark
            );

            // グレースケールエフェクトの状態とパラメータを反映する
            _greyScale?.Update(
                greyEnabled,
                greyStrength,
                greyCenter,
                greyDistortion,
                greyNoise,
                greyLight,
                greyDark
            );
        }
    }
}