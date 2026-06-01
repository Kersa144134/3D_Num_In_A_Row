// ======================================================
// BasePostProcessController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-21
// 更新日時 : 2026-01-21
// 概要     : Full Screen Pass Render Feature を制御するための共通基底クラス
// ======================================================

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ShaderSystem.Application
{
    /// <summary>
    /// Full Screen Pass Render Feature 制御基底クラス
    /// </summary>
    public abstract class BasePostProcessController
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// 制御対象となる Full Screen Pass Render Feature
        /// </summary>
        protected readonly ScriptableRendererFeature _fullScreenPassFeature;

        /// <summary>
        /// Full Screen Pass で使用される Effect Material
        /// </summary>
        protected readonly Material _effectMaterial;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="fullScreenPassFeature">外部から注入される制御対象の Render Feature</param>
        protected BasePostProcessController(
            ScriptableRendererFeature fullScreenPassFeature,
            Material effectMaterial)
        {
            _fullScreenPassFeature = fullScreenPassFeature;
            _effectMaterial = effectMaterial;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Full Screen Pass Render Feature の有効状態を設定する
        /// </summary>
        /// <param name="isEnable">有効にするかどうか</param>
        public void SetFullScreenPassActive(bool isEnable)
        {
            if (_fullScreenPassFeature == null)
            {
                return;
            }

            if (_fullScreenPassFeature.isActive)
            {
                return;
            }

            _fullScreenPassFeature.SetActive(isEnable);
        }

        // ======================================================
        // 抽象メソッド
        // ======================================================

        /// <summary>
        /// 派生クラスごとのシェーダーパラメータを
        /// Material に書き込むための抽象メソッド
        /// </summary>
        /// <param name="material">
        /// 書き込み対象となる Effect Material
        /// </param>
        protected abstract void ApplyPropertiesToMaterial(
            Material material);

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 現在保持している内部状態を
        /// Effect Material に反映する
        /// </summary>
        protected void ApplyToMaterial()
        {
            if (_effectMaterial == null)
            {
                return;
            }

            ApplyPropertiesToMaterial(_effectMaterial);
        }
    }
}