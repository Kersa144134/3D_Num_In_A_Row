// ======================================================
// StickStateManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
// 概要     : スティックおよびD-Pad入力状態を管理するクラス
// ======================================================

using UnityEngine;
using InputSystem.Data;

namespace InputSystem.Manager
{
    /// <summary>
    /// スティック・D-Pad入力状態の管理クラス
    /// 物理ゲームパッドまたは仮想ゲームパッドからの入力を統合管理
    /// </summary>
    public class StickStateManager
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
        // 公開メソッド
        // ======================================================

        /// <summary>
        /// 指定コントローラのスティックおよびD-Pad状態を更新
        /// </summary>
        /// <param name="controller">物理または仮想ゲームパッド入力ソース</param>
        public void UpdateStickStates(in IGamepadInputSource controller)
        {
            // 左スティックの状態を更新
            LeftStick = controller.LeftStick;

            // 右スティックの状態を更新
            RightStick = controller.RightStick;

            // D-Padの状態を更新
            DPad = controller.DPad;
        }
    }
}