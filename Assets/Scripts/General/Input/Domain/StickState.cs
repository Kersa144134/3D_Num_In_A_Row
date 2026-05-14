// ======================================================
// StickState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
// 概要     : スティックの押している状態を管理するクラス
// ======================================================

using System;
using UnityEngine;
using UniRx;

namespace InputSystem.Domain
{
    /// <summary>
    /// スティック状態管理用クラス
    /// 入力 / 押す状態を管理し、イベント通知を行う
    /// </summary>
    public class StickState
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>スティックの入力状態</summary>
        private Vector2 _angle = Vector2.zero;

        /// <summary>前フレームの押している状態</summary>
        private bool _wasPressed = false;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>スティックの入力状態</summary>
        public Vector2 Angle => _angle;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>押すイベント Subject</summary>
        private readonly Subject<Vector2> _onDown = new Subject<Vector2>();

        /// <summary>押すイベントストリーム</summary>
        public IObservable<Vector2> OnDown => _onDown;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>入力判定を行う最小スティック強度</summary>
        private const float INPUT_THRESHOLD = 0.5f;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在の入力状態から Down を更新しイベント通知
        /// </summary>
        /// <param name="current">現在の入力状態</param>
        public void Update(in Vector2 current)
        {
            // 入力状態を更新
            _angle = current;

            // 現在の入力強度を取得
            float magnitude = current.magnitude;

            // 入力閾値未満
            if (magnitude < INPUT_THRESHOLD)
            {
                // 押している状態を解除
                _wasPressed = false;

                return;
            }

            // 押す
            if (!_wasPressed)
            {
                // 押している状態を更新
                _wasPressed = true;

                // 最も強い方向を取得
                Vector2 direction = GetDominantDirection(current);

                _onDown.OnNext(direction);
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 入力ベクトルから最も強い4方向を取得
        /// </summary>
        /// <param name="input">入力ベクトル</param>
        /// <returns>4方向ベクトル</returns>
        private Vector2 GetDominantDirection(in Vector2 input)
        {
            // 横方向の絶対値を取得
            float horizontalAbs = Mathf.Abs(input.x);

            // 縦方向の絶対値を取得
            float verticalAbs = Mathf.Abs(input.y);

            // 横入力が優先される場合
            if (horizontalAbs > verticalAbs)
            {
                // 右方向
                if (input.x > 0.0f)
                {
                    return Vector2.right;
                }

                // 左方向
                return Vector2.left;
            }

            // 上方向
            if (input.y > 0.0f)
            {
                return Vector2.up;
            }

            // 下方向
            return Vector2.down;
        }
    }
}