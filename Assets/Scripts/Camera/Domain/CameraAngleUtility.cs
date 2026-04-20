// ======================================================
// CameraAngleUtility.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-20
// 更新日時 : 2026-04-20
// 概要     : 角度に関する共通計算ユーティリティ
// ======================================================

namespace CameraSystem.Domain
{
    /// <summary>
    /// 角度計算ユーティリティ
    /// </summary>
    public sealed class CameraAngleUtility
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 角度を -180 ～ 180 の範囲に正規化する
        /// </summary>
        /// <param name="angle">変換対象の角度</param>
        /// <returns>正規化後の角度</returns>
        public float NormalizeAngle(in float angle)
        {
            // 360 で剰余を取得
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