// ======================================================
// ButtonStateUpdateService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2026-03-17
// 概要     : ボタン入力状態を更新するサービス
// ======================================================

using InputSystem.Data;

namespace InputSystem.Service
{
    /// <summary>
    /// ボタン入力状態を更新するサービス
    /// 各入力種別に応じてコントローラーから値を取得し、ButtonStateへ反映する
    /// </summary>
    public class ButtonStateUpdateService
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定ボタンの状態を更新
        /// </summary>
        /// <param name="controller">入力取得元コントローラー</param>
        /// <param name="type">対象ボタン種別</param>
        /// <param name="state">更新対象のボタン状態</param>
        public void UpdateButtonState(
            in IGamepadInputSource controller,
            in GamepadInputType type,
            in ButtonState state)
        {
            // 指定ボタンの現在入力値を取得
            bool current =
                GetButtonValue(
                    controller,
                    type);

            // ButtonState に現在状態を反映
            state.Update(current);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定ボタンの現在入力値を取得
        /// </summary>
        /// <param name="controller">入力取得元コントローラー</param>
        /// <param name="type">取得対象ボタン種別</param>
        /// <returns>押下中であれば true</returns>
        private bool GetButtonValue(
            in IGamepadInputSource controller,
            in GamepadInputType type)
        {
            // 入力種別ごとに対応するコントローラーのプロパティを返す
            switch (type)
            {
                case GamepadInputType.ButtonA:
                    return controller.ButtonA;
                case GamepadInputType.ButtonB:
                    return controller.ButtonB;
                case GamepadInputType.ButtonX:
                    return controller.ButtonX;
                case GamepadInputType.ButtonY:
                    return controller.ButtonY;
                case GamepadInputType.LeftShoulder:
                    return controller.LeftShoulder;
                case GamepadInputType.RightShoulder:
                    return controller.RightShoulder;
                case GamepadInputType.LeftTrigger:
                    return controller.LeftTrigger;
                case GamepadInputType.RightTrigger:
                    return controller.RightTrigger;
                case GamepadInputType.LeftStickButton:
                    return controller.LeftStickButton;
                case GamepadInputType.RightStickButton:
                    return controller.RightStickButton;
                case GamepadInputType.Start:
                    return controller.StartButton;
                case GamepadInputType.Select:
                    return controller.SelectButton;
                default:
                    return false;
            }
        }
    }
}