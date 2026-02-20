// ======================================================
// GamepadInputController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-09-24
// 更新日時 : 2025-11-11
// 概要     : ゲームパッドのスティック、ABXYボタン、ショルダー、トリガー入力を取得し、
//            デッドゾーンを考慮して値を保持するクラス
// ======================================================

using UnityEngine;
using UnityEngine.InputSystem;
using InputSystem.Data;

namespace InputSystem.Controller
{
    /// <summary>
    /// ゲームパッド入力を取得するクラス
    /// デッドゾーン処理を行った入力値を保持する
    /// </summary>
    public class GamepadInputController : IGamepadInputSource
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>左トリガーのデッドゾーン値</summary>
        private const float LEFT_TRIGGER_DEAD_ZONE = 0.1f;

        /// <summary>右トリガーのデッドゾーン値</summary>
        private const float RIGHT_TRIGGER_DEAD_ZONE = 0.1f;

        /// <summary>左スティックのデッドゾーン値</summary>
        private const float LEFT_STICK_DEAD_ZONE = 0.1f;

        /// <summary>右スティックのデッドゾーン値</summary>
        private const float RIGHT_STICK_DEAD_ZONE = 0.1f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ボタンAが押されているか</summary>
        public bool ButtonA { get; private set; }

        /// <summary>ボタンBが押されているか</summary>
        public bool ButtonB { get; private set; }

        /// <summary>ボタンXが押されているか</summary>
        public bool ButtonX { get; private set; }

        /// <summary>ボタンYが押されているか</summary>
        public bool ButtonY { get; private set; }

        /// <summary>左ショルダーボタンが押されているか</summary>
        public bool LeftShoulder { get; private set; }

        /// <summary>右ショルダーボタンが押されているか</summary>
        public bool RightShoulder { get; private set; }

        /// <summary>左トリガーがデッドゾーン以上押されているか</summary>
        public bool LeftTrigger { get; private set; }

        /// <summary>右トリガーがデッドゾーン以上押されているか</summary>
        public bool RightTrigger { get; private set; }

        /// <summary>左スティックボタンが押されているか</summary>
        public bool LeftStickButton { get; private set; }

        /// <summary>右スティックボタンが押されているか</summary>
        public bool RightStickButton { get; private set; }

        /// <summary>左スティックの入力ベクトル</summary>
        public Vector2 LeftStick { get; private set; } = Vector2.zero;

        /// <summary>右スティックの入力ベクトル</summary>
        public Vector2 RightStick { get; private set; } = Vector2.zero;

        /// <summary>十字キーの入力ベクトル</summary>
        public Vector2 DPad { get; private set; } = Vector2.zero;

        /// <summary>スタートボタンが押されているか</summary>
        public bool StartButton { get; private set; }

        /// <summary>セレクトボタンが押されているか</summary>
        public bool SelectButton { get; private set; }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ゲームパッド入力を更新し、各プロパティに反映する
        /// </summary>
        public void UpdateInputs()
        {
            // 現在接続されているゲームパッドを取得
            Gamepad pad = Gamepad.current;

            // ゲームパッド未接続なら処理なし
            if (pad == null)
            {
                return;
            }

            // --------------------------------------------------
            // ABXYボタン入力取得
            // --------------------------------------------------
            ButtonA = pad.buttonSouth.isPressed;
            ButtonB = pad.buttonEast.isPressed;
            ButtonX = pad.buttonWest.isPressed;
            ButtonY = pad.buttonNorth.isPressed;

            // --------------------------------------------------
            // ショルダー / トリガー / スティックボタン入力取得
            // --------------------------------------------------
            LeftShoulder = pad.leftShoulder.isPressed;
            RightShoulder = pad.rightShoulder.isPressed;
            LeftTrigger = pad.leftTrigger.ReadValue() >= LEFT_TRIGGER_DEAD_ZONE;
            RightTrigger = pad.rightTrigger.ReadValue() >= RIGHT_TRIGGER_DEAD_ZONE;
            LeftStickButton = pad.leftStickButton.isPressed;
            RightStickButton = pad.rightStickButton.isPressed;

            // --------------------------------------------------
            // スティック入力取得＆デッドゾーン適用
            // --------------------------------------------------
            LeftStick = ApplyDeadZone(pad.leftStick.ReadValue(), LEFT_STICK_DEAD_ZONE);
            RightStick = ApplyDeadZone(pad.rightStick.ReadValue(), RIGHT_STICK_DEAD_ZONE);
            DPad = pad.dpad.ReadValue();

            // --------------------------------------------------
            // スタート・セレクトボタン入力取得
            // --------------------------------------------------
            StartButton = pad.startButton.isPressed;
            SelectButton = pad.selectButton.isPressed;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 入力ベクトルにデッドゾーン処理を適用する
        /// </summary>
        /// <param name="input">入力ベクトル</param>
        /// <param name="deadZone">デッドゾーン値</param>
        /// <returns>デッドゾーン未満なら Vector2.zero、そうでなければ元の入力ベクトル</returns>
        private Vector2 ApplyDeadZone(in Vector2 input, in float deadZone)
        {
            // ベクトルの長さがデッドゾーン未満ならゼロにする
            return input.magnitude < deadZone ? Vector2.zero : input;
        }
    }
}