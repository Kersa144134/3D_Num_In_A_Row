// ======================================================
// CameraModel.cs
// 作成者   : 高橋一翔
// 更新日時 : 2026-04-21
// 概要     : カメラの距離と回転状態を管理するモデル
// ======================================================

using UnityEngine;
using UnityEngine.UIElements;

namespace CameraSystem.Domain
{
    /// <summary>
    /// カメラの回転状態を管理するモデル
    /// </summary>
    public class CameraModel
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在のカメラ位置</summary>
        private Vector3 _position;

        /// <summary>X 軸回転</summary>
        private float _rotationX;

        /// <summary>Y 軸回転</summary>
        private float _rotationY;

        /// <summary>Z 軸距離</summary>
        private float _distanceZ;

        /// <summary>OrthographicSize</summary>
        private float _orthographicSize;

        /// <summary>Z 距離の最小値</summary>
        private readonly float _distanceZMin;

        /// <summary>Z 距離の最大値</summary>
        private readonly float _distanceZMax;

        /// <summary>ピッチ角の最小値</summary>
        private readonly float _pitchMinX;

        /// <summary>ピッチ角の最大値</summary>
        private readonly float _pitchMaxX;

        /// <summary>OrthographicSizeの最小値</summary>
        private readonly float _orthographicSizeMin;

        /// <summary>OrthographicSizeの最大値</summary>
        private readonly float _orthographicSizeMax;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        public CameraModel(
            in Vector3 initialPosition,
            in float initialRotationX,
            in float initialRotationY,
            in float initialDistanceZ,
            in float pitchMinX,
            in float pitchMaxX,
            in float distanceZMin,
            in float distanceZMax,
            in float orthographicSizeMin,
            in float orthographicSizeMax)
        {
            _position = initialPosition;
            _rotationX = initialRotationX;
            _rotationY = initialRotationY;
            _distanceZ = initialDistanceZ;
            _pitchMinX = pitchMinX;
            _pitchMaxX = pitchMaxX;
            _distanceZMin = distanceZMin;
            _distanceZMax = distanceZMax;
            _orthographicSizeMin = orthographicSizeMin;
            _orthographicSizeMax = orthographicSizeMax;
        }

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在のカメラ位置</summary>
        public Vector3 Position => _position;

        /// <summary>現在の Z 距離</summary>
        public float DistanceZ => _distanceZ;

        /// <summary>現在の X 回転値</summary>
        public float RotationX => _rotationX;

        /// <summary>現在の Y 回転値</summary>
        public float RotationY => _rotationY;

        /// <summary>現在の OrthographicSize</summary>
        public float OrthographicSize => _orthographicSize;

        /// <summary>Z 距離の最小値</summary>
        public float DistanceZMin => _distanceZMin;

        /// <summary>Z 距離の最大値</summary>
        public float DistanceZMax => _distanceZMax;

        /// <summary>ピッチ角の最小値</summary>
        public float PitchMinX => _pitchMinX;

        /// <summary>ピッチ角の最大値</summary>
        public float PitchMaxX => _pitchMaxX;

        /// <summary>OrthographicSize の最小値</summary>
        public float OrthographicSizeMin => _orthographicSizeMin;

        /// <summary>OrthographicSize の最大値</summary>
        public float OrthographicSizeMax => _orthographicSizeMax;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// カメラ位置を適用する
        /// </summary>
        public void ApplyPosition(in Vector3 position)
        {
            _position = position;
        }
        
        /// <summary>
        /// X 回転を直接設定する
        /// </summary>
        public void ApplyRotationX(in float value)
        {
            _rotationX = Clamp(value, _pitchMinX, _pitchMaxX);
        }

        /// <summary>
        /// Y 回転を直接設定する
        /// </summary>
        public void ApplyRotationY(in float value)
        {
            _rotationY = value;
        }

        /// <summary>
        /// Z 距離を直接設定する
        /// </summary>
        public void ApplyDistanceZ(in float value)
        {
            _distanceZ = Clamp(value, _distanceZMin, _distanceZMax);
        }

        /// <summary>
        /// OrthographicSizeを直接設定する
        /// </summary>
        public void ApplyOrthographicSize(in float value)
        {
            _orthographicSize = Clamp(value, _orthographicSizeMin, _orthographicSizeMax);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 値を範囲内に制限する
        /// </summary>
        private float Clamp(in float value, in float min, in float max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}