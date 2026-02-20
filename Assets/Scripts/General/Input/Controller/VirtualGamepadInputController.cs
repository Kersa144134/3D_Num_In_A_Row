// ======================================================
// VirtualGamepadInputController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 概要     : KeyboardInputController と MouseInputController から入力を取得し、
//            ゲームパッド互換の仮想入力を生成するクラス
// ======================================================

using UnityEngine;
using InputSystem.Data;

namespace InputSystem.Controller
{
    /// <summary>
    /// キーボード・マウス入力を統合して仮想ゲームパッド入力を生成するクラス
    /// </summary>
    public class VirtualGamepadInputController : IGamepadInputSource
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>キーボード入力を取得するコントローラクラス</summary>
        private readonly KeyboardInputController _keyboard;

        /// <summary>マウス入力を取得するコントローラクラス</summary>
        private readonly MouseInputController _mouse;

        /// <summary>入力マッピング情報</summary>
        private readonly InputMapping[] _mappings;
        
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>A ボタンの仮想入力状態</summary>
        public bool ButtonA { get; private set; }

        /// <summary>B ボタンの仮想入力状態</summary>
        public bool ButtonB { get; private set; }

        /// <summary>X ボタンの仮想入力状態</summary>
        public bool ButtonX { get; private set; }

        /// <summary>Y ボタンの仮想入力状態</summary>
        public bool ButtonY { get; private set; }

        /// <summary>左ショルダーの仮想入力状態</summary>
        public bool LeftShoulder { get; private set; }

        /// <summary>右ショルダーの仮想入力状態</summary>
        public bool RightShoulder { get; private set; }

        /// <summary>左トリガーの仮想入力状態</summary>
        public bool LeftTrigger { get; private set; }

        /// <summary>右トリガーの仮想入力状態</summary>
        public bool RightTrigger { get; private set; }

        /// <summary>左スティックボタンの仮想入力状態</summary>
        public bool LeftStickButton { get; private set; }

        /// <summary>右スティックボタンの仮想入力状態</summary>
        public bool RightStickButton { get; private set; }

        /// <summary>左スティックの仮想入力値</summary>
        public Vector2 LeftStick { get; private set; }

        /// <summary>右スティックの仮想入力値</summary>
        public Vector2 RightStick { get; private set; }

        /// <summary>D-Pad の仮想入力値</summary>
        public Vector2 DPad { get; private set; }

        /// <summary>Start ボタンの仮想入力状態</summary>
        public bool StartButton { get; private set; }

