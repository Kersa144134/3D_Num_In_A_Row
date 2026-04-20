// ======================================================
// CameraProjectionService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-20
// 更新日時 : 2026-04-20
// 概要     : カメラ投影方法を切り替えるサービス
// ======================================================

using UnityEngine;

namespace CameraSystem.Application
{
    /// <summary>
    /// カメラ投影切り替えサービス
    /// </summary>
    public sealed class CameraProjectionService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>透視時の視野角</summary>
        private readonly float _perspectiveFov;

        /// <summary>平行時のサイズ</summary>
        private readonly float _orthographicSize;

        /// <summary>NearClip</summary>
        private readonly float _nearClip;

        /// <summary>FarClip</summary>
        private readonly float _farClip;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CameraProjectionService(
            in float perspectiveFov,
            in float orthographicSize,
            in float nearClip,
            in float farClip)
        {
            _perspectiveFov = perspectiveFov;
            _orthographicSize = orthographicSize;
            _nearClip = nearClip;
            _farClip = farClip;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 投影方法を切り替える
        /// </summary>
        /// <param name="camera">対象カメラ</param>
        /// <param name="isPerspective">true:透視 / false:平行</param>
        public void SetProjection(
            in Camera camera,
            in bool isPerspective)
        {
            if (camera == null)
            {
                return;
            }

            // --------------------------------------------------
            // ProjectionMatrixのリセット
            // --------------------------------------------------
            camera.ResetProjectionMatrix();

            // --------------------------------------------------
            // 透視投影
            // --------------------------------------------------
            if (isPerspective)
            {
                camera.orthographic = false;

                // 視野角を設定
                camera.fieldOfView = _perspectiveFov;
            }
            // --------------------------------------------------
            // 平行投影
            // --------------------------------------------------
            else
            {
                camera.orthographic = true;

                // サイズを設定
                camera.orthographicSize = _orthographicSize;
            }

            // --------------------------------------------------
            // 共通設定
            // --------------------------------------------------
            camera.nearClipPlane = _nearClip;
            camera.farClipPlane = _farClip;
        }
    }
}