// ======================================================
// CameraRotationUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-20
// 更新日時 : 2026-04-20
// 概要     : カメラ回転の入力計算とモデル反映を担うユースケース
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

        /// <summary>水平回転速度</summary>
        private readonly float _rotationSpeedY;

        /// <summary>垂直回転速度</summary>
        private readonly float _rotationSpeedX;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CameraRotationUseCase(
            CameraModel cameraModel,
            float rotationSpeedX,
            float rotationSpeedY)
        {
            _cameraModel = cameraModel;
            _rotationSpeedX = rotationSpeedX;
            _rotationSpeedY = rotationSpeedY;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力値から回転を計算しモデルへ反映する
        /// </summary>
        /// <param name="input">スティック入力値</param>
        /// <param name="deltaTime">非スケールデルタ時間</param>
        public void UpdateRotation(in Vector2 input, in float deltaTime)
        {
            // --------------------------------------------------
            // 回転量計算
            // --------------------------------------------------
            // 垂直回転量を計算する
            float rotationX = input.y * _rotationSpeedX * deltaTime;

            // 水平回転量を計算する
            float rotationY = input.x * _rotationSpeedY * deltaTime;

            // --------------------------------------------------
            // モデル更新
            // --------------------------------------------------
            // X 回転をモデルへ加算する
            _cameraModel.AddRotationX(rotationX);

            // Y 回転をモデルへ加算する
            _cameraModel.AddRotationY(rotationY);
        }
    }
}