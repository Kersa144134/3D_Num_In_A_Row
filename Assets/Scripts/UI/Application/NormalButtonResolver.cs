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
        // 通常ボタン解決
        // ======================================================

        public NormalButtonEvent GetNormalButton(in UIActionType type)
        {
            if (_normalForwardTable.TryGetValue(type, out NormalButtonEvent buttonEvent))
            {
                return buttonEvent;
            }

            return null;
        }

        public bool TryGetNormalType(in NormalButtonEvent buttonEvent, out UIActionType type)
        {
            return _normalReverseTable.TryGetValue(buttonEvent, out type);
        }

        // ======================================================
        // ダイアログボタン解決
        // ======================================================

        public NormalButtonEvent GetDialogButton(in UIActionType type, in DialogType dialogType)
        {
            if (_dialogForwardTable.TryGetValue((type, dialogType), out NormalButtonEvent buttonEvent))
            {
                return buttonEvent;
            }

            return null;
        }

        public bool TryGetDialogType(in NormalButtonEvent buttonEvent, out UIActionType type, out DialogType dialogType)
        {
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