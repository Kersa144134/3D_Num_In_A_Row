// ======================================================
// IGamepadInputSource.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-11-11
// 更新日時 : 2025-11-11
// 概要     : ゲームパッド入力を抽象化する共通インターフェース
//            物理パッド、仮想パッドなど入力ソースの共通化用
// ======================================================

using UnityEngine;

namespace InputSystem.Data
{
    /// <summary>
    /// ゲームパッド入力ソースの共通インターフェース
    /// このインターフェースを実装することで、物理ゲームパッド・仮想ゲームパッドなど
    /// 入力ソースを統一的に扱うことができる
    /// </summary>
    public interface IGamepadInputSource
    {
        // ======================================================
        // ボタン入力
        // ======================================================

        /// <summary>ボタンAが押下されているかを返す</summary>
        bool ButtonA { get; }

        /// <summary>ボタンBが押下されているかを返す</summary>
        bool ButtonB { get; }

        /// <summary>ボタンXが押下されているかを返す</summary>
        bool ButtonX { get; }

        /// <summary>ボタンYが押下されているかを返す</summary>
        bool ButtonY { get; }

        // ======================================================
        // ショルダー／トリガー／スティックボタン入力
        // ======================================================

        /// <summary>左ショルダーボタンが押下されているかを返す</summary>
        bool LeftShoulder { get; }

        /// <summary>右ショルダーボタンが押下されているかを返す</summary>
        bool RightShoulder { get; }

        /// <summary>左トリガーがデッドゾーン以上押されているかを返す</summary>
        bool LeftTrigger { get; }

        /// <summary>右トリガーがデッドゾーン以上押されているかを返す</summary>
        bool RightTrigger { get; }

        /// <summary>左スティックボタンが押下されているかを返す</summary>
        bool LeftStickButton { get; }

        /// <summary>右スティックボタンが押下されているかを返す</summary>
        bool RightStickButton { get; }

        // ======================================================
        // スティック／D-Pad入力
        // ======================================================

        /// <summary>左スティックの入力ベクトルを返す</summary>
        Vector2 LeftStick { get; }

        /// <summary>右スティックの入力ベクトルを返す</summary>
        Vector2 RightStick { get; }

        /// <summary>D-Padの入力ベクトルを返す</summary>
        Vector2 DPad { get; }

        // ======================================================
        // システムボタン
        // ======================================================

        /// <summary>Startボタンが押下されているかを返す</summary>
        bool StartButton { get; }

        /// <summary>Selectボタンが押下されているかを返す</summary>
        bool SelectButton { get; }
    }
}