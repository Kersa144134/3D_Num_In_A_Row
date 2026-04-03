// ======================================================
// PieceData.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-03
// 更新日時 : 2026-04-03
// 概要     : 駒の表示情報を保持する構造体
// ======================================================

using UnityEngine;

namespace BoardSystem.Data
{
    /// <summary>
    /// 駒データ
    /// </summary>
    public readonly struct PieceData
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>Transform参照</summary>
        public readonly Transform Transform;

        /// <summary>プレイヤー番号</summary>
        public readonly int Player;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="transform">対象Transform</param>
        /// <param name="player">プレイヤー番号</param>
        public PieceData(in Transform transform, in int player)
        {
            // Transformを設定
            Transform = transform;

            // プレイヤー番号を設定
            Player = player;
        }
    }
}