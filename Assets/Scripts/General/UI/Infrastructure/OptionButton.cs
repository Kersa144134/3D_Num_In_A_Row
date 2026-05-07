// ======================================================
// OptionButton.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-07
// 概要     : オプションボタンの入力通知を担当するクラス
// ======================================================

using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using OptionSystem.Domain;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// オプションボタンの入力通知を担当するクラス
    /// </summary>
    public sealed class OptionButton :
        MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>
        /// インスペクタ設定用データ
        /// </summary>
        [SerializeField]
        private OptionButtonConfig _config;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 実行時データ
        /// </summary>
        private OptionButtonData _data;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>クリック通知用 Subject</summary>
        private readonly Subject<OptionButtonData> _onClick = new Subject<OptionButtonData>();

        /// <summary>クリックイベント用ストリーム</summary>
        public IObservable<OptionButtonData> OnClickAsObservable => _onClick;

        /// <summary>ホバー開始通知用 Subject</summary>
        private readonly Subject<OptionButtonData> _onEnter = new Subject<OptionButtonData>();

        /// <summary>ホバー開始イベント用ストリーム</summary>
        public IObservable<OptionButtonData> OnEnterAsObservable => _onEnter;

        /// <summary>ホバー終了通知用 Subject</summary>
        private readonly Subject<OptionButtonData> _onExit = new Subject<OptionButtonData>();

        /// <summary>ホバー終了イベント用ストリーム</summary>
        public IObservable<OptionButtonData> OnExitAsObservable => _onExit;

        // ======================================================
        // Unity イベント
        // ======================================================

        /// <summary>
        /// オブジェクト破棄時の後始末
        /// ・Subjectの解放
        /// </summary>
        private void OnDestroy()
        {
            _onClick.Dispose();
            _onEnter.Dispose();
            _onExit.Dispose();
        }

        /// <summary>
        /// クリック時処理
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick.OnNext(_data);
        }

        /// <summary>
        /// ホバー開始時処理
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onEnter.OnNext(_data);
        }

        /// <summary>
        /// ホバー終了時処理
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            _onExit.OnNext(_data);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ボタンデータを設定する
        /// </summary>
        public void Initialize()
        {
            // 自分自身を紐づけて実行データ化
            _data = _config.ToRuntimeData(GetComponent<Button>());
        }
    }
}