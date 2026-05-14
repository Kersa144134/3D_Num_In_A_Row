// ======================================================
// StickStateUpdateService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2026-04-26
// 概要     : スティックおよび D-Pad 入力状態を取得するサービス
// ======================================================

using UnityEngine;
using InputSystem.Domain;

namespace InputSystem.Application
{
    /// <summary>
    /// スティック・D-Pad入力状態を取得するサービス
    /// </summary>
    public class StickStateUpdateService
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// スティック・D-Pad入力を一括取得する
        /// </summary>
        /// <param name="controller">入力取得元</param>
        /// <param name="left">左スティック出力</param>
        /// <param name="right">右スティック出力</param>
        /// <param name="dPad">D-Pad 出力</param>
        public void GetStickStates(
            in IGamepadInputSource controller,
            in StickState left,
            in StickState right,
            in StickState dPad)
        {
            left.Update(controller.LeftStick);
            right.Update(controller.RightStick);
            dPad.Update(controller.DPad);
        }
    }
}