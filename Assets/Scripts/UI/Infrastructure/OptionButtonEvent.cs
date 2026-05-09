// ======================================================
// OptionButtonEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-09
// 概要     : オプションボタンの入力通知を担当するクラス
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using OptionSystem.Domain;

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
        /// Optionクリック通知用 Subject
        /// </summary>
        private readonly Subject<OptionButtonData> _onOptionClick = new Subject<OptionButtonData>();

        /// <summary>
        /// Optionクリックイベントストリーム
        /// </summary>
        public IObservable<OptionButtonData> OnOptionClickAsObservable => _onOptionClick;

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
            // 基底クリックイベントを Option 用へ変換
            OnClickAsObservable
                .Subscribe(_ =>
                {
                    _onOptionClick.OnNext(_data);
                })
                .AddTo(this);
        }
    }
}