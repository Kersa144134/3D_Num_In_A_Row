// ======================================================
// CameraPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
// 更新日時 : 2026-04-08
// 概要     : カメラ入力・状態更新・描画反映を管理するプレゼンター
// ======================================================

using CameraSystem.Domain;
using InputSystem;
using SceneSystem.Domain;
using UnityEngine;

namespace CameraSystem.Presentation
{
    /// <summary>
    /// カメラ制御用プレゼンター
    /// </summary>
    public sealed class CameraPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>モデル</summary>
        private CameraModel _cameraModel;

        /// <summary>ビュー</summary>
        private CameraView _cameraView;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("回転速度設定")]
        /// <summary>水平回転速度</summary>
        [SerializeField]
        private float _rotationSpeedY = 120.0f;

        /// <summary>垂直回転速度</summary>
        [SerializeField]
        private float _rotationSpeedX = 120.0f;

        [Header("回転制限設定")]
        /// <summary>X 回転の最小値</summary>
        [SerializeField]
        private float _minRotationX = -90.0f;

        /// <summary>X 回転の最大値</summary>
        [SerializeField]
        private float _maxRotationX = 90.0f;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            // Transform の回転を Euler 角で取得
            Vector3 euler = transform.rotation.eulerAngles;

            // -180～180 の範囲に正規化
            float initialRotationX = NormalizeAngle(euler.x);
            float initialRotationY = NormalizeAngle(euler.y);

            _cameraModel = new CameraModel(
                initialRotationX,
                initialRotationY,
                _minRotationX,
                _maxRotationX
            );
            _cameraView = new CameraView(transform);

            // インスタンスからコンポーネント取得
            _inputManager = InputManager.Instance;
        }

        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // --------------------------------------------------
            // 入力取得
            // --------------------------------------------------
            // 左右入力を取得する
            float inputHorizontal = _inputManager.LeftStick.x;

            // 上下入力を取得する
            float inputVertical = _inputManager.LeftStick.y;

            // --------------------------------------------------
            // モデル更新
            // --------------------------------------------------
            // 入力に応じた回転を加算する
            float rotationX = inputVertical * _rotationSpeedX * unscaledDeltaTime;
            float rotationY = inputHorizontal * _rotationSpeedY * unscaledDeltaTime;

            _cameraModel.AddRotationX(rotationX);
            _cameraModel.AddRotationY(rotationY);

            // --------------------------------------------------
            // ビュー反映
            // --------------------------------------------------
            _cameraView.ApplyRotation(_cameraModel.RotationX, _cameraModel.RotationY);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 角度を -180 ～ 180 の範囲に正規化する
        /// </summary>
        private float NormalizeAngle(in float angle)
        {
            // 360 で剰余を取ることで範囲を圧縮する
            float result = angle % 360.0f;

            // 180 を超えた場合は負方向へ折り返す
            if (result > 180.0f)
            {
                result -= 360.0f;
            }

            // -180 未満の場合は正方向へ折り返す
            if (result < -180.0f)
            {
                result += 360.0f;
            }

            return result;
        }
    }
}