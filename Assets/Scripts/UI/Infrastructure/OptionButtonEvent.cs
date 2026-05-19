// ======================================================
// OptionButtonEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-09
// 概要     : オプションボタンの入力通知を担当するクラス
// ======================================================

using OptionSystem.Domain;
using System;
using UISystem.Domain;
using UniRx;
using UnityEngine;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// オプションボタンの入力通知を担当するクラス
    /// </summary>
    public sealed class OptionButtonEvent : BaseButtonEvent
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>インスペクタ設定用オプションデータ</summary>
        [SerializeField]
        private OptionButtonConfig _config;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>実行時オプションデータ</summary>
        private OptionButtonData _data;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>実行時オプションデータ</summary>
        public OptionButtonData Data => _data;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>
        /// オプションクリック通知用 Subject
        /// </summary>
        private readonly Subject<UIClickType> _onOptionClick = new Subject<UIClickType>();

        /// <summary>
        /// オプションクリックイベントストリーム
        /// </summary>
        public IObservable<UIClickType> OnOptionClick => _onOptionClick;

        // ======================================================
        // Unityイベント
        // ======================================================

        protected override void Awake()
        {
            base.Awake();

            // Runtime データを生成
            _data = _config.ToRuntimeData(Button);

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

            // Optionクリック通知を解放
            _onOptionClick.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 基底イベントを購読する
        /// </summary>
        private void SubscribeBaseEvents()
        {
            // 基底クリックイベントをオプション用へ変換
            OnClick
                .Subscribe(type =>
                {
                    _onOptionClick.OnNext(type);
                })
                .AddTo(this);
        }
    }
}