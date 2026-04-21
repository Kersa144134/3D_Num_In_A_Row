// ======================================================
// CameraModel.cs
// 作成者   : 高橋一翔
// 更新日時 : 2026-04-21
// 概要     : カメラの回転状態（現在値 + 目標値）を管理するモデル
// ======================================================

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

        /// <summary>X 軸現在回転</summary>
        private float _rotationX;

        /// <summary>Y 軸現在回転</summary>
        private float _rotationY;

        /// <summary>X 回転の最小値</summary>
        private readonly float _minX;

        /// <summary>X 回転の最大値</summary>
        private readonly float _maxX;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
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
        }

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在の X 回転値</summary>
        public float RotationX
        {
            get { return _rotationX; }
        }

        /// <summary>現在の Y 回転値</summary>
        public float RotationY
        {
            get { return _rotationY; }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// X 回転を直接設定する
        /// </summary>
        public void SetRotationX(in float value)
        {
            _rotationX = Clamp(value, _minX, _maxX);
        }

        /// <summary>
        /// Y 回転を直接設定する
        /// </summary>
        public void SetRotationY(in float value)
        {
            _rotationY = value;
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