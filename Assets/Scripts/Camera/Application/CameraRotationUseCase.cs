// ======================================================
// CameraRotationUseCase.cs
// 作成者   : 高橋一翔
// 更新日時 : 2026-04-21
// 概要     : カメラ回転の入力計算 + 補間処理を行うユースケース
// ======================================================

using UnityEngine;
using CameraSystem.Domain;

namespace CameraSystem.Application
{
    /// <summary>
    /// カメラ回転ユースケース
    /// </summary>
    public sealed class CameraRotationUseCase
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>回転対象モデル</summary>
        private readonly CameraModel _cameraModel;

       /// <summary>最大回転速度</summary>
        private readonly float _maxSpeed;

        /// <summary>回転加速度</summary>
        private readonly float _acceleration;

        /// <summary>X 軸現在速度</summary>
        private float _velocityX;

        /// <summary>Y 軸現在速度</summary>
        private float _velocityY;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>入力判定に使用する閾値</summary>
        private const float INPUT_THRESHOLD = 0.01f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CameraRotationUseCase(
            in CameraModel cameraModel,
            in float maxSpeed,
            in float acceleration)
        {
            _cameraModel = cameraModel;
            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力値から回転を計算しモデルへ反映する
        /// </summary>
        public void UpdateRotation(in Vector2 input, in float deltaTime)
        {
            // --------------------------------------------------
            // 入力解析
            // --------------------------------------------------
            // 入力ベクトルの長さを取得
            float inputMagnitude = input.magnitude;

            // デッドゾーン判定
            bool isNoInput = inputMagnitude < INPUT_THRESHOLD;

            // --------------------------------------------------
            // 目標速度生成
            // --------------------------------------------------
            float targetVelocityX;
            float targetVelocityY;

            if (!isNoInput)
            {
                // 入力方向を正規化
                Vector2 inputDirection = input.normalized;

                // X 軸の目標速度算出
                targetVelocityX = inputDirection.y * _maxSpeed * inputMagnitude;

                // Y 軸の目標速度算出
                targetVelocityY = inputDirection.x * _maxSpeed * inputMagnitude;
            }
            else
            {
                // 入力がない場合は目標速度を 0 にする
                targetVelocityX = 0.0f;
                targetVelocityY = 0.0f;
            }

            // --------------------------------------------------
            // 速度補間
            // --------------------------------------------------
            _velocityX = UpdateVelocity(_velocityX, targetVelocityX, deltaTime);
            _velocityY = UpdateVelocity(_velocityY, targetVelocityY, deltaTime);

            // --------------------------------------------------
            // 回転反映
            // --------------------------------------------------
            // 現在速度から次の回転値を算出する
            float nextX = _cameraModel.RotationX + _velocityX * deltaTime;
            float nextY = _cameraModel.RotationY + _velocityY * deltaTime;

            // X 回転は制限付きで反映する
            _cameraModel.SetRotationX(nextX);

            //  Y回転はそのまま反映する
            _cameraModel.SetRotationY(nextY);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 速度を目標速度へ補間する処理
        /// </summary>
        private float UpdateVelocity(
            in float currentVelocity,
            in float targetVelocity,
            in float deltaTime)
        {
            // 現在速度と目標速度の差分を取得
            float delta = targetVelocity - currentVelocity;

            // フレームあたりの最大変化量を算出
            float maxDelta = _acceleration * deltaTime;

            // 加速度制限
            float nextVelocity = currentVelocity + Mathf.Clamp(delta, -maxDelta, maxDelta);

            // 最大速度制限
            nextVelocity = Mathf.Clamp(nextVelocity, -_maxSpeed, _maxSpeed);

            return nextVelocity;
        }
    }
}