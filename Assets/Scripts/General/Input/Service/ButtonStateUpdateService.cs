// ======================================================
// ButtonStateUpdateService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-12-16
// 概要     : ボタン入力状態を更新するサービス
// ======================================================

using InputSystem.Data;

namespace InputSystem.Manager
{
    /// <summary>
    /// ボタン入力状態を更新するサービス
    /// </summary>
    public class ButtonStateUpdateService
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        public void UpdateButtonState(
            in IGamepadInputSource controller,
            in GamepadInputType type,
            in ButtonState state)
        {
            bool current = GetButtonValue(controller, type);

            state.Update(current);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        private bool GetButtonValue(
            in IGamepadInputSource controller,
            in GamepadInputType type)
        {
            switch (type)
            {
                case GamepadInputType.ButtonA: return controller.ButtonA;
                case GamepadInputType.ButtonB: return controller.ButtonB;
                case GamepadInputType.ButtonX: return controller.ButtonX;
                case GamepadInputType.ButtonY: return controller.ButtonY;
                case GamepadInputType.LeftShoulder: return controller.LeftShoulder;
                case GamepadInputType.RightShoulder: return controller.RightShoulder;
                case GamepadInputType.LeftTrigger: return controller.LeftTrigger;
                case GamepadInputType.RightTrigger: return controller.RightTrigger;
                case GamepadInputType.LeftStickButton: return controller.LeftStickButton;
                case GamepadInputType.RightStickButton: return controller.RightStickButton;
                case GamepadInputType.Start: return controller.StartButton;
                case GamepadInputType.Select: return controller.SelectButton;
                default: return false;
            }
        }
    }
}