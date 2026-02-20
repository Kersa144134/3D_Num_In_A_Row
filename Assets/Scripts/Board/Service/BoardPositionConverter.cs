// ======================================================
// BoardPositionConverter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 概要     : 盤面サイズに応じたワールド座標と列インデックス変換
// ======================================================

using UnityEngine;

namespace BoardSystem.Service
{
    /// <summary>
    /// 盤面ワールド座標と列インデックスの変換サービス
    /// </summary>
    public sealed class BoardPositionConverter
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 盤面サイズ
        /// </summary>
        private readonly int _boardSize;

        /// <summary>
        /// 中心インデックス番号
        /// </summary>
        private readonly float _centerIndex;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        public BoardPositionConverter(in int boardSize)
        {
            _boardSize = boardSize;

            // 偶数/奇数で中央補正を変更
            _centerIndex = (_boardSize % 2 == 0)
                ? _boardSize / 2f - 0.5f
                : (_boardSize - 1) / 2f;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ワールドX,Z座標から列インデックスに変換
        /// </summary>
        public void WorldPositionToColumn(
            in float cellSpacing,
            in float worldX,
            in float worldZ,
            out int x,
            out int z)
        {
            // ワールド座標を列インデックスに変換
            x = Mathf.RoundToInt(worldX / cellSpacing + _centerIndex);
            x = Mathf.Clamp(x, 0, _boardSize - 1);

            z = Mathf.RoundToInt(worldZ / cellSpacing + _centerIndex);
            z = Mathf.Clamp(z, 0, _boardSize - 1);
        }

        /// <summary>
        /// 列インデックスからワールド座標に変換
        /// </summary>
        /// <param name="columnX">列Xインデックス</param>
        /// <param name="columnX">列Yインデックス</param>
        /// <param name="columnX">列Zインデックス</param>
        /// <param name="cellSpacing">マス間隔</param>
        /// <returns>ワールド座標</returns>
        public void ColumnToWorldPosition(
            in float cellSpacing,
            in int columnX,
            in int columnY,
            in int columnZ,
            out float worldX,
            out float worldY,
            out float worldZ)
        {
            worldX = (columnX - _centerIndex) * cellSpacing;
            worldY = (columnY - _centerIndex) * cellSpacing;
            worldZ = (columnZ - _centerIndex) * cellSpacing;
        }
    }
}