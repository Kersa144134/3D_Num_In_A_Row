// ======================================================
// CameraView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
// 更新日時 : 2026-05-18
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
        /// カメラの親 Transform
        /// </summary>
        private readonly Transform _parentTransform;

        /// <summary>
        /// カメラの Transform
        /// </summary>
        private readonly Transform _cameraTransform;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="parentTransform">カメラの親 Transform</param>
        /// <param name="cameraTransform">カメラ Transform</param>
        public CameraView(in Transform parentTransform, in Transform cameraTransform)
        {
            _parentTransform = parentTransform;
            _cameraTransform = cameraTransform;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// カメラのワールド X, Y 回転を適用する
        /// </summary>
        /// <param name="rotationX">X 回転</param>
        /// <param name="rotationY">Y 回転</param>
        public void ApplyRotation(in float rotationX, in float rotationY)
        {
            // Euler 角を生成
            Vector3 euler = new Vector3(rotationX, rotationY, 0.0f);

            // 親 Transform に回転を反映する
            _parentTransform.rotation = Quaternion.Euler(euler);
        }

        /// <summary>
        /// カメラのローカル Z 距離を適用する
        /// </summary>
        /// <param name="distanceZ">適用する Z 距離</param>
        public void ApplyDistanceZ(in float distanceZ)
        {
            Vector3 localPosition = _cameraTransform.localPosition;

            // Z 座標を Unity 用に反転して更新
            localPosition.z = -distanceZ;

            // ローカル座標を反映
            _cameraTransform.localPosition = localPosition;
        }
    }
}