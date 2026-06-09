// ======================================================
// DialogButton.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-06-09
// 概要     : UIActionType と DialogType と Button を紐づけるデータクラス
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using UISystem.Domain;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// UIActionType と DialogType と Button を紐づけるデータ
    /// </summary>
    [Serializable]
    public sealed class DialogButton
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
        // フィールド
        // ======================================================

        /// <summary>
        /// 対応するダイアログ種別
        /// </summary>
        private DialogType _dialogType = DialogType.None;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// UIアクション種別取得
        /// </summary>
        public UIActionType Type => _type;

        /// <summary>
        /// ダイアログ種別取得
        /// </summary>
        public DialogType DialogType => _dialogType;

        /// <summary>
        /// ボタン取得
        /// </summary>
        public Button Button => _button;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ダイアログ情報を後から設定する
        /// </summary>
        public void Initialize(in DialogType dialogType)
        {
            _dialogType = dialogType;
        }
    }
}