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

        /// <summary>セル間隔</summary>
        private readonly float _cellSpacing;

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        /// <summary>生成駒リスト（座標とプレイヤー番号を管理）</summary>
        private readonly List<(Transform transform, int player, int x, int y, int z)> _pieces =
            new List<(Transform, int, int, int, int)>();

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
            _boardSize = boardSize;
            _playerOnePrefab = p1;
            _playerTwoPrefab = p2;

            _cellSpacing = root.localScale.x / boardSize;

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
            // プレハブ選択
            // --------------------------------------------------
            GameObject prefab = player == 1 ? _playerOnePrefab : _playerTwoPrefab;

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
            float scaleFactor = 1f / (_boardSize + 0.5f);
            piece.transform.localScale = Vector3.one * scaleFactor;

            // --------------------------------------------------
            // 落下アニメーション
            // --------------------------------------------------
            await _dropAnimation.AnimateDropAsync(piece.transform, start, end);

            // --------------------------------------------------
            // 駒リストに追加
            // 座標とプレイヤー情報を保持
            // --------------------------------------------------
            _pieces.Add((piece.transform, player, x, y, z));
        }

        /// <summary>
        /// 指定座標の駒を削除
        /// </summary>
        public void DeletePiece(in int x, in int y, in int z)
        {
            // --------------------------------------------------
            // リストから検索
            // --------------------------------------------------
            for (int i = _pieces.Count - 1; i >= 0; i--)
            {
                var p = _pieces[i];
                if (p.x == x && p.y == y && p.z == z)
                {
                    // オブジェクト削除
                    Object.Destroy(p.transform.gameObject);

                    // リストからも削除
                    _pieces.RemoveAt(i);
                    break;
                }
            }
        }
    }
}