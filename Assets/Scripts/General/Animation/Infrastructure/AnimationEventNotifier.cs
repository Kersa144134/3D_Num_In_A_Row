// ======================================================
// AnimationEventNotifier.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-22
// 更新日時 : 2026-05-22
// 概要     : AnimationEvent によるイベント通知クラス
// ======================================================

using System;
using UnityEngine;
using UniRx;

namespace AnimationSystem.Infrastructure
{
    /// <summary>
    /// AnimationEvent を受け取り、イベント通知するクラス
    /// </summary>
    public sealed class AnimationEventNotifier : MonoBehaviour
    {
        // ======================================================
        // UniRx イベント
        // ======================================================

        /// <summary>
        /// アニメーションチェックポイントイベント用 Subject
        /// </summary>
        private readonly Subject<Unit> _onAnimationCheckPointSubject = new Subject<Unit>();

        /// <summary>
        /// アニメーション終了イベント用 Subject
        /// </summary>
        private readonly Subject<Unit> _onAnimationEndSubject = new Subject<Unit>();

        /// <summary>
        /// アニメーションチェックポイントイベントストリーム
        /// </summary>
        public IObservable<Unit> OnAnimationCheckPoint => _onAnimationCheckPointSubject;

        /// <summary>
        /// アニメーション終了イベントストリーム
        /// </summary>
        public IObservable<Unit> OnAnimationEnd => _onAnimationEndSubject;

        // ======================================================
        // Unity イベント
        // ======================================================

        private void OnDestroy()
        {
            _onAnimationCheckPointSubject.OnCompleted();
            _onAnimationCheckPointSubject.Dispose();

            _onAnimationEndSubject.OnCompleted();
            _onAnimationEndSubject.Dispose();
        }

        // ======================================================
        // アニメーションイベント
        // ======================================================

        /// <summary>
        /// AnimationClip のチェックポイントタイミングで呼び出されるメソッド
        /// </summary>
        public void NotifyAnimationCheckPoint()
        {
            _onAnimationCheckPointSubject.OnNext(Unit.Default);
        }

        /// <summary>
        /// AnimationClip の終了タイミングで呼び出されるメソッド
        /// </summary>
        public void NotifyAnimationEnd()
        {
            _onAnimationEndSubject.OnNext(Unit.Default);
        }
    }
}