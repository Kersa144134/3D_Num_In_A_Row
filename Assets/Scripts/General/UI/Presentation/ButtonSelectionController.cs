// ======================================================
// ButtonSelectionController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 概要     : Button の単一選択状態を管理する
// ======================================================

using UnityEngine.UI;

namespace UISystem.Presentation
{
    /// <summary>
    /// Button 選択状態制御クラス
    /// </summary>
    public sealed class ButtonSelectionController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Button 配列</summary>
        private readonly Button[] _buttonArray;

        /// <summary>選択状態配列</summary>
        private readonly bool[] _selectStateArray;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>Button 配列</summary>
        public Button[] ButtonArray => _buttonArray;

        /// <summary>選択状態配列</summary>
        public bool[] SelectStateArray => _selectStateArray;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="buttonArray">Button 配列</param>
        public ButtonSelectionController(in Button[] buttonArray)
        {
            _buttonArray = buttonArray;

            _selectStateArray =
                new bool[_buttonArray.Length];

            Initialize();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// Button を選択状態へ変更する
        /// </summary>
        /// <param name="selectedButton">選択 Button</param>
        public void Select(in Button selectedButton)
        {
            // 配列数分ループ
            for (int index = 0; index < _buttonArray.Length; index++)
            {
                // 選択状態更新
                _selectStateArray[index] = _buttonArray[index] == selectedButton;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            if (_selectStateArray.Length == 0)
            {
                return;
            }

            // index0 を初期選択
            _selectStateArray[0] = true;
        }
    }
}