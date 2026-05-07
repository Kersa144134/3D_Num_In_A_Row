// ======================================================
// OptionButton.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-07
// 概要     : オプションボタンのクリック・フォーカス入力通知を担当するクラス
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
        /// ・ボタン識別情報などを保持
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
        /// Buttonキャッシュ
        /// </summary>
        private Button _cachedButton;

        // ======================================================
        // UniRx 変数（クリック）
        // ======================================================

        /// <summary>クリック通知用Subject</summary>
        private readonly Subject<OptionButtonData> _onClick = new Subject<OptionButtonData>();

        /// <summary>クリックイベントストリーム</summary>
        public IObservable<OptionButtonData> OnClickAsObservable => _onClick;

        // ======================================================
        // UniRx 変数（フォーカス）
        // ======================================================

        /// <summary>フォーカス開始通知用Subject</summary>
        private readonly Subject<OptionButtonData> _onFocusEnter = new Subject<OptionButtonData>();

        /// <summary>フォーカス開始ストリーム</summary>
        public IObservable<OptionButtonData> OnFocusEnterAsObservable => _onFocusEnter;

        /// <summary>フォーカス終了通知用Subject</summary>
        private readonly Subject<OptionButtonData> _onFocusExit = new Subject<OptionButtonData>();

        /// <summary>フォーカス終了ストリーム</summary>
        public IObservable<OptionButtonData> OnFocusExitAsObservable => _onFocusExit;

        // ======================================================
        // Unityイベント
        // ======================================================

        private void Awake()
        {
            // ボタンコンポーネントをキャッシュ
            _cachedButton = GetComponent<Button>();

            // Runtime データ生成
            _data = _config.ToRuntimeData(_cachedButton);
        }

        private void OnDestroy()
        {
            // Subject 解放
            _onClick.Dispose();
            _onFocusEnter.Dispose();
            _onFocusExit.Dispose();
        }

        // ======================================================
        // EventSystem 入力イベント
        // ======================================================

        /// <summary>
        /// クリック入力
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick.OnNext(_data);
        }

        /// <summary>
        /// マウスフォーカス開始
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _onFocusEnter.OnNext(_data);
        }

        /// <summary>
        /// マウスフォーカス終了
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            _onFocusExit.OnNext(_data);
        }

        /// <summary>
        /// 決定入力
        /// </summary>
        public void OnSubmit(BaseEventData eventData)
        {
            _onClick.OnNext(_data);
        }

        /// <summary>
        /// フォーカス開始
        /// </summary>
        public void OnSelect(BaseEventData eventData)
        {
            _onFocusEnter.OnNext(_data);
        }

        /// <summary>
        /// フォーカス終了
        /// </summary>
        public void OnDeselect(BaseEventData eventData)
        {
            _onFocusExit.OnNext(_data);
        }
    }
}