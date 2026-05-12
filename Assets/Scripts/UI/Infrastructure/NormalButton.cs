// ======================================================
// NormalButton.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : UIActionType と Button を紐づけるデータクラス
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UISystem.Domain;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// UIActionType と Button を紐づけるデータ
    /// </summary>
    [Serializable]
    public sealed class NormalButton
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>
        /// 対応する UI アクション種別
        /// </summary>
        [SerializeField]
        private UIActionType _type;

        /// <summary>
        /// 対象ボタン
        /// </summary>
        [SerializeField]
        private Button _button;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// UIアクション種別取得
        /// </summary>
        public UIActionType Type => _type;

        /// <summary>
        /// ボタン取得
        /// </summary>
        public Button Button => _button;
    }
}