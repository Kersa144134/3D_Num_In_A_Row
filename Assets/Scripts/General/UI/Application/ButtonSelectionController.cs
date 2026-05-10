// ======================================================
// ButtonSelectionController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 概要     : Button の単一選択状態を管理する
// ======================================================

using UnityEngine.UI;

namespace UISystem.Application
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

            // 配列数に応じて選択状態配列を生成
            _selectStateArray = new bool[_buttonArray.Length];
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定 Button を選択状態へ変更する
        /// </summary>
        /// <param name="button">選択対象ボタン</param>
        public void SelectByButton(in Button button)
        {
            for (int index = 0; index < _buttonArray.Length; index++)
            {
                // 選択状態更新
                _selectStateArray[index] = _buttonArray[index] == button;
            }
        }

        /// <summary>
        /// 指定インデックスを選択状態へ変更する
        /// </summary>
        /// <param name="index">選択対象インデックス</param>
        public void SelectByIndex(in int index)
        {
            if (index < 0 ||
                index >= _selectStateArray.Length)
            {
                return;
            }

            for (int i = 0; i < _selectStateArray.Length; i++)
            {
                // 選択状態更新
                _selectStateArray[i] = i == index;
            }
        }

        /// <summary>
        /// 現在選択されているインデックスを取得する
        /// </summary>
        /// <returns>選択インデックス 未選択の場合は -1</returns>
        public int GetCurrentSelectedIndex()
        {
            for (int index = 0; index < _selectStateArray.Length; index++)
            {
                if (_selectStateArray[index] == false)
                {
                    continue;
                }

                return index;
            }

            // 未選択状態
            return -1;
        }
    }
}