        /// <summary>Select ボタンの仮想入力状態</summary>
        public bool SelectButton { get; private set; }
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 仮想ゲームパッド入力を初期化
        /// </summary>
        /// <param name="keyboard">キーボード入力コントローラ</param>
        /// <param name="mouse">マウス入力コントローラ</param>
        /// <param name="mappings">InputMappingConfig から取得したマッピング配列</param>
        public VirtualGamepadInputController(
            in KeyboardInputController keyboard,
            in MouseInputController mouse,
            in InputMapping[] mappings)
        {
            _keyboard = keyboard;
            _mouse = mouse;
            _mappings = mappings ?? new InputMapping[0];
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// キーボード・マウス入力から仮想ゲームパッド値を更新する
        /// </summary>
        public void UpdateInputs()
        {
            bool buttonA = false;
            bool buttonB = false;
            bool buttonX = false;
            bool buttonY = false;

            bool leftShoulder = false;
            bool rightShoulder = false;
            bool leftTrigger = false;
            bool rightTrigger = false;
            bool leftStickButton = false;
            bool rightStickButton = false;

            Vector2 leftStick = Vector2.zero;
            Vector2 rightStick = Vector2.zero;
            Vector2 dpad = Vector2.zero;

            bool startButton = false;
            bool selectButton = false;

            // --------------------------------------------------
            // キーボード入力の反映
            // --------------------------------------------------
            ApplyKeyboardMappings(
                ref buttonA, ref buttonB, ref buttonX, ref buttonY,
                ref leftShoulder, ref rightShoulder,
                ref leftTrigger, ref rightTrigger,
                ref leftStickButton, ref rightStickButton,
                ref leftStick, ref rightStick, ref dpad,
                ref startButton, ref selectButton);

            // --------------------------------------------------
            // マウス入力の反映
            // --------------------------------------------------
            ApplyMouseMappings(
                ref buttonA, ref buttonB, ref buttonX, ref buttonY,
                ref leftShoulder, ref rightShoulder,
                ref leftTrigger, ref rightTrigger,
                ref leftStickButton, ref rightStickButton,
                ref leftStick, ref rightStick, ref dpad,
                ref startButton, ref selectButton);

            // --------------------------------------------------
            // ベクトル正規化
            // --------------------------------------------------
            LeftStick = leftStick.magnitude > 1f ? leftStick.normalized : leftStick;
            RightStick = Vector2.ClampMagnitude(rightStick, 1f);
            DPad = dpad.magnitude > 1f ? dpad.normalized : dpad;

            ButtonA = buttonA;
            ButtonB = buttonB;
            ButtonX = buttonX;
            ButtonY = buttonY;

            LeftShoulder = leftShoulder;
            RightShoulder = rightShoulder;
            LeftTrigger = leftTrigger;
            RightTrigger = rightTrigger;
            LeftStickButton = leftStickButton;
            RightStickButton = rightStickButton;

            StartButton = startButton;
            SelectButton = selectButton;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// キーボード入力を仮想ゲームパッドに反映
        /// </summary>
        /// <param name="buttonA">ButtonA の状態参照</param>
        /// <param name="buttonB">ButtonB の状態参照</param>
        /// <param name="buttonX">ButtonX の状態参照</param>
        /// <param name="buttonY">ButtonY の状態参照</param>
        /// <param name="leftShoulder">LeftShoulder の状態参照</param>
        /// <param name="rightShoulder">RightShoulder の状態参照</param>
        /// <param name="leftTrigger">LeftTrigger の状態参照</param>
        /// <param name="rightTrigger">RightTrigger の状態参照</param>
        /// <param name="leftStickButton">左スティックボタンの状態参照</param>
        /// <param name="rightStickButton">右スティックボタンの状態参照</param>
        /// <param name="leftStick">左スティック入力ベクトル参照</param>
        /// <param name="rightStick">右スティック入力ベクトル参照</param>
        /// <param name="dpad">D-Pad 入力ベクトル参照</param>
        /// <param name="startButton">Start ボタンの状態参照</param>
        /// <param name="selectButton">Select ボタンの状態参照</param>
        private void ApplyKeyboardMappings(
            ref bool buttonA, ref bool buttonB, ref bool buttonX, ref bool buttonY,
            ref bool leftShoulder, ref bool rightShoulder,
            ref bool leftTrigger, ref bool rightTrigger,
            ref bool leftStickButton, ref bool rightStickButton,
            ref Vector2 leftStick, ref Vector2 rightStick, ref Vector2 dpad,
            ref bool startButton, ref bool selectButton)
        {
            // 全マッピングを列挙してキーボードバインディングがあるものを反映
            foreach (InputMapping map in _mappings)
            {
                if (!map.IsKeyboardBinding)
                {
                    // キーボードに関連するマッピングでなければスキップ
                    continue;
                }

                // 反映する値を初期化
                float value = 0f;

                // 指定ゲームパッド入力に対応するキーを押下していれば 1f とする
                if (_keyboard.GetButton(map.gamepadInput))
                {
                    value = 1f;
                }

                // 入力がなければ反映せずにスキップ
                if (value == 0f)
                {
                    continue;
                }

                // 対応する仮想ゲームパッドの状態に反映
                ApplyGamepadMapping(
                    ref buttonA, ref buttonB, ref buttonX, ref buttonY,
                    ref leftShoulder, ref rightShoulder,
                    ref leftTrigger, ref rightTrigger,
                    ref leftStickButton, ref rightStickButton,
                    ref leftStick, ref rightStick, ref dpad,
                    ref startButton, ref selectButton,
                    map.gamepadInput, value);
            }
        }

        /// <summary>
        /// マウス入力を仮想ゲームパッドに反映
        /// </summary>
        /// <param name="buttonA">ButtonA の状態参照</param>
        /// <param name="buttonB">ButtonB の状態参照</param>
        /// <param name="buttonX">ButtonX の状態参照</param>
        /// <param name="buttonY">ButtonY の状態参照</param>
        /// <param name="leftShoulder">LeftShoulder の状態参照</param>
        /// <param name="rightShoulder">RightShoulder の状態参照</param>
        /// <param name="leftTrigger">LeftTrigger の状態参照</param>
        /// <param name="rightTrigger">RightTrigger の状態参照</param>
        /// <param name="leftStickButton">左スティックボタンの状態参照</param>
        /// <param name="rightStickButton">右スティックボタンの状態参照</param>
        /// <param name="leftStick">左スティック入力ベクトル参照</param>
        /// <param name="rightStick">右スティック入力ベクトル参照</param>
        /// <param name="dpad">D-Pad 入力ベクトル参照</param>
        /// <param name="startButton">Start ボタンの状態参照</param>
        /// <param name="selectButton">Select ボタンの状態参照</param>
        private void ApplyMouseMappings(
            ref bool buttonA, ref bool buttonB, ref bool buttonX, ref bool buttonY,
            ref bool leftShoulder, ref bool rightShoulder,
            ref bool leftTrigger, ref bool rightTrigger,
            ref bool leftStickButton, ref bool rightStickButton,
            ref Vector2 leftStick, ref Vector2 rightStick, ref Vector2 dpad,
            ref bool startButton, ref bool selectButton)
        {
            // 前フレームとの差分を方向別に取得
            MouseInputController.MouseDelta delta = _mouse.GetMouseDelta();

            // 全マッピングを列挙してマウスバインディングがあるものを反映
            foreach (InputMapping map in _mappings)
            {
                if (!map.IsMouseBinding)
                {
                    // マウス入力に対応していない場合はスキップ
                    continue;
                }

                // 反映する値を初期化
                float value = 0f;

                // 移動・ホイール系の入力の場合、方向別の値を取得
                switch (map.mouseInput)
                {
                    case MouseInputType.MoveUp: value = delta.MoveUp; break;
                    case MouseInputType.MoveDown: value = delta.MoveDown; break;
                    case MouseInputType.MoveLeft: value = delta.MoveLeft; break;
                    case MouseInputType.MoveRight: value = delta.MoveRight; break;
                    case MouseInputType.WheelUp: value = delta.WheelUp; break;
                    case MouseInputType.WheelDown: value = delta.WheelDown; break;
                }

                // ボタン系のマウス入力の場合、押下していれば 1f とする
                if (map.mouseInput == MouseInputType.LeftButton ||
                    map.mouseInput == MouseInputType.RightButton ||
                    map.mouseInput == MouseInputType.MiddleButton)
                {
                    if (_mouse.GetButton(map.gamepadInput))
                    {
                        value = 1f;
                    }
                }

                // 入力がなければ反映せずにスキップ
                if (value == 0f)
                {
                    continue;
                }

                // 対応する仮想ゲームパッドの状態に反映
                ApplyGamepadMapping(
                    ref buttonA, ref buttonB, ref buttonX, ref buttonY,
                    ref leftShoulder, ref rightShoulder,
                    ref leftTrigger, ref rightTrigger,
                    ref leftStickButton, ref rightStickButton,
                    ref leftStick, ref rightStick, ref dpad,
                    ref startButton, ref selectButton,
                    map.gamepadInput, value);
            }
        }

        /// <summary>
        /// 指定されたゲームパッド入力タイプを仮想ゲームパッド状態に反映する
        /// </summary>
        /// <param name="buttonA">ButtonA の状態参照</param>
        /// <param name="buttonB">ButtonB の状態参照</param>
        /// <param name="buttonX">ButtonX の状態参照</param>
        /// <param name="buttonY">ButtonY の状態参照</param>
        /// <param name="leftShoulder">LeftShoulder の状態参照</param>
        /// <param name="rightShoulder">RightShoulder の状態参照</param>
        /// <param name="leftTrigger">LeftTrigger の状態参照</param>
        /// <param name="rightTrigger">RightTrigger の状態参照</param>
        /// <param name="leftStickButton">左スティックボタンの状態参照</param>
        /// <param name="rightStickButton">右スティックボタンの状態参照</param>
        /// <param name="leftStick">左スティック入力ベクトル参照</param>
        /// <param name="rightStick">右スティック入力ベクトル参照</param>
        /// <param name="dpad">D-Pad 入力ベクトル参照</param>
        /// <param name="startButton">Start ボタンの状態参照</param>
        /// <param name="selectButton">Select ボタンの状態参照</param>
        /// <param name="type">反映するゲームパッド入力種別</param>
        /// <param name="value">入力値（押下なら 1f、軸なら変位量）</param>
        private void ApplyGamepadMapping(
            ref bool buttonA, ref bool buttonB, ref bool buttonX, ref bool buttonY,
            ref bool leftShoulder, ref bool rightShoulder,
            ref bool leftTrigger, ref bool rightTrigger,
            ref bool leftStickButton, ref bool rightStickButton,
            ref Vector2 leftStick, ref Vector2 rightStick, ref Vector2 dpad,
            ref bool startButton, ref bool selectButton,
            in GamepadInputType type, in float value)
        {
            // 値が 0 以下なら何も反映せずに終了
            if (value <= 0f)
            {
                return;
            }

            // ボタン系入力を反映
            switch (type)
            {
                case GamepadInputType.ButtonA: buttonA = true; break;
                case GamepadInputType.ButtonB: buttonB = true; break;
                case GamepadInputType.ButtonX: buttonX = true; break;
                case GamepadInputType.ButtonY: buttonY = true; break;
                case GamepadInputType.LeftShoulder: leftShoulder = true; break;
                case GamepadInputType.RightShoulder: rightShoulder = true; break;
                case GamepadInputType.LeftTrigger: leftTrigger = true; break;
                case GamepadInputType.RightTrigger: rightTrigger = true; break;
                case GamepadInputType.LeftStickButton: leftStickButton = true; break;
                case GamepadInputType.RightStickButton: rightStickButton = true; break;
                case GamepadInputType.Start: startButton = true; break;
                case GamepadInputType.Select: selectButton = true; break;
            }

            // スティック系入力を反映
            switch (type)
            {
                case GamepadInputType.LeftStickUp: leftStick.y += value; break;
                case GamepadInputType.LeftStickDown: leftStick.y -= value; break;
                case GamepadInputType.LeftStickLeft: leftStick.x -= value; break;
                case GamepadInputType.LeftStickRight: leftStick.x += value; break;
                case GamepadInputType.RightStickUp: rightStick.y += value; break;
                case GamepadInputType.RightStickDown: rightStick.y -= value; break;
                case GamepadInputType.RightStickLeft: rightStick.x -= value; break;
                case GamepadInputType.RightStickRight: rightStick.x += value; break;
                case GamepadInputType.DPadUp: dpad.y += value; break;
                case GamepadInputType.DPadDown: dpad.y -= value; break;
                case GamepadInputType.DPadLeft: dpad.x -= value; break;
                case GamepadInputType.DPadRight: dpad.x += value; break;
            }
        }
    }
}