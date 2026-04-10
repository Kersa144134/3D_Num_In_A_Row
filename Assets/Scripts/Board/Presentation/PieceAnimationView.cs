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
using BoardSystem.Application;

namespace BoardSystem.Presentation
{
    /// <summary>
    /// 駒アニメーションビュー
    /// </summary>
    public sealed class PieceAnimationView
    {
        // ======================================================
        // 構造体
        // ======================================================

        /// <summary>
        /// 駒移動計画データ
        /// </summary>
        public struct MovePlanData
        {
            /// <summary>対象Transform</summary>
            public Transform Transform;

            /// <summary>開始位置</summary>
            public Vector3 Start;

            /// <summary>終了位置</summary>
            public Vector3 End;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public MovePlanData(
                Transform transform,
                Vector3 start,
                Vector3 end)
            {
                Transform = transform;
                Start = start;
                End = end;
            }
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>落下アニメーションサービス</summary>
        private readonly DropAnimationService _dropAnimation;

        /// <summary>削除時パーティクル</summary>
        private readonly GameObject _deleteParticle;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PieceAnimationView(
            DropAnimationService dropAnimation,
            GameObject deleteParticle)
        {
            // 落下アニメーションサービスを保持する
            _dropAnimation = dropAnimation;

            // 削除パーティクルを保持する
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
            await _dropAnimation.AnimateDropAsync(
                target,
                start,
                end
            );
        }

        /// <summary>
        /// 複数落下アニメーション
        /// </summary>
        public async UniTask PlayMovesAsync(
            List<MovePlanData> plans)
        {
            // タスクリストを生成する
            List<UniTask> tasks =
                new List<UniTask>(plans.Count);

            // 各移動を登録する
            for (int i = 0; i < plans.Count; i++)
            {
                MovePlanData plan = plans[i];

                tasks.Add(
                    _dropAnimation.AnimateDropAsync(
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
            ParticleSystem ps =
                particle.GetComponent<ParticleSystem>();

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