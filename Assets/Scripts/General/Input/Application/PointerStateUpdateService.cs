// ======================================================
// PointerStateUpdateService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-26
// 更新日時 : 2026-04-26
// 概要     : 入力デバイスに応じてポインター座標を更新するサービス
// ======================================================

using UnityEngine;
using InputSystem.Domain;

namespace InputSystem.Application
{
    /// <summary>
    /// ポインター状態更新サービス
    /// </summary>
    public class PointerStateUpdateService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ポインター移動速度</summary>
        private readonly float _pointerSpeed;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="pointerSpeed">ポインター移動速度</param>
        public PointerStateUpdateService(in float pointerSpeed)
        {
            _pointerSpeed = pointerSpeed;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ポインター座標を更新する
        /// </summary>
        /// <param name="controller">入力コントローラー</param>
        /// <param name="pointer">現在のポインター座標</param>
        public void UpdatePointer(
            in IGamepadInputSource controller,
            ref Vector2 pointer)
        {
            if (controller == null)
            {
                return;
            }

            // --------------------------------------------------
            // マウス
            // --------------------------------------------------
            if (controller.IsPointerAbsolute)
            {
                pointer = controller.PointerPosition;
            }
            // --------------------------------------------------
            // ゲームパッド
            // --------------------------------------------------
            else
            {
                Vector2 delta =
                    controller.PointerDelta *
                    _pointerSpeed *
                    Time.deltaTime;

                pointer += delta;
            }

            // --------------------------------------------------
            // 画面内制限
            // --------------------------------------------------
            pointer.x = Mathf.Clamp(pointer.x, 0f, Screen.width);
            pointer.y = Mathf.Clamp(pointer.y, 0f, Screen.height);
        }
    }
}