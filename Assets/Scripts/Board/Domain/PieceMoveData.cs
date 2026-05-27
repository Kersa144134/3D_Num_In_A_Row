// ======================================================
// PieceMovePlanData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-27
// 更新日時 : 2026-05-27
// 概要     : 駒移動演出に使用するデータ
// ======================================================

using UnityEngine;

namespace BoardSystem.Domain
{
    /// <summary>
    /// 駒移動データ
    /// </summary>
    public struct PieceMoveData
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>移動対象Transform</summary>
        public Transform Transform;

        /// <summary>移動開始位置</summary>
        public Vector3 Start;

        /// <summary>移動終了位置</summary>
        public Vector3 End;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="transform">移動対象Transform</param>
        /// <param name="start">移動開始位置</param>
        /// <param name="end">移動終了位置</param>
        public PieceMoveData(
            Transform transform,
            Vector3 start,
            Vector3 end)
        {
            Transform = transform;
            Start = start;
            End = end;
        }
    }
}