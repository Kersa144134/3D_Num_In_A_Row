// ======================================================
// BoardPositionConvertService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-01
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

        /// <summary>盤面のワールド原点位置</summary>
        private readonly Vector3 _originPosition;
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        /// <param name="originPosition">盤面のワールド原点</param>
        public BoardPositionConvertService(
            in int boardSize,
            in Vector3 originPosition)
        {
            _boardSize = boardSize;
            _originPosition = originPosition;

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
        public void WorldPositionToColumn(
            in float cellSpacing,
            in float worldX,
            in float worldZ,
            out int columnX,
            out int columnZ)
        {
            // ローカル座標に変換
            float localX = worldX - _originPosition.x;
            float localZ = worldZ - _originPosition.z;

            // インデックス変換
            columnX = Mathf.RoundToInt(localX / cellSpacing + _centerIndex);
            columnZ = Mathf.RoundToInt(localZ / cellSpacing + _centerIndex);

            // 範囲制限
            columnX = Mathf.Clamp(columnX, 0, _boardSize - 1);
            columnZ = Mathf.Clamp(columnZ, 0, _boardSize - 1);
        }

        /// <summary>
        /// 列インデックスからワールド座標に変換
        /// </summary>
        public void ColumnToWorldPosition(
            in float cellSpacing,
            in int columnX,
            in int columnY,
            in int columnZ,
            out float worldX,
            out float worldY,
            out float worldZ)
        {
            // ローカル座標生成
            float localX = (columnX - _centerIndex) * cellSpacing;
            float localY = (columnY - _centerIndex) * cellSpacing;
            float localZ = (columnZ - _centerIndex) * cellSpacing;

            // 原点を加算してワールド座標へ
            worldX = localX + _originPosition.x;
            worldY = localY + _originPosition.y;
            worldZ = localZ + _originPosition.z;
        }

        /// <summary>
        /// 駒生成時の上空ワールドY座標を取得
        /// </summary>
        /// <param name="cellSpacing">セル間隔</param>
        /// <returns>ワールドY座標</returns>
        public float GetSpawnWorldY(
            in float cellSpacing)
        {
            // 生成用Yインデックス算出
            int spawnIndexY = _boardSize;

            // ローカルY座標算出
            float localY =
                (spawnIndexY - _centerIndex) *
                cellSpacing;

            // ワールドY座標へ変換
            float worldY =
                localY +
                _originPosition.y;

            return worldY;
        }
    }
}