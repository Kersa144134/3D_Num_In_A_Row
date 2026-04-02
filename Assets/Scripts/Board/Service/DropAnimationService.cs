// ======================================================
// DropAnimationService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-02
// 更新日時 : 2026-04-02
// 概要     : 駒の落下アニメーションサービス
// ======================================================

using UnityEngine;
using Cysharp.Threading.Tasks;

namespace BoardSystem.Service
{
    /// <summary>
    /// 駒落下アニメーションサービス
    /// </summary>
    public sealed class DropAnimationService
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>落下最大速度</summary>
        private const float MAX_FALL_SPEED = 10f;

        /// <summary>重力加速度（加速率）</summary>
        private const float GRAVITY = 9.8f;

        /// <summary>到達判定用の閾値</summary>
        private const float ARRIVAL_THRESHOLD = 0.01f;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 駒を物理風に落下させるアニメーション
        /// </summary>
        /// <param name="pieceTransform">落下対象の Transform</param>
        /// <param name="start">開始位置</param>
        /// <param name="end">到達位置（最終位置）</param>
        public async UniTask AnimateDropAsync(
            Transform pieceTransform,
            Vector3 start,
            Vector3 end)
        {
            // --------------------------------------------------
            // 初期化処理
            // --------------------------------------------------
            float velocity = 0f;
            Vector3 position = start;

            // --------------------------------------------------
            // 落下処理
            // --------------------------------------------------
            while (true)
            {
                // フレーム間の経過時間取得
                float deltaTime = Time.deltaTime;

                // 重力による加速
                velocity += GRAVITY * deltaTime;

                // 最大速度制限
                if (velocity > MAX_FALL_SPEED)
                {
                    velocity = MAX_FALL_SPEED;
                }

                // Y 座標更新
                position.y -= velocity * deltaTime;

                // 目標座標に到達または超過したらループ終了
                if (position.y <= end.y + ARRIVAL_THRESHOLD)
                {
                    break;
                }

                // Transform に反映
                pieceTransform.position = position;

                // 次フレームまで待機
                await UniTask.Yield();
            }

            // --------------------------------------------------
            // 最終位置補正
            // --------------------------------------------------
            pieceTransform.position = end;
        }
    }
}