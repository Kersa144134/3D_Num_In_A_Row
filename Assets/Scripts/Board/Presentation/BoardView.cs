// ======================================================
// BoardView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-07
// 概要     : 3D 目並べゲームの表示を制御するクラス
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using BoardSystem.Application;
using BoardSystem.Domain;

namespace BoardSystem.Presentation
{
    /// <summary>
    /// 目並べビュー
    /// </summary>
    public sealed class BoardView
    {
        // ======================================================
        // 構造体
        // ======================================================

        /// <summary>
        /// 駒移動計画データ
        /// </summary>
        private struct MovePlanData
        {
            /// <summary>対象駒データ</summary>
            public PieceData Piece;

            /// <summary>開始位置</summary>
            public Vector3 Start;

            /// <summary>終了位置</summary>
            public Vector3 End;

            /// <summary>移動先インデックス</summary>
            public BoardIndex To;

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public MovePlanData(
                PieceData piece,
                Vector3 start,
                Vector3 end,
                BoardIndex to)
            {
                Piece = piece;
                Start = start;
                End = end;
                To = to;
            }
        }
        
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>座標変換サービス</summary>
        private readonly BoardPositionConverter _boardPositionConverter;

        /// <summary>落下アニメーションサービス</summary>
        private readonly DropAnimationService _dropAnimation =
            new DropAnimationService();

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
        /// ビュー生成
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

            // セル間隔算出
            _cellSpacing = root.localScale.x / boardSize;

            // 座標変換サービス初期化
            _boardPositionConverter = new BoardPositionConverter(
                boardSize,
                root.position
            );

            // 最大駒数を元に容量確保
            int capacity = _boardSize * _boardSize * _boardSize;
            _pieces = new Dictionary<BoardIndex, PieceData>(capacity);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 駒登録
        /// </summary>
        public void SetPiece(
            in BoardIndex index,
            in PieceData piece)
        {
            _pieces[index] = piece;
        }

        /// <summary>
        /// 駒削除
        /// </summary>
        public void RemovePiece(in BoardIndex index)
        {
            if (_pieces.ContainsKey(index))
            {
                _pieces.Remove(index);
            }
            else
            {
                Debug.LogWarning($"RemovePiece: 駒が存在しません ({index.X}, {index.Y}, {index.Z})");
            }
        }

        /// <summary>
        /// 駒オブジェクト破棄
        /// </summary>
        public void DestroyPiece(in BoardIndex index)
        {
            if (_pieces.TryGetValue(index, out PieceData piece))
            {
                Object.Destroy(piece.Transform.gameObject);
            }
            else
            {
                Debug.LogWarning($"DestroyPiece: 駒が存在しません ({index.X}, {index.Y}, {index.Z})");
            }
        }

        /// <summary>
        /// 駒存在判定
        /// </summary>
        public bool HasPiece(BoardIndex index)
        {
            return _pieces.ContainsKey(index);
        }

        /// <summary>
        /// 駒取得
        /// </summary>
        public bool TryGetPiece(in BoardIndex index, out PieceData piece)
        {
            return _pieces.TryGetValue(index, out piece);
        }

        /// <summary>
        /// ワールド座標から列インデックスに変換
        /// </summary>
        public void WorldToColumn(
            in float worldX,
            in float worldZ,
            out int x,
            out int z)
        {
            _boardPositionConverter.WorldPositionToColumn(
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
        public async UniTask<PieceData> SpawnPieceAsync(
            int x,
            int y,
            int z,
            int player)
        {
            // 目標座標算出
            _boardPositionConverter.ColumnToWorldPosition(
                _cellSpacing,
                x,
                y,
                z,
                out float targetX,
                out float targetY,
                out float targetZ
            );

            // 初期位置生成
            float spawnY = _boardPositionConverter.GetSpawnWorldY(_cellSpacing);

            Vector3 startPosition =
                new Vector3(targetX, spawnY, targetZ);

            Vector3 endPosition =
                new Vector3(targetX, targetY, targetZ);

            // プレハブ選択
            GameObject prefab =
                player == 1
                ? _playerOnePrefab
                : _playerTwoPrefab;

            // インスタンス生成
            GameObject piece =
                Object.Instantiate(
                    prefab,
                    startPosition,
                    Quaternion.identity,
                    _root
                );

            // スケール調整
            float scaleFactor = 1f / (_boardSize + 0.5f);

            piece.transform.localScale =
                Vector3.one * scaleFactor;

            // 落下アニメーション
            await _dropAnimation.AnimateDropAsync(
                piece.transform,
                startPosition,
                endPosition
            );

            // PieceData を生成して返却
            return new PieceData(
                piece.transform,
                player
            );
        }

        /// <summary>
        /// 複数駒を同時に落下させる
        /// </summary>
        public async UniTask MovePiecesAsync(IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            // 移動計画作成
            List<MovePlanData> plans = CreateMovePlans(moves);

            if (plans.Count == 0)
            {
                return;
            }

            // アニメーション実行
            await ExecuteMoveAnimations(plans);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 移動計画を生成
        /// </summary>
        private List<MovePlanData> CreateMovePlans(IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            // スナップショット作成
            Dictionary<BoardIndex, PieceData> snapshot =
                new Dictionary<BoardIndex, PieceData>(_pieces);

            // 計画リスト生成
            List<MovePlanData> plans = new List<MovePlanData>(moves.Count);

            for (int i = 0; i < moves.Count; i++)
            {
                // 移動情報取得
                (BoardIndex from, BoardIndex to) move = moves[i];

                // スナップショットから駒を取得
                if (!snapshot.TryGetValue(move.from, out PieceData piece))
                {
                    continue;
                }

                // 開始位置取得
                Vector3 startPosition = piece.Transform.position;

                // 終了位置算出
                _boardPositionConverter.ColumnToWorldPosition(
                    _cellSpacing,
                    move.to.X,
                    move.to.Y,
                    move.to.Z,
                    out float targetX,
                    out float targetY,
                    out float targetZ
                );

                Vector3 endPosition =
                    new Vector3(targetX, targetY, targetZ);

                // 計画追加
                plans.Add(
                    new MovePlanData(
                        piece,
                        startPosition,
                        endPosition,
                        move.to
                    )
                );
            }

            return plans;
        }

        /// <summary>
        /// 駒落下アニメーション実行
        /// </summary>
        private async UniTask ExecuteMoveAnimations(List<MovePlanData> plans)
        {
            // タスクリスト生成
            List<UniTask> tasks =
                new List<UniTask>(plans.Count);

            for (int i = 0; i < plans.Count; i++)
            {
                MovePlanData plan = plans[i];

                // アニメーション登録
                tasks.Add(
                    _dropAnimation.AnimateDropAsync(
                        plan.Piece.Transform,
                        plan.Start,
                        plan.End
                    )
                );
            }

            // 全アニメーションの完了を待機
            await UniTask.WhenAll(tasks);
        }
    }
}