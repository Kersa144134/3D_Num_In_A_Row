// ======================================================
// CameraView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
// 更新日時 : 2026-04-08
// 概要     : カメラの Transform 操作を担当するビュー
// ======================================================

using UnityEngine;

namespace CameraSystem.Presentation
{
    /// <summary>
    /// カメラの Transform 操作を担当するビュー
    /// </summary>
    public class CameraView
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// カメラのTransform
        /// </summary>
        private readonly Transform _cameraTransform;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="transform">カメラ Transform</param>
        public CameraView(in Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 回転を適用する
        /// </summary>
        /// <param name="rotationX">X 回転</param>
        /// <param name="rotationY">Y 回転</param>
        public void ApplyRotation(in float rotationX, in float rotationY)
        {
            // Euler角を生成する
            Vector3 euler = new Vector3(rotationX, rotationY, 0.0f);

            // Transformに回転を反映する
            _cameraTransform.rotation = Quaternion.Euler(euler);
        }
    }
}