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
        /// <param name="initialIndex">初期選択インデックス</param>
        public ButtonSelectionController(in Button[] buttonArray, in int initialIndex)
        {
            _buttonArray = buttonArray;

            _selectStateArray =
                new bool[_buttonArray.Length];

            // 指定インデックスで初期選択
            SetInitialSelect(initialIndex);
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
        /// 初期選択インデックスを指定して初期化する
        /// </summary>
        /// <param name="index">初期選択インデックス</param>
        public void SetInitialSelect(in int index)
        {
            if (index < 0 || index >= _selectStateArray.Length)
            {
                return;
            }

            // すべて非選択にリセット
            for (int i = 0; i < _selectStateArray.Length; i++)
            {
                _selectStateArray[i] = false;
            }

            // 指定インデックスのみ選択状態にする
            _selectStateArray[index] = true;
        }
    }
}