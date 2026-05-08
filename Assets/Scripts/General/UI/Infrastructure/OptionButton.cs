// ======================================================
// OptionButton.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-08
// 概要     : オプションボタンのクリック・ホバー・選択入力通知を担当するクラス
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;
using OptionSystem.Domain;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// オプションボタンの入力通知を担当するクラス
    /// </summary>
    public sealed class OptionButton :
        MonoBehaviour,
        IDisposable,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        ISelectHandler,
        IDeselectHandler,
        ISubmitHandler
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

        /// <summary>
        /// Dispose 実行済みフラグ
        /// </summary>
        private bool _isDisposed;

        // ======================================================
        // UniRx 変数
        // ======================================================

        // --------------------------------------------------
        // クリック
        // --------------------------------------------------
        /// <summary>
        /// クリック通知用 Subject
        /// </summary>
        private readonly Subject<OptionButtonData> _onClick =
            new Subject<OptionButtonData>();

        /// <summary>
        /// クリックイベントストリーム
        /// </summary>
        public IObservable<OptionButtonData> OnClickAsObservable => _onClick;

        // --------------------------------------------------
        // ホバー
        // --------------------------------------------------
        /// <summary>
        /// ホバー開始通知用 Subject
        /// </summary>
        private readonly Subject<OptionButtonData> _onHoverEnter =
            new Subject<OptionButtonData>();

        /// <summary>
        /// ホバー開始イベントストリーム
        /// </summary>
        public IObservable<OptionButtonData> OnHoverEnterAsObservable => _onHoverEnter;

        /// <summary>
        /// ホバー終了通知用 Subject
        /// </summary>
        private readonly Subject<OptionButtonData> _onHoverExit =
            new Subject<OptionButtonData>();

        /// <summary>
        /// ホバー終了イベントストリーム
        /// </summary>
        public IObservable<OptionButtonData> OnHoverExitAsObservable => _onHoverExit;

        // --------------------------------------------------
        // 選択
        // --------------------------------------------------
        /// <summary>
        /// 選択開始通知用 Subject
        /// </summary>
        private readonly Subject<OptionButtonData> _onSelectEnter =
            new Subject<OptionButtonData>();

        /// <summary>
        /// 選択開始イベントストリーム
        /// </summary>
        public IObservable<OptionButtonData> OnSelectEnterAsObservable => _onSelectEnter;

        /// <summary>
        /// 選択終了通知用 Subject
        /// </summary>
        private readonly Subject<OptionButtonData> _onSelectExit =
            new Subject<OptionButtonData>();

        /// <summary>
        /// 選択終了イベントストリーム
        /// </summary>
        public IObservable<OptionButtonData> OnSelectExitAsObservable => _onSelectExit;

        // ======================================================
        // Unityイベント
        // ======================================================

        private void Awake()
        {
            Button button = GetComponent<Button>();

            // Runtime データを生成
            _data = _config.ToRuntimeData(button);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        // ======================================================
        // IDisposable
        // ======================================================

        /// <summary>
        /// リソースを解放する
        /// </summary>
        public void Dispose()
        {
            // 多重実行防止
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _onClick.Dispose();
            _onHoverEnter.Dispose();
            _onHoverExit.Dispose();
            _onSelectEnter.Dispose();
            _onSelectExit.Dispose();
        }

        // ======================================================
        // EventSystem 入力イベント
        // ======================================================

        // --------------------------------------------------
        // クリック
        // --------------------------------------------------
        /// <summary>
        /// マウスクリック入力
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnPointerClick(
            PointerEventData eventData)
        {
            _onClick.OnNext(_data);
        }

        /// <summary>
        /// 決定入力
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnSubmit(
            BaseEventData eventData)
        {
            _onClick.OnNext(_data);
        }

        // --------------------------------------------------
        // ホバー
        // --------------------------------------------------
        /// <summary>
        /// マウスホバー開始
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnPointerEnter(
            PointerEventData eventData)
        {
            _onHoverEnter.OnNext(_data);
        }

        /// <summary>
        /// マウスホバー終了
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnPointerExit(
            PointerEventData eventData)
        {
            _onHoverExit.OnNext(_data);
        }

        // --------------------------------------------------
        // 選択
        // --------------------------------------------------
        /// <summary>
        /// EventSystem 選択開始
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnSelect(
            BaseEventData eventData)
        {
            _onSelectEnter.OnNext(_data);
        }

        /// <summary>
        /// EventSystem 選択終了
        /// </summary>
        /// <param name="eventData">イベント情報</param>
        public void OnDeselect(
            BaseEventData eventData)
        {
            _onSelectExit.OnNext(_data);
        }
    }
}