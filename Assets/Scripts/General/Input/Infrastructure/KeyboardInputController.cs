// ======================================================
// KeyboardInputController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-06
// 更新日時 : 2025-11-11
// 概要     : InputMapping に基づきマウス入力を解析し、
//            キーボード入力を取得するコントローラークラス
// ======================================================

using InputSystem.Domain;
using System.Collections.Generic;
using UnityEngine;

namespace InputSystem.Infrastructure
{
    /// <summary>
    /// キーボード入力を処理し、ゲームパッド互換の抽象入力種別に対応した押している状態を返すクラス
    /// </summary>
    public class KeyboardInputController
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>入力マッピング情報</summary>
        private readonly InputMapping[] _mappings;

        /// <summary>GamepadInputType に対応する KeyCode の対応表</summary>
        private readonly Dictionary<GamepadInputType, KeyCode> _keyMap
            = new Dictionary<GamepadInputType, KeyCode>();

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// InputMapping 配列を受け取り初期化
        /// </summary>
        /// <param name="mappings">InputMappingConfig などから取得したマッピング配列</param>
        public KeyboardInputController(in InputMapping[] mappings)
        {
            _mappings = mappings ?? new InputMapping[0];

            BuildDictionary();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定されたゲームパッド入力に対応するキーが押されているかを返す
        /// </summary>
        public bool GetButton(in GamepadInputType inputType)
        {
            // 未登録なら false
            if (!_keyMap.TryGetValue(inputType, out KeyCode key))
            {
                return false;
            }

            return Input.GetKey(key);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// InputMapping を辞書化
        /// </summary>
        private void BuildDictionary()
        {
            _keyMap.Clear();

            for (int i = 0; i < _mappings.Length; i++)
            {
                InputMapping map = _mappings[i];

                if (map.keyCode == KeyCode.None)
                {
                    continue;
                }

                // 既に登録済みなら上書き
                _keyMap[map.gamepadInput] = map.keyCode;
            }
        }


    }
}