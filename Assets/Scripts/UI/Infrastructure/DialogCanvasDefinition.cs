// ======================================================
// DialogCanvasDefinition.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-19
// 更新日時 : 2026-05-19
// 概要     : ダイアログ Canvas と種類の紐づけ定義
// ======================================================

using System;
using UnityEngine;
using UISystem.Domain;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// ダイアログ Canvas 定義
    /// </summary>
    [Serializable]
    public class DialogCanvasDefinition
    {
        /// <summary>ダイアログ種別</summary>
        public DialogType Type;

        /// <summary>対象 Canvas</summary>
        public GameObject Canvas;

        /// <summary>対象ボタンイベント</summary>
        public NormalButton[] Buttons;

        /// <summary>対象パネルイベント</summary>
        public BasePanelEvent[] Panels;
    }
}