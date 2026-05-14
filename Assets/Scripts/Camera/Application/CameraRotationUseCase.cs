// ======================================================
// CameraRotationUseCase.cs
// 作成者   : 高橋一翔
// 更新日時 : 2026-05-14
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

        /// <summary>入力用回転加速度</summary>
        private readonly float _acceleration;

        /// <summary>イベント用の回転収束時間</summary>
        private readonly float _smoothTime;

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
            in float acceleration,
            in float smoothTime)
        {
            _cameraModel = cameraModel;
            _maxSpeed = maxSpeed;
            _acceleration = acceleration;
            _smoothTime = smoothTime;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力値から回転を計算しモデルへ反映する
        /// </summary>
        public void UpdateInputRotation(in Vector2 input, in float deltaTime)
        {
            // --------------------------------------------------
            // 入力取得
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

            // 有効入力時
            if (!isNoInput)
            {
                // 入力方向を正規化
                Vector2 inputDirection = input.normalized;

                // 目標速度算出
                targetVelocityX = inputDirection.y * _maxSpeed * inputMagnitude;
                targetVelocityY = inputDirection.x * _maxSpeed * inputMagnitude;
            }
            else
            {
                // 入力なしの場合目標速度停止
                targetVelocityX = 0.0f;
                targetVelocityY = 0.0f;
            }

            // --------------------------------------------------
            // 速度補間
            // --------------------------------------------------
            _velocityX = UpdateVelocity(
                _velocityX,
                targetVelocityX,
                _acceleration,
                deltaTime);

            _velocityY = UpdateVelocity(
                _velocityY,
                targetVelocityY,
                _acceleration,
                deltaTime);

            // --------------------------------------------------
            // 回転反映
            // --------------------------------------------------
            // 次フレーム回転算出
            float nextX = _cameraModel.RotationX + _velocityX * deltaTime;
            float nextY = _cameraModel.RotationY + _velocityY * deltaTime;

            // 回転反映
            _cameraModel.SetRotationX(nextX);
            _cameraModel.SetRotationY(nextY);
        }

        /// <summary>
        /// イベント用回転を計算しモデルへ反映する
        /// </summary>
        public void UpdateEventRotation(in Vector3 input, in float deltaTime)
        {
            // --------------------------------------------------
            // 方向取得
            // --------------------------------------------------
            // 入力ベクトルが 0 の場合は処理なし
            if (input == Vector3.zero)
            {
                return;
            }

            // 正規化して方向取得
            Vector3 direction = input.normalized;
            
            // --------------------------------------------------
            // 目標回転生成
            // --------------------------------------------------
            // X 軸回転
            float targetRotationX = 0f;

            // Y 軸回転
            // XZ 平面の方向から Yaw 角を算出
            float targetRotationY =
                -Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // --------------------------------------------------
            // 回転収束
            // --------------------------------------------------
            float nextX =
                Mathf.SmoothDampAngle(
                    _cameraModel.RotationX,
                    targetRotationX,
                    ref _velocityX,
                    _smoothTime,
                    Mathf.Infinity,
                    deltaTime);

            float nextY =
                Mathf.SmoothDampAngle(
                    _cameraModel.RotationY,
                    targetRotationY,
                    ref _velocityY,
                    _smoothTime,
                    Mathf.Infinity,
                    deltaTime);

            // --------------------------------------------------
            // モデル反映
            // --------------------------------------------------
            // 計算結果をカメラモデルへ適用
            _cameraModel.SetRotationX(nextX);
            _cameraModel.SetRotationY(nextY);
        }

        /// <summary>
        /// 回転速度を即座にリセットする
        /// </summary>
        public void ResetRotationVelocity()
        {
            _velocityX = 0.0f;
            _velocityY = 0.0f;
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
            in float acceleration,
            in float deltaTime)
        {
            // 現在速度との差分取得
            float delta = targetVelocity - currentVelocity;

            // フレームあたり最大変化量算出
            float maxDelta = acceleration * deltaTime;

            // 加速度制限付き補間
            float nextVelocity =
                currentVelocity +
                Mathf.Clamp(delta, -maxDelta, maxDelta);

            // 最大速度制限
            nextVelocity = Mathf.Clamp(nextVelocity, -_maxSpeed, _maxSpeed);

            return nextVelocity;
        }
    }
}