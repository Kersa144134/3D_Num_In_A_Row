// ======================================================
// ButtonState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
// 概要     : ボタンの押下状態を管理するクラス
// ======================================================

namespace InputSystem.Data
{
    /// <summary>
    /// ボタン状態管理用クラス
    /// 押下中 / 押下開始 / 離上 の状態を管理
    /// </summary>
    public class ButtonState
    {
        /// <summary>現在押下中かどうか</summary>
        public bool IsPressed;

        /// <summary>押下した瞬間かどうか</summary>
        public bool Down;

        /// <summary>離した瞬間かどうか</summary>
        public bool Up;

        /// <summary>前フレームの押下状態</summary>
        private bool _wasPressed;

        /// <summary>現在の押下状態から Down / Up を更新</summary>
        /// <param name="current">最新の押下状態</param>
        public void Update(bool current)
        {
            // 前フレームの状態と比較して押下開始/離上を判定
            Down = current && !_wasPressed;
            Up = !current && _wasPressed;

            // 現在押下状態を保存
            IsPressed = current;

            // 次フレーム判定用に保持
            _wasPressed = current;
        }
    }
}