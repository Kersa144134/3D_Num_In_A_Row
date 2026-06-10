// ======================================================
// CameraDistanceUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-18
// 更新日時 : 2026-05-21
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

        /// <summary>入力時最大速度</summary>
        private readonly float _maxSpeed;

        /// <summary>入力時加速度</summary>
        private readonly float _acceleration;

        /// <summary>イベント補間時間</summary>
        private readonly float _smoothTime;

        /// <summary>Z 距離現在速度</summary>
        private float _velocityDistanceZ;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>仮想パッド用入力倍率</summary>
        private const float VIRTUAL_PAD_INPUT_MULTIPLIER = 15.0f;
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="cameraModel">制御対象モデル</param>
        /// <param name="maxSpeed">入力時最大速度</param>
        /// <param name="acceleration">入力時加速度</param>
        /// <param name="smoothTime">イベント補間時間</param>
        public CameraDistanceUseCase(
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
        /// 入力値から Z 距離を更新する
        /// </summary>
        /// <param name="input">入力値（+1 / -1 / 0）</param>
        /// <param name="isGamepadUsed">ゲームパッドを使用しているか</param>
        /// <param name="deltaTime">デルタ時間</param>
        public void UpdateInputDistance(in float input, in bool isGamepadUsed, in float deltaTime)
        {
            // --------------------------------------------------
            // デバイス倍率算出
            // --------------------------------------------------
            float speedMultiplier = 1.0f;
            float accelerationMultiplier = 1.0f;

            if (!isGamepadUsed)
            {
                speedMultiplier = VIRTUAL_PAD_INPUT_MULTIPLIER;
                accelerationMultiplier = VIRTUAL_PAD_INPUT_MULTIPLIER;
            }

            // --------------------------------------------------
            // 入力判定
            // --------------------------------------------------
            bool isNoInput = input == 0.0f;

            // --------------------------------------------------
            // 目標速度生成
            // --------------------------------------------------
            float targetVelocityZ;

            if (isNoInput)
            {
                targetVelocityZ = 0.0f;
            }
            else
            {
                targetVelocityZ = input * _maxSpeed * speedMultiplier;
            }

            // --------------------------------------------------
            // 速度補間
            // --------------------------------------------------
            _velocityDistanceZ = UpdateVelocity(
                _velocityDistanceZ,
                targetVelocityZ,
                _acceleration * accelerationMultiplier,
                deltaTime);

            // --------------------------------------------------
            // 距離反映
            // --------------------------------------------------
            float nextDistanceZ =
                _cameraModel.DistanceZ +
                _velocityDistanceZ * deltaTime;

            // モデルへ適用
            _cameraModel.ApplyDistanceZ(nextDistanceZ);

            // --------------------------------------------------
            // OrthographicSize 算出
            // --------------------------------------------------
            float nextOrthographicSize =
                CalculateOrthographicSize(_cameraModel.DistanceZ);

            // モデルへ適用
            _cameraModel.ApplyOrthographicSize(nextOrthographicSize);
        }

        /// <summary>
        /// イベント用 Z 距離を補間しモデルへ反映する
        /// </summary>
        /// <param name="targetDistanceZ">目標 Z 距離</param>
        /// <param name="deltaTime">デルタ時間</param>
        public void UpdateEventDistance(
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

            // モデルへ適用
            _cameraModel.ApplyDistanceZ(nextDistanceZ);

            // --------------------------------------------------
            // OrthographicSize 算出
            // --------------------------------------------------
            float nextOrthographicSize =
                CalculateOrthographicSize(_cameraModel.DistanceZ);

            // モデルへ適用
            _cameraModel.ApplyOrthographicSize(nextOrthographicSize);
        }

        /// <summary>
        /// 距離速度をリセットする
        /// </summary>
        public void ResetVelocity()
        {
            // 現在速度を初期化
            _velocityDistanceZ = 0.0f;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 現在速度を目標速度へ補間する
        /// </summary>
        /// <param name="currentVelocity">現在速度</param>
        /// <param name="targetVelocity">目標速度</param>
        /// <param name="acceleration">加速度</param>
        /// <param name="deltaTime">デルタ時間</param>
        /// <returns>補間後速度</returns>
        private float UpdateVelocity(
            in float currentVelocity,
            in float targetVelocity,
            in float acceleration,
            in float deltaTime)
        {
            // 速度差分を取得
            float velocityDelta =
                targetVelocity - currentVelocity;

            // 今回加算可能な最大速度量
            float maxDelta =
                acceleration * deltaTime;

            // 最大変化量以内で補間
            return Mathf.MoveTowards(
                currentVelocity,
                targetVelocity,
                maxDelta);
        }

        /// <summary>
        /// Z 距離から OrthographicSize を算出する
        /// </summary>
        /// <param name="distanceZ">現在 Z 距離</param>
        /// <returns>算出された OrthographicSize</returns>
        private float CalculateOrthographicSize(in float distanceZ)
        {
            // Z 距離を 0 ～ 1 に正規化
            float normalizedDistance =
                Mathf.InverseLerp(
                    _cameraModel.DistanceZMin,
                    _cameraModel.DistanceZMax,
                    distanceZ);

            // OrthographicSize へ変換
            return Mathf.Lerp(
                _cameraModel.OrthographicSizeMin,
                _cameraModel.OrthographicSizeMax,
                normalizedDistance);
        }
    }
}