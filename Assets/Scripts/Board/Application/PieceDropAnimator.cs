// ======================================================
// PieceDropAnimator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-02
// 更新日時 : 2026-04-02
// 概要     : 駒の落下アニメーション
// ======================================================

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BoardSystem.Application
{
    /// <summary>
    /// 駒落下アニメーション
    /// </summary>
    public sealed class PieceDropAnimator
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
            // 現在の落下速度
            float velocity = 0f;

            // 現在位置
            Vector3 position = start;

            try
            {
                // --------------------------------------------------
                // 落下処理
                // --------------------------------------------------
                while (true)
                {
                    // フレーム間の経過時間を取得
                    float deltaTime = Time.deltaTime;

                    // 重力加速度を加算して速度を更新
                    velocity += GRAVITY * deltaTime;

                    // 最大落下速度を超えないように制限
                    if (velocity > MAX_FALL_SPEED)
                    {
                        // 上限値にクランプ
                        velocity = MAX_FALL_SPEED;
                    }

                    // 現在位置の Y 座標を速度に応じて減少
                    position.y -= velocity * deltaTime;

                    // 目標位置を超えた、または十分近づいた場合に終了
                    if (position.y <= end.y + ARRIVAL_THRESHOLD)
                    {
                        // ループを抜ける
                        break;
                    }

                    // 計算した位置を Transform に反映
                    pieceTransform.position = position;

                    // 次のフレームまで非同期で待機
                    await UniTask.Yield();
                }
            }
            finally
            {
                // --------------------------------------------------
                // 最終位置補正
                // --------------------------------------------------
                pieceTransform.position = end;
            }
        }
    }
}