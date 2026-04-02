// ======================================================
// BoardView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-02
// 概要     : 3D 目並べゲームの表示を制御するクラス（UniTask対応）
// ======================================================

using UnityEngine;
using Cysharp.Threading.Tasks;
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
        private readonly BoardPositionConvertService _boardPositionConvert;

        /// <summary>
        /// 落下アニメーションサービス
        /// </summary>
        private readonly DropAnimationService _dropAnimation = new DropAnimationService();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 親 Transform
        /// </summary>
        private readonly Transform _root;

        /// <summary>
        /// プレイヤー 1 Prefab
        /// </summary>
        private readonly GameObject _playerOnePrefab;

        /// <summary>
        /// プレイヤー 2 Prefab
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
        /// ビュー生成
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

            _boardPositionConvert = new BoardPositionConvertService(
                boardSize,
                root.position
            );
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
            _boardPositionConvert.WorldPositionToColumn(
                _cellSpacing,
                worldX,
                worldZ,
                out x,
                out z
            );
        }

        /// <summary>
        /// 駒生成
        /// </summary>
        public async UniTask SpawnPieceAsync(
            int x,
            int y,
            int z,
            int player)
        {
            // --------------------------------------------------
            // 目標ワールド座標の算出
            // --------------------------------------------------
            float targetX;
            float targetY;
            float targetZ;

            // グリッド座標 → ワールド座標へ変換
            _boardPositionConvert.ColumnToWorldPosition(
                _cellSpacing,
                x,
                y,
                z,
                out targetX,
                out targetY,
                out targetZ
            );

            // --------------------------------------------------
            // 生成位置の算出
            // --------------------------------------------------
            // Y のみ指定位置から落下
            float spawnY = _boardPositionConvert.GetSpawnWorldY(_cellSpacing);

            // 生成位置
            Vector3 start = new Vector3(
                targetX,
                spawnY,
                targetZ
            );

            // 最終到達位置
            Vector3 end = new Vector3(
                targetX,
                targetY,
                targetZ
            );

            // --------------------------------------------------
            // プレハブ選択
            // --------------------------------------------------
            GameObject prefab =
                player == 1
                ? _playerOnePrefab
                : _playerTwoPrefab;

            // --------------------------------------------------
            // インスタンス生成
            // --------------------------------------------------
            GameObject piece = Object.Instantiate(
                prefab,
                start,
                Quaternion.identity,
                _root
             );

            // --------------------------------------------------
            // スケール調整
            // --------------------------------------------------
            float scaleFactor =
                1f /
                (_boardSize + 0.5f);

            piece.transform.localScale =
                Vector3.one *
                scaleFactor;

            // --------------------------------------------------
            // 落下アニメーション
            // --------------------------------------------------
            await _dropAnimation.AnimateDropAsync(
                piece.transform,
                start,
                end
            );
        }
    }
}