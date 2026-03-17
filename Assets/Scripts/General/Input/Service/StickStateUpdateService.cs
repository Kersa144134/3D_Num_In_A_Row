// ======================================================
// StickStateUpdateService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2026-03-17
// 概要     : スティックおよびD-Pad入力状態を更新するサービス
// ======================================================

using UnityEngine;
using InputSystem.Data;

namespace InputSystem.Service
{
    /// <summary>
    /// スティック・D-Pad入力状態を更新するサービス
    /// </summary>
    public class StickStateUpdateService
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定コントローラーのスティック・D-Pad 状態を更新
        /// </summary>
        /// <param name="controller">入力取得元コントローラー</param>
        /// <param name="leftStick">左スティックの入力を反映する変数</param>
        /// <param name="rightStick">右スティックの入力を反映する変数</param>
        /// <param name="dPad">D-Pad の入力を反映する変数</param>
        public void UpdateStickStates(
            in IGamepadInputSource controller,
            ref Vector2 leftStick,
            ref Vector2 rightStick,
            ref Vector2 dPad)
        {
            leftStick = controller.LeftStick;
            rightStick = controller.RightStick;
            dPad = controller.DPad;
        }
    }
}