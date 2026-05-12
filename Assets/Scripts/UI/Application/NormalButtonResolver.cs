// ======================================================
// NormalButtonResolver.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : UIActionType と NormalButtonEvent の参照解決クラス
// ======================================================

using System.Collections.Generic;
using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// 通常ボタンの参照解決を行うサービスクラス
    /// </summary>
    public sealed class NormalButtonResolver
    {
        // ======================================================
        // 辞書
        // ======================================================

        /// <summary>
        /// 正引き辞書
        /// UIActionType → NormalButtonEvent
        /// </summary>
        private readonly Dictionary<UIActionType, NormalButtonEvent> _forwardTable;

        /// <summary>
        /// 逆引き辞書
        /// NormalButtonEvent → UIActionType
        /// </summary>
        private readonly Dictionary<NormalButtonEvent, UIActionType> _reverseTable;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public NormalButtonResolver(in Dictionary<UIActionType, NormalButtonEvent> table)
        {
            _forwardTable = table;

            // 逆引き辞書構築
            _reverseTable = new Dictionary<NormalButtonEvent, UIActionType>(table.Count);

            foreach (KeyValuePair<UIActionType, NormalButtonEvent> pair in table)
            {
                _reverseTable[pair.Value] = pair.Key;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// UIActionType からボタンイベント取得
        /// </summary>
        public NormalButtonEvent Get(in UIActionType type)
        {
            if (_forwardTable.TryGetValue(type, out NormalButtonEvent buttonEvent))
            {
                return buttonEvent;
            }

            return null;
        }

        /// <summary>
        /// ボタンイベントから UIActionType を取得
        /// </summary>
        public bool TryGetType(in NormalButtonEvent buttonEvent, out UIActionType type)
        {
            return _reverseTable.TryGetValue(buttonEvent, out type);
        }
    }
}