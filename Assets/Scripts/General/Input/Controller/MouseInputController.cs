// ======================================================
// MouseInputController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-06
// 更新日時 : 2025-11-11
// 概要     : InputMapping に基づきマウス入力を解析し、
//            マウス入力を取得するコントローラクラス
// ======================================================

using UnityEngine;
using InputSystem.Data;

namespace InputSystem.Controller
{
    /// <summary>
    /// マウス入力を処理し、ゲームパッド互換の抽象入力値を取得するクラス
    /// </summary>
    public class MouseInputController
    {
        // ======================================================
        // 構造体
        // ======================================================

        /// <summary>
        /// マウス移動量・スクロール量を方向別に保持
        /// </summary>
        public struct MouseDelta
        {
            public float MoveLeft;
            public float MoveRight;
            public float MoveUp;
            public float MoveDown;
            public float WheelUp;
            public float WheelDown;
        }

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>入力マッピング情報</summary>
        private readonly InputMapping[] _mappings;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>前フレームのマウス座標</summary>
        private Vector2 _prevMousePosition;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// InputMapping 配列を受け取り初期化
        /// </summary>
        /// <param name="mappings">InputMappingConfig などから取得したマッピング配列</param>
        public MouseInputController(in InputMapping[] mappings)
        {
            // null安全のため空配列を補填
            _mappings = mappings ?? new InputMapping[0];

            // 初期座標を現在のマウス位置で初期化
            _prevMousePosition = Input.mousePosition;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // ボタン入力
        // --------------------------------------------------
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
            if (!found || !map.IsMouseBinding)
            {
                return false;
            }

            // マウス入力判定
            switch (map.mouseInput)
            {
                case MouseInputType.LeftButton: return Input.GetMouseButton(0);
                case MouseInputType.RightButton: return Input.GetMouseButton(1);
                case MouseInputType.MiddleButton: return Input.GetMouseButton(2);
                default: return false;
            }
        }

        // --------------------------------------------------
        // ポインタ / エイム系入力
        // --------------------------------------------------
        /// <summary>
        /// 前フレームからのマウス移動量とホイール回転量を方向別に取得し、次フレーム用に更新
        /// </summary>
        public MouseDelta GetMouseDelta()
        {
            MouseDelta delta = new MouseDelta();

            // マウス移動量
            Vector2 current = Input.mousePosition;
            Vector2 move = current - _prevMousePosition;
            _prevMousePosition = current;

            if (move.x > 0f) delta.MoveRight = move.x;
            else if (move.x < 0f) delta.MoveLeft = -move.x;

            if (move.y > 0f) delta.MoveUp = move.y;
            else if (move.y < 0f) delta.MoveDown = -move.y;

            // ホイール回転量
            float wheel = Input.mouseScrollDelta.y;
            if (wheel > 0f) delta.WheelUp = wheel;
            else if (wheel < 0f) delta.WheelDown = -wheel;

            return delta;
        }

        /// <summary>
        /// マウスの現在座標（スクリーン座標）を取得
        /// </summary>
        public Vector2 GetPointerPosition()
        {
            return Input.mousePosition;
        }
    }
}