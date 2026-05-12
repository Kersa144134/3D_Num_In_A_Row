// ======================================================
// ButtonDictionaryBuilder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : UI のボタンイベント辞書構築を行うクラス
// ======================================================

using System.Collections.Generic;
using UnityEngine.UI;
using OptionSystem.Domain;
using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// ボタンイベントの辞書を構築するクラス
    /// </summary>
    public sealed class ButtonDictionaryBuilder
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// NormalButton 辞書を構築する
        /// </summary>
        /// <param name="normalButtons">対象の NormalButton リスト</param>
        /// <returns>生成した NormalButtonEvent 辞書</returns>
        public Dictionary<UIActionType, NormalButtonEvent> BuildNormalButtons(in NormalButton[] normalButtons)
        {
            // --------------------------------------------------
            // 辞書生成
            // --------------------------------------------------
            Dictionary<UIActionType, NormalButtonEvent> table = new Dictionary<UIActionType, NormalButtonEvent>();

            // --------------------------------------------------
            // NormalButtonEvent 取得
            // --------------------------------------------------
            for (int index = 0; index < normalButtons.Length; index++)
            {
                NormalButton normalButton = normalButtons[index];

                if (normalButton == null || normalButton.Button == null)
                {
                    continue;
                }

                // ButtonEvent 取得
                NormalButtonEvent buttonEvent = normalButton.Button.GetComponent<NormalButtonEvent>();

                // 辞書登録
                table[normalButton.Type] = buttonEvent;
            }

            return table;
        }

        /// <summary>
        /// OptionButtonBinder 辞書を構築する
        /// </summary>
        /// <param name="factory">Binder生成ファクトリ</param>
        /// <param name="groups">オプションボタングループ一覧</param>
        /// <returns>生成した OptionButtonBinder 辞書</returns>
        public Dictionary<OptionType, OptionButtonBinder> BuildOptionButtons(
            in OptionButtonBinderFactory factory,
            in OptionButtonGroup[] groups)
        {
            // --------------------------------------------------
            // 辞書生成
            // --------------------------------------------------
            Dictionary<OptionType, OptionButtonBinder> binders
                = new Dictionary<OptionType, OptionButtonBinder>();

            // --------------------------------------------------
            // OptionButtonBinder 生成
            // --------------------------------------------------
            for (int index = 0; index < groups.Length; index++)
            {
                OptionButtonGroup group = groups[index];

                if (group == null)
                {
                    continue;
                }

                // GridLayoutGroup 取得
                GridLayoutGroup gridLayoutGroup = group.Group;

                if (gridLayoutGroup == null)
                {
                    continue;
                }

                // OptionButtonBinder を辞書登録
                binders[group.Type] = factory.Create(group.Type, gridLayoutGroup);
            }

            return binders;
        }
    }
}