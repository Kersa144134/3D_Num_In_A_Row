// ======================================================
// ButtonState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
// 概要     : ボタンの押している状態を管理するクラス
// ======================================================

using System;
using UniRx;

namespace InputSystem.Domain
{
    /// <summary>
    /// ボタン状態管理用クラス
    /// 押している / 押す / 離す状態を管理し、イベント通知を行う
    /// </summary>
    public class ButtonState
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>前フレームの押している状態</summary>
        private bool _wasPressed = false;

        // ======================================================
        // UniRx 関連
        // ======================================================

        /// <summary>押すイベント用 Subject</summary>
        private readonly Subject<Unit> _onDown = new Subject<Unit>();

        /// <summary>離すイベント用 Subject</summary>
        private readonly Subject<Unit> _onUp = new Subject<Unit>();

        /// <summary>押すイベントストリーム</summary>
        public IObservable<Unit> OnDown => _onDown;

        /// <summary>離すイベントストリーム</summary>
        public IObservable<Unit> OnUp => _onUp;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 現在の押している状態から Down / Up を更新しイベント通知
        /// </summary>
        /// <param name="current">現在の押している状態</param>
        public void Update(in bool current)
        {

            // 押す
            if (current && !_wasPressed)
            {
                _onDown.OnNext(Unit.Default);
            }

            // 離す
            if (!current && _wasPressed)
            {
                _onUp.OnNext(Unit.Default);
            }

            // 前フレームの押している状態を更新
            _wasPressed = current;
        }
    }
}