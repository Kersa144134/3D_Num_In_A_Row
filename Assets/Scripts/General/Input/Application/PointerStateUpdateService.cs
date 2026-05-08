// ======================================================
// PointerStateUpdateService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-26
// 更新日時 : 2026-05-08
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
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ポインター座標を更新する
        /// </summary>
        /// <param name="controller">入力コントローラー</param>
        /// <param name="pointer">現在のポインター座標</param>
        /// <param name="pointerSpeed">ポインター移動速度</param>
        public void UpdatePointer(
            in IGamepadInputSource controller,
            ref Vector2 pointer,
            in float pointerSpeed)
        {
            if (controller == null)
            {
                return;
            }

            // --------------------------------------------------
            // マウス
            // --------------------------------------------------
            // 絶対座標入力の場合はそのまま反映
            if (controller.IsPointerAbsolute)
            {
                pointer = controller.PointerPosition;
            }
            // --------------------------------------------------
            // ゲームパッド
            // --------------------------------------------------
            else
            {
                // フレーム時間を考慮した移動量を算出
                Vector2 delta =
                    controller.PointerDelta *
                    pointerSpeed *
                    Time.deltaTime;

                // 現在座標へ加算
                pointer += delta;
            }

            // 画面内制限
            ClampPointerPosition(ref pointer);
        }

        /// <summary>
        /// ポインター座標を指定位置へ設定する
        /// </summary>
        /// <param name="pointer">現在のポインター座標</param>
        /// <param name="position">設定する座標</param>
        public void SetPointerPosition(
            ref Vector2 pointer,
            in Vector2 position)
        {
            // 指定座標をそのまま反映
            pointer = position;

            // 画面内制限
            ClampPointerPosition(ref pointer);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// ポインター座標を画面内へ制限する
        /// </summary>
        /// <param name="pointer">制限対象のポインター座標</param>
        private void ClampPointerPosition(ref Vector2 pointer)
        {
            // X 座標を画面内へ制限
            pointer.x = Mathf.Clamp(pointer.x, 0f, Screen.width);

            // Y 座標を画面内へ制限
            pointer.y = Mathf.Clamp(pointer.y, 0f, Screen.height);
        }
    }
}