// ======================================================
// BoardDeleteHandler.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-10
// 更新日時 : 2026-06-10
// 概要     : ライン演出用の始点・終点ワールド座標
// ======================================================

using UnityEngine;

namespace BoardSystem.Domain
{
    /// <summary>
    /// ライン演出用の始点・終点ワールド座標
    /// </summary>
    public readonly struct LinePositionInfo
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ライン始点ワールド座標</summary>
        public Vector3 StartPosition
        {
            get;
        }

        /// <summary>ライン終点ワールド座標</summary>
        public Vector3 EndPosition
        {
            get;
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ライン位置情報を生成
        /// </summary>
        /// <param name="startPosition">ライン始点ワールド座標</param>
        /// <param name="endPosition">ライン終点ワールド座標</param>
        public LinePositionInfo(in Vector3 startPosition, in Vector3 endPosition)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
    }
}