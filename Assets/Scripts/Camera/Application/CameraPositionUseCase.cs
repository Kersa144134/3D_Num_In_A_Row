// ======================================================
// CameraPositionUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-10
// 更新日時 : 2026-06-10
// 概要     : カメラ位置の補間処理を管理するユースケース
// ======================================================

using UnityEngine;
using CameraSystem.Domain;

namespace CameraSystem.Application
{
    /// <summary>
    /// カメラ位置制御ユースケース
    /// </summary>
    public sealed class CameraPositionUseCase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>モデル</summary>
        private readonly CameraModel _cameraModel;

        /// <summary>イベント補間時間</summary>
        private Vector3 _velocityPosition;

        /// <summary>補間時間</summary>
        private readonly float _smoothTime;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>位置補間終了距離</summary>
        private const float POSITION_COMPLETE_DISTANCE = 0.001f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraModel">モデル</param>
        /// <param name="smoothTime">イベント補間時間</param>
        public CameraPositionUseCase(in CameraModel cameraModel, in float smoothTime)
        {
            _cameraModel = cameraModel;
            _smoothTime = smoothTime;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベント用位置を補間しモデルへ反映する
        /// </summary>
        /// <param name="targetPosition">目標座標</param>
        /// <param name="deltaTime">デルタ時間</param>
        public void UpdateEventPosition(
            in Vector3 targetPosition,
            in float deltaTime)
        {
            // 目標位置との距離を取得
            float distance = Vector3.Distance(
                _cameraModel.Position,
                targetPosition);

            // 一定距離以内なら補間終了
            if (distance <= POSITION_COMPLETE_DISTANCE)
            {
                // 速度リセット
                ResetVelocity();

                // モデルへ反映
                _cameraModel.ApplyPosition(targetPosition);

                return;
            }

            // 現在位置を目標位置へ補間
            Vector3 nextPosition = Vector3.SmoothDamp(
                _cameraModel.Position,
                targetPosition,
                ref _velocityPosition,
                _smoothTime,
                Mathf.Infinity,
                deltaTime);

            // モデルへ反映
            _cameraModel.ApplyPosition(nextPosition);
        }

        /// <summary>
        /// 位置補間速度をリセットする
        /// </summary>
        public void ResetVelocity()
        {
            _velocityPosition = Vector3.zero;
        }
    }
}