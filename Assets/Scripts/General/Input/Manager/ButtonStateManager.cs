// ======================================================
// ButtonStateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-12-16
// 概要     : ボタン入力状態を管理するクラス
//            InputManager.ButtonState を内部で保持し、押下状態を更新する
// ======================================================

using System;
using InputSystem.Data;

namespace InputSystem.Manager
{
    /// <summary>
    /// ボタン状態を管理するクラス
    /// GamepadInputType 列挙順に配列で保持し、毎フレーム更新
    /// </summary>
    public class ButtonStateManager
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ボタン状態配列（GamepadInputType の順序で固定）</summary>
        private readonly ButtonState[] _buttonStates;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ButtonStateManager のコンストラクタ
        /// </summary>
        public ButtonStateManager()
        {
            // enum の数だけ配列を確保
            int enumLength = Enum.GetValues(typeof(GamepadInputType)).Length;

            _buttonStates = new ButtonState[enumLength];

            // 各ボタン状態を初期化
            for (int i = 0; i < _buttonStates.Length; i++)
            {
                _buttonStates[i] = new ButtonState();
            }
        }

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ボタンAの状態</summary>
        public ButtonState ButtonA => _buttonStates[(int)GamepadInputType.ButtonA];

        /// <summary>ボタンBの状態</summary>
        public ButtonState ButtonB => _buttonStates[(int)GamepadInputType.ButtonB];

        /// <summary>ボタンXの状態</summary>
        public ButtonState ButtonX => _buttonStates[(int)GamepadInputType.ButtonX];

        /// <summary>ボタンYの状態</summary>
        public ButtonState ButtonY => _buttonStates[(int)GamepadInputType.ButtonY];

        /// <summary>左ショルダーの状態</summary>
        public ButtonState LeftShoulder => _buttonStates[(int)GamepadInputType.LeftShoulder];

        /// <summary>右ショルダーの状態</summary>
        public ButtonState RightShoulder => _buttonStates[(int)GamepadInputType.RightShoulder];

        /// <summary>左トリガーの状態</summary>
        public ButtonState LeftTrigger => _buttonStates[(int)GamepadInputType.LeftTrigger];

        /// <summary>右トリガーの状態</summary>
        public ButtonState RightTrigger => _buttonStates[(int)GamepadInputType.RightTrigger];

        /// <summary>左スティックボタンの状態</summary>
        public ButtonState LeftStickButton => _buttonStates[(int)GamepadInputType.LeftStickButton];

        /// <summary>右スティックボタンの状態</summary>
        public ButtonState RightStickButton => _buttonStates[(int)GamepadInputType.RightStickButton];

        /// <summary>Startボタンの状態</summary>
        public ButtonState StartButton => _buttonStates[(int)GamepadInputType.Start];

        /// <summary>Selectボタンの状態</summary>
        public ButtonState SelectButton => _buttonStates[(int)GamepadInputType.Select];

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ゲームパッド入力からボタン状態を更新する
        /// </summary>
        /// <param name="controller">IGamepadInputSource を実装した入力コントローラ</param>
        public void UpdateButtonStates(in IGamepadInputSource controller)
        {
            _buttonStates[(int)GamepadInputType.ButtonA].Update(controller.ButtonA);
            _buttonStates[(int)GamepadInputType.ButtonB].Update(controller.ButtonB);
            _buttonStates[(int)GamepadInputType.ButtonX].Update(controller.ButtonX);
            _buttonStates[(int)GamepadInputType.ButtonY].Update(controller.ButtonY);
            _buttonStates[(int)GamepadInputType.LeftShoulder].Update(controller.LeftShoulder);
            _buttonStates[(int)GamepadInputType.RightShoulder].Update(controller.RightShoulder);
            _buttonStates[(int)GamepadInputType.LeftTrigger].Update(controller.LeftTrigger);
            _buttonStates[(int)GamepadInputType.RightTrigger].Update(controller.RightTrigger);
            _buttonStates[(int)GamepadInputType.LeftStickButton].Update(controller.LeftStickButton);
            _buttonStates[(int)GamepadInputType.RightStickButton].Update(controller.RightStickButton);
            _buttonStates[(int)GamepadInputType.Start].Update(controller.StartButton);
            _buttonStates[(int)GamepadInputType.Select].Update(controller.SelectButton);
        }
    }
}