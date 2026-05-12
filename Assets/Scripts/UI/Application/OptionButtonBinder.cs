// ======================================================
// OptionButtonBinder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-10
// 更新日時 : 2026-05-10
// 概要     : オプションボタンと選択制御クラスを結びつけるバインダークラス
// ======================================================

using UnityEngine.UI;
using OptionSystem.Domain;
using UISystem.Infrastructure;
using UISystem.Domain;

namespace UISystem.Application
{
    /// <summary>
    /// オプションボタンと選択制御クラスを結びつけるバインダークラス
    /// </summary>
    public sealed class OptionButtonBinder
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// 選択制御クラス
        /// </summary>
        private readonly ButtonSelectionController _controller;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 対象オプション種別
        /// </summary>
        public OptionType Type { get; }

        /// <summary>
        /// ボタン配列
        /// </summary>
        public Button[] Buttons { get; }

        /// <summary>
        /// ボタンイベント配列
        /// </summary>
        public OptionButtonEvent[] Events { get; }

        /// <summary>
        /// 選択状態配列
        /// </summary>
        public bool[] SelectStateArray => _controller.SelectStateArray;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public OptionButtonBinder(
            OptionType type,
            Button[] buttons,
            OptionButtonEvent[] events,
            ButtonSelectionController controller,
            int initialIndex)
        {
            Type = type;
            Buttons = buttons;
            Events = events;
            _controller = controller;

            // 初期選択反映
            _controller.SelectByIndex(initialIndex);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定ボタン選択処理
        /// </summary>
        /// <param name="button">選択対象ボタン</param>
        public void SelectByButton(in Button button)
        {
            _controller.SelectByButton(button);
        }

        /// <summary>
        /// 指定インデックス選択処理
        /// </summary>
        /// <param name="index">選択対象インデックス</param>
        public void SelectByIndex(in int index)
        {
            _controller.SelectByIndex(index);
        }

        /// <summary>
        /// 現在選択中インデックス取得
        /// </summary>
        public int GetCurrentSelectedIndex()
        {
            return _controller.GetCurrentSelectedIndex();
        }

        /// <summary>
        /// OptionType を UIActionType に変換する
        /// </summary>
        public UIActionType ToUIAction(in OptionType type)
        {
            return type switch
            {
                OptionType.PlayerCount => UIActionType.OptionPlayerCount,
                OptionType.LimitTime => UIActionType.OptionLimitTime,
                OptionType.BoardSize => UIActionType.OptionBoardSize,
                OptionType.ConnectCount => UIActionType.OptionConnectCount,
                OptionType.CameraRotationSpeed => UIActionType.OptionCameraRotationSpeed,
                OptionType.PointerSpeed => UIActionType.OptionPointerSpeed,
                _ => UIActionType.None
            };
        }
    }
}