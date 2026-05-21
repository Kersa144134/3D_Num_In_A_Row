// ======================================================
// CameraRotationUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
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

        /// <summary>Pitch 角の最小絶対値</summary>
        private const float MIN_PITCH_ANGLE = 15f;

        /// <summary>Pitch 角の最大絶対値</summary>
        private const float MAX_PITCH_ANGLE = 60f;

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

            // 入力無効判定
            bool isNoInput = inputMagnitude < INPUT_THRESHOLD;

            // --------------------------------------------------
            // 目標速度生成
            // --------------------------------------------------
            float targetVelocityX;
            float targetVelocityY;

            if (isNoInput)
            {
                targetVelocityX = 0;
                targetVelocityY = 0;
            }
            else
            {
                // 入力方向を正規化
                Vector2 inputDirection = input.normalized;

                // 目標速度算出
                targetVelocityX = inputDirection.y * _maxSpeed * inputMagnitude;
                targetVelocityY = inputDirection.x * _maxSpeed * inputMagnitude;
            }

            // --------------------------------------------------
            // 速度補間
            // --------------------------------------------------
            // 現在速度を目標速度へ補間
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

            // モデルへ適用
            _cameraModel.SetRotationX(nextX);
            _cameraModel.SetRotationY(nextY);
        }

        /// <summary>
        /// イベント用回転を計算しモデルへ反映する
        /// </summary>
        public void UpdateEventRotation(in Vector3 targetAngle, in float deltaTime)
        {
            // --------------------------------------------------
            // 方向取得
            // --------------------------------------------------
            // 正規化して方向取得
            Vector3 direction = targetAngle.normalized;

            // 入力ベクトルの長さを取得
            float angleMagnitude = targetAngle.magnitude;

            // 入力無効判定
            bool isNoInput = angleMagnitude < INPUT_THRESHOLD;

            if (isNoInput)
            {
                return;
            }

            // --------------------------------------------------
            // 目標回転生成
            // --------------------------------------------------
            // XZ 平面上の水平方向距離を算出
            float horizontalDistance = Mathf.Sqrt(
                direction.x * direction.x +
                direction.z * direction.z);

            // X 軸回転角を算出（Pitch）
            // Y 成分と水平方向距離から上下方向の角度を生成し、ラジアン値を度数法へ変換
            float targetRotationX =
                Mathf.Atan2(direction.y, horizontalDistance) * Mathf.Rad2Deg;

            // Y 軸回転角を算出（Yaw）
            // XZ 平面上の方向ベクトルから左右方向の角度を生成し、ラジアン値を度数法へ変換
            // カメラを対象方向の反対側へ向けるため、180 度加算する
            float targetRotationY =
                Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + 180f;

            // --------------------------------------------------
            // Pitch 角度制限
            // --------------------------------------------------
            if (targetRotationX < MIN_PITCH_ANGLE)
            {
                targetRotationX = MIN_PITCH_ANGLE;
            }

            if (targetRotationX > MAX_PITCH_ANGLE)
            {
                targetRotationX = MAX_PITCH_ANGLE;
            }

            // --------------------------------------------------
            // 回転反映
            // --------------------------------------------------
            // X 軸回転を目標 Pitch 角へ補間
            float nextX = Mathf.SmoothDampAngle(
                _cameraModel.RotationX,
                targetRotationX,
                ref _velocityX,
                _smoothTime,
                Mathf.Infinity,
                deltaTime);

            // Y 軸回転を目標 Yaw 角へ補間
            float nextY = Mathf.SmoothDampAngle(
                _cameraModel.RotationY,
                targetRotationY,
                ref _velocityY,
                _smoothTime,
                Mathf.Infinity,
                deltaTime);

            // モデルへ適用
            _cameraModel.SetRotationX(nextX);
            _cameraModel.SetRotationY(nextY);
        }

        /// <summary>
        /// 回転速度を即座にリセットする
        /// </summary>
        public void ResetVelocity()
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