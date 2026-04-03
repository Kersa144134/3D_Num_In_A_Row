// ======================================================
// BoardIndex.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-03
// 更新日時 : 2026-04-03
// 概要     : 盤面上の3次元インデックスを表す構造体
// ======================================================

using System;

namespace BoardSystem.Data
{
    /// <summary>
    /// 盤面座標インデックス
    /// </summary>
    public readonly struct BoardIndex
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>X座標</summary>
        public readonly int X;

        /// <summary>Y座標</summary>
        public readonly int Y;

        /// <summary>Z座標</summary>
        public readonly int Z;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// 無効インデックス
        /// </summary>
        public static readonly BoardIndex Invalid =
            new BoardIndex(-1, -1, -1);

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="z">Z座標</param>
        public BoardIndex(int x, int y, int z)
        {
            // X座標を設定
            X = x;

            // Y座標を設定
            Y = y;

            // Z座標を設定
            Z = z;
        }
    }
}