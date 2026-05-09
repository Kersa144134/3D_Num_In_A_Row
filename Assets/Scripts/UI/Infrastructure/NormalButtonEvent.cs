// ======================================================
// NormalButtonEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-09
// 更新日時 : 2026-05-09
// 概要     : 通常ボタンの入力通知を担当するクラス
// ======================================================

using System;
using UniRx;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// 通常ボタンの入力通知を担当するクラス
    /// </summary>
    public sealed class NormalButtonEvent : BaseButtonEvent
    {
        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>
        /// Normal クリック通知用 Subject
        /// </summary>
        private readonly Subject<Unit> _onNormalClick = new Subject<Unit>();

        /// <summary>
        /// Normal クリックイベントストリーム
        /// </summary>
        public IObservable<Unit> OnNormalClickAsObservable => _onNormalClick;

        // ======================================================
        // Unityイベント
        // ======================================================

        protected override void Awake()
        {
            base.Awake();

            // 基底イベント購読
            SubscribeBaseEvents();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public override void Dispose()
        {
            // 基底 Dispose 実行
            base.Dispose();

            // Normal クリック通知を解放
            _onNormalClick.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 基底イベントを購読する
        /// </summary>
        private void SubscribeBaseEvents()
        {
            // 基底クリックイベントを Normal 用へ変換
            OnClickAsObservable
                .Subscribe(_ =>
                {
                    _onNormalClick.OnNext(Unit.Default);
                })
                .AddTo(this);
        }
    }
}