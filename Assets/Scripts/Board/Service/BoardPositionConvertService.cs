// ======================================================
// BoardPositionConvertService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-03-06
// 概要     : 盤面サイズに応じたワールド座標と列インデックス変換サービス
// ======================================================

using UnityEngine;

namespace BoardSystem.Service
{
    /// <summary>
    /// 盤面ワールド座標と列インデックスの変換サービス
    /// </summary>
    public sealed class BoardPositionConvertService
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        /// <summary>中心インデックス番号</summary>
        private readonly float _centerIndex;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        public BoardPositionConvertService(in int boardSize)
        {
            _boardSize = boardSize;

            // 偶数/奇数で中央補正を変更
            _centerIndex = (_boardSize % 2 == 0)
                ? (_boardSize / 2f) - 0.5f
                : (_boardSize - 1) / 2f;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ワールド座標から列インデックスに変換
        /// </summary>
        /// <param name="worldX">ワールド X 座標</param>
        /// <param name="worldZ">ワールド Z 座標<</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        public void WorldPositionToColumn(
            in float cellSpacing,
            in float worldX,
            in float worldZ,
            out int columnX,
            out int columnZ)
        {
            columnX = Mathf.RoundToInt(worldX / cellSpacing + _centerIndex);
            columnX = Mathf.Clamp(columnX, 0, _boardSize - 1);

            columnZ = Mathf.RoundToInt(worldZ / cellSpacing + _centerIndex);
            columnZ = Mathf.Clamp(columnZ, 0, _boardSize - 1);
        }

        /// <summary>
        /// 列インデックスからワールド座標に変換
        /// </summary>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnY">列 Y インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <param name="worldX">ワールド X 座標<</param>
        /// <param name="worldY">ワールド Y 座標<</param>
        /// <param name="worldZ">ワールド Z 座標<</param>
        /// <param name="cellSpacing">マス間隔</param>
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