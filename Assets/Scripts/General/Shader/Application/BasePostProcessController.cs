// ======================================================
// BasePostProcessController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-21
// 更新日時 : 2026-01-21
// 概要     : Full Screen Pass Render Feature を制御するための共通基底クラス
// ======================================================

using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ShaderSystem.Controller
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
            // Full Screen Pass Feature が未設定の場合は処理を行わない
            if (_fullScreenPassFeature == null)
            {
                return;
            }

            // すでに目的の状態である場合は処理を行わない
            if (_fullScreenPassFeature.isActive == isEnable)
            {
                return;
            }

            // Full Screen Pass Feature の有効状態を切り替える
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
            // Material が未設定の場合は反映を行わない
            if (_effectMaterial == null)
            {
                return;
            }

            // 派生クラス側で定義された
            // 個別のパラメータ反映処理を呼び出す
            ApplyPropertiesToMaterial(_effectMaterial);
        }
    }
}