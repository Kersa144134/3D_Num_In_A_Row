// ======================================================
// CameraModel.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-08
// 更新日時 : 2026-04-08
// 概要     : カメラの回転状態を管理するモデル
// ======================================================

using UnityEngine;

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

        /// <summary>
        /// X 軸回転
        /// </summary>
        private float _rotationX;

        /// <summary>
        /// Y 軸回転
        /// </summary>
        private float _rotationY;

        /// <summary>
        /// X 回転の最小値
        /// </summary>
        private readonly float _minX;

        /// <summary>
        /// X 回転の最大値
        /// </summary>
        private readonly float _maxX;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="initialRotationX">初期X回転（-180～180想定）</param>
        /// <param name="initialRotationY">初期Y回転（-180～180想定）</param>
        /// <param name="minX">X回転の最小値</param>
        /// <param name="maxX">X回転の最大値</param>
        public CameraModel(
            in float initialRotationX,
            in float initialRotationY,
            in float minX,
            in float maxX)
        {
            _rotationX = initialRotationX;
            _rotationY = initialRotationY;
            _minX = minX;
            _maxX = maxX;

            // 設定された範囲でクランプする
            _rotationX = Clamp(_rotationX, _minX, _maxX);
        }

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 現在の X 回転値を取得する
        /// </summary>
        public float RotationX
        {
            get { return _rotationX; }
        }

        /// <summary>
        /// 現在の Y 回転値を取得する
        /// </summary>
        public float RotationY
        {
            get { return _rotationY; }
        }

        // ======================================================
        // 回転更新
        // ======================================================

        /// <summary>
        /// X 軸回転を加算する
        /// </summary>
        /// <param name="value">加算値</param>
        public void AddRotationX(in float value)
        {
            // 入力値を加算する
            _rotationX += value;

            // 設定された範囲でクランプする
            _rotationX = Clamp(_rotationX, _minX, _maxX);
        }

        /// <summary>
        /// Y 軸回転を加算する
        /// </summary>
        /// <param name="value">加算値</param>
        public void AddRotationY(in float value)
        {
            // 入力値を加算する
            _rotationY += value;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 値を範囲内に制限する
        /// </summary>
        private float Clamp(in float value, in float min, in float max)
        {
            // 最小値未満の場合は最小値にする
            if (value < min)
            {
                return min;
            }

            // 最大値を超える場合は最大値にする
            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}