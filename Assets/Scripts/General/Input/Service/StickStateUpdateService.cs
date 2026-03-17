// ======================================================
// StickStateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
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
        // プロパティ
        // ======================================================

        /// <summary>左スティックの入力ベクトル</summary>
        public Vector2 LeftStick { get; private set; }

        /// <summary>右スティックの入力ベクトル</summary>
        public Vector2 RightStick { get; private set; }

        /// <summary>D-Pad の入力ベクトル</summary>
        public Vector2 DPad { get; private set; }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定コントローラーのスティックおよび D-Pad 状態を更新
        /// </summary>
        /// <param name="controller">入力取得元コントローラー</param>
        /// <param name="type">対象ボタン種別</param>
        public void UpdateStickState(
            in IGamepadInputSource controller)
        {
            LeftStick = controller.LeftStick;
            RightStick = controller.RightStick;
            DPad = controller.DPad;
        }
    }
}