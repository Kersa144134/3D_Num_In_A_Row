// ======================================================
// StickStateUpdateService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2026-04-24
// 概要     : スティックおよび D-Pad 入力状態を取得するサービス
// ======================================================

using UnityEngine;
using InputSystem.Data;

namespace InputSystem.Service
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
        /// スティック・D-Pad 入力を取得する
        /// </summary>
        /// <param name="controller">入力取得元コントローラー</param>
        /// <returns>スティックとD-Padの入力値</returns>
        public (Vector2 left, Vector2 right, Vector2 dPad) GetStickStates(in IGamepadInputSource controller)
        {
            Vector2 left = controller.LeftStick;
            Vector2 right = controller.RightStick;
            Vector2 dPad = controller.DPad;

            return (left, right, dPad);
        }
    }
}