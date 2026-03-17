// ======================================================
// BoardView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-03-16
// 概要     : 3D 目並べゲームの表示を制御するクラス
// ======================================================

using UnityEngine;
using BoardSystem.Service;

namespace BoardSystem
{
    /// <summary>
    /// 目並べビュー
    /// </summary>
    public sealed class BoardView
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// 座標変換サービス
        /// </summary>
        private readonly BoardPositionConvertService _convertService;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 親Transform
        /// </summary>
        private readonly Transform _root;

        /// <summary>
        /// プレイヤー1Prefab
        /// </summary>
        private readonly GameObject _playerOnePrefab;

        /// <summary>
        /// プレイヤー2Prefab
        /// </summary>
        private readonly GameObject _playerTwoPrefab;

        /// <summary>
        /// セル間隔
        /// </summary>
        private readonly float _cellSpacing;

        /// <summary>
        /// 盤面サイズ
        /// </summary>
        private readonly int _boardSize;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// View生成
        /// </summary>
        public BoardView(
            Transform root,
            int boardSize,
            GameObject p1,
            GameObject p2)
        {
            _root = root;

            _boardSize = boardSize;

            _playerOnePrefab = p1;

            _playerTwoPrefab = p2;

            _cellSpacing =
                root.localScale.x /
                boardSize;

            _convertService =
                new BoardPositionConvertService(
                    boardSize);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ワールド→列変換
        /// </summary>
        public void WorldToColumn(
            in float worldX,
            in float worldZ,
            out int x,
            out int z)
        {
            _convertService.WorldPositionToColumn(
                _cellSpacing,
                worldX,
                worldZ,
                out x,
                out z);
        }

        /// <summary>
        /// 駒生成
        /// </summary>
        public void SpawnPiece(
            in int x,
            in int y,
            in int z,
            in int player)
        {
            float worldX;
            float worldY;
            float worldZ;

            _convertService.ColumnToWorldPosition(
                _cellSpacing,
                x,
                y,
                z,
                out worldX,
                out worldY,
                out worldZ);

            Vector3 position =
                new Vector3(
                    worldX,
                    worldY,
                    worldZ);

            GameObject prefab =
                player == 1
                ? _playerOnePrefab
                : _playerTwoPrefab;

            GameObject piece =
                Object.Instantiate(
                    prefab,
                    position,
                    Quaternion.identity,
                    _root);

            float scaleFactor =
                1f /
                (_boardSize + 0.5f);

            piece.transform.localScale =
                Vector3.one *
                scaleFactor;
        }
    }
}