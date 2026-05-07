// ======================================================
// GridLayoutGroupButtonCollector.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-07
// 概要     : GridLayoutGroup 配下の Button を収集するクラス
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// GridLayoutGroup 配下の Button / OptionButton を収集するクラス
    /// </summary>
    public sealed class GridLayoutGroupButtonCollector
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Button を取得する
        /// </summary>
        public Button[] GetButtons(in GridLayoutGroup gridLayoutGroup)
        {
            if (gridLayoutGroup == null)
            {
                return Array.Empty<Button>();
            }

            int childCount = gridLayoutGroup.transform.childCount;

            Button[] buttons = new Button[childCount];

            for (int index = 0; index < childCount; index++)
            {
                Transform child = gridLayoutGroup.transform.GetChild(index);

                buttons[index] = child.GetComponent<Button>();
            }

            return buttons;
        }

        /// <summary>
        /// OptionButton を取得する
        /// </summary>
        public OptionButton[] GetOptionButtons(in GridLayoutGroup gridLayoutGroup)
        {
            if (gridLayoutGroup == null)
            {
                return Array.Empty<OptionButton>();
            }

            int childCount = gridLayoutGroup.transform.childCount;

            OptionButton[] buttons = new OptionButton[childCount];

            for (int index = 0; index < childCount; index++)
            {
                Transform child = gridLayoutGroup.transform.GetChild(index);

                buttons[index] = child.GetComponent<OptionButton>();
            }

            return buttons;
        }
    }
}