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
            RotationAxis axis,
            RotationDirection direction)
        {
            // 回転軸を設定
            Axis = axis;

            // 回転方向を設定
            Direction = direction;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定された回転軸と方向が一致するか判定する
        /// </summary>
        /// <param name="axis">比較する回転軸</param>
        /// <param name="direction">比較する回転方向</param>
        /// <returns>一致する場合は true</returns>
        public bool Matches(
            RotationAxis axis,
            RotationDirection direction)
        {
            // 軸と方向の両方が一致する場合のみ true を返す
            return Axis == axis && Direction == direction;
        }

        /// <summary>
        /// 回転方向を反転したコマンドを取得する
        /// </summary>
        /// <returns>方向が反転した新しいコマンド</returns>
        public RotationCommand GetInverted()
        {
            // 反転後の方向を決定
            RotationDirection invertedDirection =
                Direction == RotationDirection.Positive
                ? RotationDirection.Negative
                : RotationDirection.Positive;

            // 同じ軸で方向のみ反転した新しいコマンドを返す
            return new RotationCommand(Axis, invertedDirection);
        }
    }
}