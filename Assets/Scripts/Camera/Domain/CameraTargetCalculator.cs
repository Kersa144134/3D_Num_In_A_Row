// ======================================================
// CameraTargetCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-23
// 更新日時 : 2026-06-23
// 概要     : ライン情報からカメラ目標位置と目標角度を計算するクラス
// ======================================================

using UnityEngine;

namespace CameraSystem.Domain
{
    /// <summary>
    /// カメラ目標位置、角度計算クラス
    /// </summary>
    public sealed class CameraTargetCalculator
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ラインがほぼ Y 軸方向かを判定するための閾値</summary>
        private const float VERTICAL_LINE_SQR_MAGNITUDE_THRESHOLD = 0.0001f;

        /// <summary>ライン中心が原点付近かを判定するための閾値</summary>
        private const float CENTER_POSITION_SQR_MAGNITUDE_THRESHOLD = 0.001f;

        // ======================================================
        // 計算処理
        // ======================================================

        /// <summary>
        /// ライン情報からカメラ制御用の目標位置と目標角度を計算する
        /// </summary>
        /// <param name="startPosition">ライン開始位置</param>
        /// <param name="endPosition">ライン終了位置</param>
        /// <param name="targetPosition">計算結果の目標位置</param>
        /// <param name="targetAngle">計算結果の目標方向</param>
        public void Calculate(
            in Vector3 startPosition,
            in Vector3 endPosition,
            out Vector3 targetPosition,
            out Vector3 targetAngle)
        {
            // --------------------------------------------------
            // 中心位置算出
            // --------------------------------------------------
            // ラインの中間点をカメラ追従対象の基準位置とする
            Vector3 centerPosition = (startPosition + endPosition) * 0.5f;

            // 計算結果として目標位置へ反映
            targetPosition = centerPosition;

            // --------------------------------------------------
            // ライン方向算出
            // --------------------------------------------------
            // 始点から終点への方向ベクトルを取得
            Vector3 lineDirection = (endPosition - startPosition).normalized;

            // XZ平面に投影した方向成分を抽出（水平判定用）
            Vector2 lineDirectionXZ = new Vector2(
                lineDirection.x,
                lineDirection.z);

            // --------------------------------------------------
            // 垂直ライン判定
            // --------------------------------------------------
            // XZ成分が極小の場合は縦方向ラインとみなす
            if (lineDirectionXZ.sqrMagnitude < VERTICAL_LINE_SQR_MAGNITUDE_THRESHOLD)
            {
                // --------------------------------------------------
                // 原点付近かどうかで分岐
                // --------------------------------------------------
                if (centerPosition.sqrMagnitude < CENTER_POSITION_SQR_MAGNITUDE_THRESHOLD)
                {
                    // 原点付近の場合はデフォルト前方を向く
                    targetAngle = Vector3.forward;
                }
                else
                {
                    // 原点からライン中心方向を向く
                    targetAngle = centerPosition.normalized;
                }

                return;
            }

            // --------------------------------------------------
            // 通常ライン方向処理
            // --------------------------------------------------
            // ラインに対して垂直な方向を算出（カメラ横方向）
            Vector3 perpendicularDirection = Vector3.Cross(
                lineDirection,
                Vector3.up).normalized;

            // ライン中心から原点への方向ベクトル
            Vector3 centerToOriginDirection =
                (Vector3.zero - centerPosition).normalized;

            // 原点側を向いている場合は反転して外向きに補正
            if (Vector3.Dot(perpendicularDirection, centerToOriginDirection) > 0.0f)
            {
                perpendicularDirection = -perpendicularDirection;
            }

            // 最終的なカメラ目標方向として採用
            targetAngle = perpendicularDirection;
        }
    }
}