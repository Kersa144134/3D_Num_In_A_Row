// ======================================================
// OptionButtonBinderFactory.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : OptionButtonBinder を生成するクラス
// ======================================================

using UnityEngine.UI;
using OptionSystem.Domain;
using OptionSystem.Infrastructure;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// OptionButtonBinder を生成するクラス
    /// </summary>
    public sealed class OptionButtonBinderFactory
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// GridLayoutGroup 配下の UI 要素収集クラス
        /// </summary>
        private readonly GridLayoutGroupButtonCollector _gridLayoutGroupButtonCollector = new GridLayoutGroupButtonCollector();

        /// <summary>
        /// 選択インデックス取得専用インターフェース
        /// </summary>
        private readonly IOptionSelectionIndexReader _optionSelectionIndexReader;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="optionSelectionIndexTable">オプションインデックス管理テーブル</param>
        public OptionButtonBinderFactory(in IOptionSelectionIndexReader optionSelectionIndexReader)
        {
            _optionSelectionIndexReader = optionSelectionIndexReader;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// OptionButtonBinder を生成する
        /// </summary>
        /// <param name="type">オプション種別</param>
        /// <param name="group">対象 GridLayoutGroup</param>
        /// <returns>生成した OptionButtonBinder</returns>
        public OptionButtonBinder Create(
            in OptionType type,
            in GridLayoutGroup group)
        {
            // GridLayoutGroup 配下の Button を取得
            Button[] buttons = _gridLayoutGroupButtonCollector.GetButtons(group);

            // GridLayoutGroup 配下の OptionButtonEvent を取得
            OptionButtonEvent[] events = _gridLayoutGroupButtonCollector.GetOptionButtons(group);

            // ボタン選択制御クラスを生成
            ButtonSelectionController controller = new ButtonSelectionController(buttons);

            // オプション種別に対応した初期選択インデックスを取得
            int initialIndex = _optionSelectionIndexReader.Get(type);

            // OptionButtonBinder を生成
            OptionButtonBinder binder = new OptionButtonBinder(
                type,
                buttons,
                events,
                controller,
                initialIndex);

            return binder;
        }
    }
}