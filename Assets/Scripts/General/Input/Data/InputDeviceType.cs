// ======================================================
// InputDeviceType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-26
// 更新日時 : 2026-04-26
// 概要     : 入力デバイス種別を定義する列挙体
// ======================================================

namespace InputSystem.Data
{
    /// <summary>
    /// 入力デバイス種別
    /// </summary>
    public enum InputDeviceType
    {
        /// <summary>物理ゲームパッド入力</summary>
        Gamepad,

        /// <summary>キーボード・マウスによる仮想入力</summary>
        Virtualpad
    }
}