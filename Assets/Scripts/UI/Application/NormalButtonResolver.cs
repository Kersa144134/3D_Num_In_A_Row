// ======================================================
// UIActionButtonResolver.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-06-09
// 概要     : UI ボタンの参照解決クラス
// ======================================================

using System.Collections.Generic;
using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// UI ボタンの参照解決を行うクラス
    /// </summary>
    public sealed class UIActionButtonResolver
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// --------------------------------------------------
        // ダイアログボタン辞書
        // --------------------------------------------------
        /// <summary>
        /// 正引き辞書
        /// (UIActionType, DialogType) → NormalButtonEvent
        /// </summary>
        private readonly Dictionary<(UIActionType, DialogType), NormalButtonEvent> _dialogForwardTable;

        /// <summary>
        /// 逆引き辞書
        /// NormalButtonEvent → (UIActionType, DialogType)
        /// </summary>
        private readonly Dictionary<NormalButtonEvent, (UIActionType, DialogType)> _dialogReverseTable;

        // ======================================================
        // ======================================================

        /// --------------------------------------------------
        // 通常ボタン辞書
        // --------------------------------------------------
        /// <summary>
        /// 正引き辞書
        /// UIActionType → NormalButtonEvent
        /// </summary>
        private readonly Dictionary<UIActionType, NormalButtonEvent> _normalForwardTable;

        /// <summary>
        /// 逆引き辞書
        /// NormalButtonEvent → UIActionType
        /// </summary>
        private readonly Dictionary<NormalButtonEvent, UIActionType> _normalReverseTable;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UIActionButtonResolver(
            in Dictionary<(UIActionType, DialogType), NormalButtonEvent> dialogTable,
            in Dictionary<UIActionType, NormalButtonEvent> normalTable)
        {
            // --------------------------------------------------
            // ダイアログテーブル保持
            // --------------------------------------------------
            _dialogForwardTable = dialogTable;

            _dialogReverseTable = new Dictionary<NormalButtonEvent, (UIActionType, DialogType)>(dialogTable.Count);

            foreach (KeyValuePair<(UIActionType, DialogType), NormalButtonEvent> pair in dialogTable)
            {
                _dialogReverseTable[pair.Value] = pair.Key;
            }
            
            // --------------------------------------------------
            // 通常テーブル保持
            // --------------------------------------------------
            _normalForwardTable = normalTable;

            _normalReverseTable = new Dictionary<NormalButtonEvent, UIActionType>(normalTable.Count);

            foreach (KeyValuePair<UIActionType, NormalButtonEvent> pair in normalTable)
            {
                _normalReverseTable[pair.Value] = pair.Key;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 通常ボタンイベントを取得
        /// </summary>
        /// <param name="type">UIアクション種別</param>
        /// <returns>対応するボタンイベント</returns>
        public NormalButtonEvent GetNormalButton(in UIActionType type)
        {
            // 指定された UI アクション種別に対応するボタンイベントを取得
            if (_normalForwardTable.TryGetValue(type, out NormalButtonEvent buttonEvent))
            {
                return buttonEvent;
            }

            return null;
        }

        /// <summary>
        /// 通常ボタンイベントから UI アクション種別を取得
        /// </summary>
        /// <param name="buttonEvent">ボタンイベント</param>
        /// <param name="type">取得した UI アクション種別</param>
        /// <returns>取得成功時 true</returns>
        public bool TryGetNormalType(
            in NormalButtonEvent buttonEvent,
            out UIActionType type)
        {
            // ボタンイベントに対応する UI アクション種別を取得
            return _normalReverseTable.TryGetValue(buttonEvent, out type);
        }

        /// <summary>
        /// ダイアログ用ボタンイベントを取得
        /// </summary>
        /// <param name="type">UIアクション種別</param>
        /// <param name="dialogType">ダイアログ種別</param>
        /// <returns>対応するボタンイベント</returns>
        public NormalButtonEvent GetDialogButton(
            in UIActionType type,
            in DialogType dialogType)
        {
            // UI アクション種別とダイアログ種別の組み合わせからボタンイベントを取得
            if (_dialogForwardTable.TryGetValue(
                (type, dialogType),
                out NormalButtonEvent buttonEvent))
            {
                return buttonEvent;
            }

            return null;
        }

        /// <summary>
        /// ダイアログ用ボタンイベントから種別情報を取得
        /// </summary>
        /// <param name="buttonEvent">ボタンイベント</param>
        /// <param name="type">取得した UI アクション種別</param>
        /// <param name="dialogType">取得したダイアログ種別</param>
        /// <returns>取得成功時 true</returns>
        public bool TryGetDialogType(
            in NormalButtonEvent buttonEvent,
            out UIActionType type,
            out DialogType dialogType)
        {
            // ボタンイベントに対応する種別情報を取得
            if (_dialogReverseTable.TryGetValue(buttonEvent, out var key))
            {
                type = key.Item1;
                dialogType = key.Item2;

                return true;
            }

            type = default;
            dialogType = default;

            return false;
        }
    }
}