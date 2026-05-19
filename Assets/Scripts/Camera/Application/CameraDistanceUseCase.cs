// ======================================================
// CameraDistanceUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-18
// 更新日時 : 2026-05-18
// 概要     : カメラ Z 距離の補間処理を管理するユースケース
// ======================================================

using UnityEngine;
using CameraSystem.Domain;

namespace CameraSystem.Application
{
    /// <summary>
    /// カメラ距離制御ユースケース
    /// </summary>
    public sealed class CameraDistanceUseCase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>距離制御対象モデル</summary>
        private readonly CameraModel _cameraModel;

        /// <summary>距離補間時間</summary>
        private readonly float _smoothTime;

        /// <summary>Z 距離現在速度</summary>
        private float _velocityDistanceZ;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraModel">制御対象モデル</param>
        /// <param name="smoothTime">補間時間</param>
        public CameraDistanceUseCase(
            in CameraModel cameraModel,
            in float smoothTime)
        {
            _cameraModel = cameraModel;
            _smoothTime = smoothTime;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベント用 Z 距離を補間しモデルへ反映する
        /// </summary>
        /// <param name="targetDistanceZ">目標 Z 距離</param>
        /// <param name="deltaTime">デルタ時間</param>
        public void UpdateDistance(
            in float targetDistanceZ,
            in float deltaTime)
        {
            // 現在距離を目標距離へ補間
            float nextDistanceZ = Mathf.SmoothDamp(
                _cameraModel.DistanceZ,
                targetDistanceZ,
                ref _velocityDistanceZ,
                _smoothTime,
                Mathf.Infinity,
                deltaTime);

            // 補間後距離をモデルへ適用
            _cameraModel.SetDistanceZ(nextDistanceZ);
        }

        /// <summary>
        /// 距離速度をリセットする
        /// </summary>
        public void ResetVelocity()
        {
            _velocityDistanceZ = 0.0f;
        }
    }
}