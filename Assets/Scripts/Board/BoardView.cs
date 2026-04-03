// ======================================================
// BoardView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-02
// 概要     : 3D 目並べゲームの表示を制御するクラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using BoardSystem.Service;
using BoardSystem.Data;

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

        /// <summary>座標変換サービス</summary>
        private readonly BoardPositionConvertService _boardPositionConvert;

        /// <summary>落下アニメーションサービス</summary>
        private readonly DropAnimationService _dropAnimation = new DropAnimationService();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>親 Transform</summary>
        private readonly Transform _root;

        /// <summary>プレイヤー 1 Prefab</summary>
        private readonly GameObject _playerOnePrefab;

        /// <summary>プレイヤー 2 Prefab</summary>
        private readonly GameObject _playerTwoPrefab;

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        /// <summary>セル間隔</summary>
        private readonly float _cellSpacing;

        /// <summary>
        /// 生成駒辞書
        /// BoardIndex をキーとして駒データを管理
        /// </summary>
        private readonly Dictionary<BoardIndex, PieceData> _pieces;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardView(
            in Transform root,
            in int boardSize,
            in GameObject p1,
            in GameObject p2)
        {
            _root = root;
            _playerOnePrefab = p1;
            _playerTwoPrefab = p2;
            _boardSize = boardSize;
            _cellSpacing = root.localScale.x / boardSize;

            _boardPositionConvert = new BoardPositionConvertService(
                boardSize,
                root.position
            );

            // 最大配置数を算出
            int capacity =
                _boardSize *
                _boardSize *
                _boardSize;

            // Dictionary 初期化
            _pieces = new Dictionary<BoardIndex, PieceData>(capacity);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ワールド座標から列インデックスに変換
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
            _boardPositionConvert.ColumnToWorldPosition(
                _cellSpacing,
                x,
                y,
                z,
                out float targetX,
                out float targetY,
                out float targetZ
            );

            // --------------------------------------------------
            // 生成位置
            // Y のみ上空からスタート
            // --------------------------------------------------
            float spawnY = _boardPositionConvert.GetSpawnWorldY(_cellSpacing);

            Vector3 start = new Vector3(targetX, spawnY, targetZ);
            Vector3 end = new Vector3(targetX, targetY, targetZ);

            // --------------------------------------------------
            // 駒生成
            // --------------------------------------------------
            GameObject prefab = player == 1 ? _playerOnePrefab : _playerTwoPrefab;

            // インスタンス生成
            GameObject piece = Object.Instantiate(
                prefab,
                start,
                Quaternion.identity,
                _root
            );

            // スケール調整
            float scaleFactor = 1f / (_boardSize + 0.5f);
            piece.transform.localScale = Vector3.one * scaleFactor;

            // --------------------------------------------------
            // 落下アニメーション
            // --------------------------------------------------
            await _dropAnimation.AnimateDropAsync(piece.transform, start, end);

            // --------------------------------------------------
            // 駒辞書に追加
            // --------------------------------------------------
            // 座標を BoardIndex に変換
            BoardIndex index = new BoardIndex(x, y, z);

            // 駒データ生成
            PieceData pieceData = new PieceData(piece.transform, player);

            // 辞書に登録
            _pieces[index] = pieceData;
        }

        /// <summary>
        /// 指定座標の駒を削除
        /// </summary>
        public void DeletePiece(in BoardIndex index)
        {
            if (_pieces.TryGetValue(index, out PieceData piece))
            {
                Object.Destroy(piece.Transform.gameObject);
                _pieces.Remove(index);
            }
        }
    }
}