// ======================================================
// BoardIndex.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-03
// 更新日時 : 2026-04-03
// 概要     : 盤面上の3次元インデックスを表す構造体
// ======================================================

namespace BoardSystem.Domain
{
    /// <summary>
    /// 盤面座標インデックス
    /// </summary>
    public readonly struct BoardIndex
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>X座標</summary>
        public readonly int X;

        /// <summary>Y座標</summary>
        public readonly int Y;

        /// <summary>Z座標</summary>
        public readonly int Z;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="z">Z座標</param>
        public BoardIndex(in int x, in int y, in int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}