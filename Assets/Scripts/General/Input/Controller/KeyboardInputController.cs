// ======================================================
// KeyboardInputController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-06
// 更新日時 : 2025-11-11
// 概要     : InputMapping に基づきマウス入力を解析し、
//            キーボード入力を取得するコントローラクラス
// ======================================================

using System.Linq;
using UnityEngine;
using InputSystem.Data;

namespace InputSystem.Controller
{
    /// <summary>
    /// キーボード入力を処理し、ゲームパッド互換の抽象入力種別に対応した押下状態を返すクラス
    /// </summary>
    public class KeyboardInputController
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>入力マッピング情報</summary>
        private readonly InputMapping[] _mappings;

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
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定されたゲームパッド入力に対応するキーが押下されているかを返す
        /// </summary>
        public bool GetButton(in GamepadInputType inputType)
        {
            InputMapping map = default;
            bool found = false;

            // ループで対応するマッピングを検索
            foreach (InputMapping m in _mappings)
            {
                if (m.gamepadInput == inputType)
                {
                    map = m;
                    found = true;
                    break;
                }
            }

            // 見つからなければ false
            if (!found || map.keyCode == KeyCode.None)
            {
                return false;
            }

            return Input.GetKey(map.keyCode);
        }
    }
}