// ======================================================
// LogViewData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : ログ表示状態を表すデータ（Model → View 転送用）
// ======================================================

using UnityEngine;

namespace UISystem.Domain
{
    /// <summary>
    /// ログの表示状態を表すデータ
    /// </summary>
    public struct LogViewData
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 表示インデックス（0 は非表示行）
        /// </summary>
        public int Index;

        /// <summary>
        /// 目標座標（UI配置用）
        /// </summary>
        public Vector2 TargetPosition;

        /// <summary>
        /// 排出中かどうか
        /// </summary>
        public bool IsExiting;

        /// <summary>
        /// 表示するログメッセージ
        /// </summary>
        public string Message;
    }
}