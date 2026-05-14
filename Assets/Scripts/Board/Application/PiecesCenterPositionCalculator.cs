// ======================================================
// PiecesCenterPositionCalculator.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-14
// 更新日時 : 2026-05-14
// 概要     : 複数ピース群の中心座標を計算するユーティリティ
// ======================================================

using System.Collections.Generic;
using UnityEngine;

namespace BoardSystem.Application
{
    /// <summary>
    /// ピース群の中心座標計算ユーティリティ
    /// </summary>
    public sealed class PiecesCenterPositionCalculator
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>座標蓄積用リスト</summary>
        private readonly List<Vector3> _positions = new List<Vector3>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 任意ピースの座標を追加する
        /// </summary>
        public void AddPosition(in Transform piece)
        {
            if (piece == null)
            {
                return;
            }

            // ワールド座標を登録
            _positions.Add(piece.position);
        }

        /// <summary>
        /// 登録済み座標の中心位置を算出する
        /// </summary>
        public Vector3 CalculateCenterPosition()
        {
            // データなしの場合は原点を返却
            if (_positions.Count == 0)
            {
                return Vector3.zero;
            }

            // 合計値初期化
            Vector3 centerPosition = Vector3.zero;

            // 全座標を加算して重心を計算する
            for (int i = 0; i < _positions.Count; i++)
            {
                // 各座標を累積
                centerPosition += _positions[i];
            }

            // 平均化して中心座標を算出
            centerPosition /= _positions.Count;

            // 内部データをクリア
            _positions.Clear();

            return centerPosition;
        }
    }
}