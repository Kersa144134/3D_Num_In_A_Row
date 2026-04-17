// ======================================================
// LineDeleteResult.cs
// 概要 : ライン削除処理の結果データ
// ======================================================

using System.Collections.Generic;

namespace BoardSystem.Domain
{
    /// <summary>
    /// ライン削除結果
    /// </summary>
    public readonly struct LineDeleteResult
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 再配置対象列
        /// </summary>
        public readonly IReadOnlyList<(int x, int z)> RepositionColumns;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LineDeleteResult(
            IReadOnlyList<(int x, int z)> repositionColumns)
        {
            RepositionColumns = repositionColumns;
        }
    }
}