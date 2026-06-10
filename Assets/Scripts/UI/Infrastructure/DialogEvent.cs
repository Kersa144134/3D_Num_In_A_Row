//// ======================================================
//// DialogEvent.cs
//// 作成者   : 高橋一翔
//// 作成日時 : 2026-05-20
//// 更新日時 : 2026-05-20
//// 概要     : ダイアログ入力に応じたイベントを発火するクラス
//// ======================================================

//using System;
//using UnityEngine;
//using UniRx;
//using UISystem.Domain;

//namespace UISystem.Infrastructure
//{
//    /// <summary>
//    /// ダイアログの入力結果をイベントとして外部へ通知するクラス
//    /// </summary>
//    public class DialogEvent : MonoBehaviour
//    {
//        // ======================================================
//        // インスペクタ設定
//        // ======================================================

//        /// <summary>発火するイベント種別</summary>
//        private DialogType _dialogType;

//        // ======================================================
//        // イベントストリーム
//        // ======================================================

//        /// <summary>ダイアログイベント通知ストリーム</summary>
//        private readonly Subject<DialogType> _onEvent = new Subject<DialogType>();

//        /// <summary>ダイアログイベントストリーム</summary>
//        public IObservable<DialogType> OnEvent => _onEvent;

//        // ======================================================
//        // Unity イベント
//        // ======================================================

//        private void OnDestroy()
//        {
//            _onEvent?.Dispose();
//        }

//        // ======================================================
//        // パブリックメソッド
//        // ======================================================

//        /// <summary>
//        /// 設定されたイベントを発火する
//        /// </summary>
//        public void InvokeEvent()
//        {
//            _onEvent.OnNext(_dialogType);
//        }
//    }
//}