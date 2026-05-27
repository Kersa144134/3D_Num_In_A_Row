// ======================================================
// PieceAnimationView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-10
// 更新日時 : 2026-04-10
// 概要     : 駒のアニメーションおよび削除演出を制御するクラス
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using BoardSystem.Domain;

namespace BoardSystem.Application
{
    /// <summary>
    /// 駒アニメーション制御クラス
    /// </summary>
    public sealed class PieceAnimationController
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>落下アニメーション</summary>
        private readonly PieceDropAnimator _dropAnimator = new PieceDropAnimator();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>削除時パーティクル</summary>
        private readonly GameObject _deleteParticle;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PieceAnimationController(in GameObject deleteParticle)
        {
            _deleteParticle = deleteParticle;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 単体落下アニメーション
        /// </summary>
        public async UniTask PlayDropAsync(
            Transform target,
            Vector3 start,
            Vector3 end)
        {
            // アニメーションを実行する
            await _dropAnimator.AnimateDropAsync(
                target,
                start,
                end
            );
        }

        /// <summary>
        /// 複数落下アニメーション
        /// </summary>
        public async UniTask PlayMovesAsync(
            List<PieceMoveData> plans)
        {
            // タスクリストを生成する
            List<UniTask> tasks =
                new List<UniTask>(plans.Count);

            // 各移動を登録する
            for (int i = 0; i < plans.Count; i++)
            {
                PieceMoveData plan = plans[i];

                tasks.Add(
                    _dropAnimator.AnimateDropAsync(
                        plan.Transform,
                        plan.Start,
                        plan.End
                    )
                );
            }

            // 全完了待機
            await UniTask.WhenAll(tasks);
        }

        /// <summary>
        /// 削除演出再生
        /// </summary>
        public void PlayDeleteEffect(in Vector3 position)
        {
            if (_deleteParticle == null)
            {
                return;
            }

            // パーティクル生成
            GameObject particle =
                Object.Instantiate(
                    _deleteParticle,
                    position,
                    Quaternion.identity
                );

            // ParticleSystem 取得
            ParticleSystem ps = particle.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                ps.Play();

                // 再生時間後に破棄
                Object.Destroy(
                    particle,
                    ps.main.duration + ps.main.startLifetime.constantMax
                );
            }
            else
            {
                // 破棄
                Object.Destroy(particle);
            }
        }
    }
}