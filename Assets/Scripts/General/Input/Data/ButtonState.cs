// ======================================================
// ButtonState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
// 概要     : ボタンの押下状態を管理するクラス
// ======================================================

using System;
using UniRx;

namespace InputSystem.Data
{
    /// <summary>
    /// ボタン状態管理用クラス
    /// 押下中 / 押下開始 / 離上 の状態を管理し、イベント通知も行う
    /// </summary>
    public class ButtonState
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>前フレームの押下状態</summary>
        private bool _wasPressed;

        /// <summary>押下時イベント</summary>
        private readonly Subject<Unit> _onDown = new Subject<Unit>();

        /// <summary>離上時イベント</summary>
        private readonly Subject<Unit> _onUp = new Subject<Unit>();

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在押下中かどうか</summary>
        public bool IsPressed { get; private set; }

        /// <summary>押下時イベント購読用</summary>
        public IObservable<Unit> OnDown => _onDown;

        /// <summary>離上時イベント購読用</summary>
        public IObservable<Unit> OnUp => _onUp;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在の押下状態から Down / Up を更新しイベント通知
        /// </summary>
        /// <param name="current">現在のの押下状態</param>
        public void Update(in bool current)
        {
            IsPressed = current;

            // 押下時
            if (current && !_wasPressed)
            {
                _onDown.OnNext(Unit.Default);
            }

            // 離上時
            if (!current && _wasPressed)
            {
                _onUp.OnNext(Unit.Default);
            }

            // 前フレームの押下状態を更新
            _wasPressed = current;
        }
    }
}