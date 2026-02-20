// ======================================================
// InputMappingConfig.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
// 概要     : ゲームパッド・キーボード・マウス入力を統一的に管理する
//            マッピング情報を定義
// ======================================================

using System;
using UnityEngine;

namespace InputSystem.Data
{
    // ======================================================
    // 列挙体
    // ======================================================

    /// <summary>ゲームパッド入力種別</summary>
    public enum GamepadInputType
    {
        ButtonA,
        ButtonB,
        ButtonX,
        ButtonY,
        LeftShoulder,
        RightShoulder,
        LeftTrigger,
        RightTrigger,
        LeftStickButton,
        RightStickButton,
        LeftStickLeft,
        LeftStickRight,
        LeftStickUp,
        LeftStickDown,
        RightStickLeft,
        RightStickRight,
        RightStickUp,
        RightStickDown,
        DPadLeft,
        DPadRight,
        DPadUp,
        DPadDown,
        Start,
        Select
    }

    /// <summary>マウス入力種別</summary>
    public enum MouseInputType
    {
        None,
        LeftButton,
        RightButton,
        MiddleButton,
        MoveLeft,
        MoveRight,
        MoveUp,
        MoveDown,
        WheelUp,
        WheelDown
    }

    // ======================================================
    // 構造体
    // ======================================================

    /// <summary>
    /// 単一入力のマッピング情報を表す構造体
    /// キーボードまたはマウスのどのキー/ボタンが、どのゲームパッド入力に対応するかを保持する
    /// </summary>
    [Serializable]
    public struct InputMapping
    {
        /// <summary>ゲームパッド入力種別</summary>
        public GamepadInputType gamepadInput;

        /// <summary>キーボードキー割当</summary>
        public KeyCode keyCode;

        /// <summary>マウス入力割当</summary>
        public MouseInputType mouseInput;

        /// <summary>キーボードバインディングがあるか</summary>
        public bool IsKeyboardBinding => keyCode != KeyCode.None;

        /// <summary>マウスバインディングがあるか</summary>
        public bool IsMouseBinding => mouseInput != MouseInputType.None;
    }

    // ======================================================
    // ScriptableObject
    // ======================================================

    /// <summary>
    /// 複数の InputMapping をまとめて管理する ScriptableObject
    /// インスペクタ上で設定したマッピングを読み込むために使用
    /// </summary>
    [CreateAssetMenu(fileName = "DefaultInputMapping", menuName = "InputSystem/DefaultInputMapping")]
    public class InputMappingConfig : ScriptableObject
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>ゲームパッド入力とキーボード／マウス対応の一覧</summary>
        public InputMapping[] Mappings;

#if UNITY_EDITOR
        // ======================================================
        // エディタ補助
        // ======================================================

        /// <summary>
        /// Unityエディタ上で列挙体変更に追従して配列を自動更新する
        /// </summary>
        private void OnValidate()
        {
            // ゲームパッド入力列挙体の全値を取得
            Array values = Enum.GetValues(typeof(GamepadInputType));

            // 配列サイズが異なる場合は再構築
            if (Mappings == null || Mappings.Length != values.Length)
            {
                InputMapping[] newMappings = new InputMapping[values.Length];

                for (int i = 0; i < values.Length; i++)
                {
                    GamepadInputType input = (GamepadInputType)values.GetValue(i);

                    // 既存マッピングを探す
                    InputMapping existing = FindExistingMapping(input);

                    // 既存のものがあれば利用、なければ未設定初期化
                    newMappings[i] = existing.gamepadInput == input
                        ? existing
                        : new InputMapping
                        {
                            gamepadInput = input,
                            keyCode = KeyCode.None,
                            mouseInput = MouseInputType.None
                        };
                }

                Mappings = newMappings;
            }
        }

        /// <summary>
        /// 既存マッピング配列から該当する入力を検索して返す
        /// </summary>
        /// <param name="input">検索対象のゲームパッド入力タイプ</param>
        /// <returns>既存マッピング、存在しなければ未設定構造体</returns>
        private InputMapping FindExistingMapping(GamepadInputType input)
        {
            if (Mappings == null)
            {
                return new InputMapping
                {
                    gamepadInput = input,
                    keyCode = KeyCode.None,
                    mouseInput = MouseInputType.None
                };
            }

            foreach (InputMapping map in Mappings)
            {
                if (map.gamepadInput == input)
                {
                    return map;
                }
            }

            return new InputMapping
            {
                gamepadInput = input,
                keyCode = KeyCode.None,
                mouseInput = MouseInputType.None
            };
        }
#endif
    }
}