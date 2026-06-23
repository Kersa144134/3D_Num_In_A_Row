// ======================================================
// RotationCommand.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-14
// 更新日時 : 2026-04-14
// 概要     : 回転入力を表現するコマンド構造体
// ======================================================

namespace BoardSystem.Domain
{
    /// <summary>
    /// 回転入力コマンド
    /// </summary>
    public readonly struct RotationCommand
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 回転軸（X または Z）
        /// </summary>
        public readonly RotationAxis Axis;

        /// <summary>
        /// 回転方向（正 / 負）
        /// </summary>
        public readonly RotationDirection Direction;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 回転コマンドを生成する
        /// </summary>
        /// <param name="axis">回転軸</param>
        /// <param name="direction">回転方向</param>
        public RotationCommand(
            in RotationAxis axis,
            in RotationDirection direction)
        {
            Axis = axis;
            Direction = direction;
        }
    }
}