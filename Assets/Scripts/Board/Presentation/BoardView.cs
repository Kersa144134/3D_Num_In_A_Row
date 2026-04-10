// ======================================================
// BoardView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-10
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

        /// <summary>駒アニメーションビュー</summary>
        private readonly PieceAnimationView _pieceAnimationView;

        /// <summary>マテリアル適用サービス</summary>
        private readonly PieceMaterialService _materialService;

        /// <summary>列選択ビュー</summary>
        private readonly ColumnSelectView _columnSelectView;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面の親 Transform</summary>
        private readonly Transform _root;

        /// <summary>駒 Prefab</summary>
        private readonly GameObject _piecePrefab;

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        /// <summary>セル間隔</summary>
        private readonly float _cellSpacing;

        /// <summary>駒スケール倍率</summary>
        private readonly float _pieceScaleFactor;

        /// <summary>盤面の回転アニメーション時間（秒）</summary>
        private readonly float _rotationDuration;

        /// <summary>
        /// 生成駒辞書
        /// BoardIndex をキーとして駒データを管理
        /// </summary>
        private readonly Dictionary<BoardIndex, PieceData> _pieces;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在の列選択表示の可視状態</summary>
        public bool IsColumnSelectVisible => _columnSelectView.IsVisible;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// ビュー生成
        /// </summary>
        public BoardView(
            in Transform root,
            in int boardSize,
            in GameObject piecePrefab,
            in Material[] pieceMaterials,
            in GameObject columnSelectRoot,
            in GameObject deleteParticle,
            in float pieceScaleFactor,
            in float rotationDuration)
        {
            _root = root;
            _boardSize = boardSize;
            _piecePrefab = piecePrefab;
            _pieceScaleFactor = pieceScaleFactor;
            _rotationDuration = rotationDuration;

            // セル間隔算出
            _cellSpacing = root.localScale.x / _boardSize;

            // 最大駒数を元に容量確保
            int capacity = _boardSize * _boardSize * _boardSize;
            _pieces = new Dictionary<BoardIndex, PieceData>(capacity);

            // クラス初期化
            _boardPositionConverter = new BoardPositionConverter(
                boardSize,
                root.position
            );
            _materialService = new PieceMaterialService(pieceMaterials);

            // ColumnSelectRoot の null チェック
            if (columnSelectRoot == null)
            {
                Debug.LogError("[BoardView] ColumnSelectRoot の取得に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                UnityEngine.Application.Quit();
#endif
                return;
            }

            // 列選択ビュー初期化
            _columnSelectView =
                new ColumnSelectView(
                    columnSelectRoot.transform,
                    _boardPositionConverter,
                    _cellSpacing
                );

            // 駒アニメーションビュー初期化
            _pieceAnimationView =
                new PieceAnimationView(
                    new DropAnimationService(),
                    deleteParticle
                );
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 駒インスタンス生成と初期設定を行う
        /// </summary>
        private GameObject CreatePiece(in Vector3 startPosition, in int player)
        {
            // 指定された位置にプレハブから駒を生成
            GameObject piece =
                Object.Instantiate(
                    _piecePrefab,
                    startPosition,
                    Quaternion.identity,
                    _root
                );

            // Renderer を取得
            Renderer renderer = piece.GetComponent<Renderer>();

            // プレイヤーに応じたマテリアルを適用
            _materialService.Apply(
                renderer,
                player
            );

            // ボードサイズに応じたスケール係数を計算
            float scaleFactor = (1f / _boardSize) * _pieceScaleFactor;

            // 駒のスケールを設定
            piece.transform.localScale = Vector3.one * scaleFactor;

            return piece;
        }

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
        /// 指定した駒の Emission カラーを変更する
        /// </summary>
        public void SetPieceEmissionColor(in BoardIndex index, in Color emissionColor)
        {
            if (_pieces.TryGetValue(index, out PieceData piece) == false)
            {
                Debug.LogWarning($"SetPieceEmissionColor: 駒が存在しません ({index.X}, {index.Y}, {index.Z})");
                return;
            }

            // Renderer を取得する
            Renderer renderer = piece.Transform.GetComponent<Renderer>();

            if (renderer == null)
            {
                Debug.LogWarning("SetPieceEmissionColor: Rendererが見つかりません");
                return;
            }

            // マテリアルを取得
            Material material = renderer.material;

            // Emission を有効化
            material.EnableKeyword("_EMISSION");

            // 指定された Emission カラーを設定
            material.SetColor("_EmissionColor", emissionColor);
        }

        /// <summary>
        /// 駒オブジェクト破棄
        /// </summary>
        public void DestroyPiece(in BoardIndex index)
        {
            if (_pieces.TryGetValue(index, out PieceData piece))
            {
                // 削除演出を再生
                _pieceAnimationView.PlayDeleteEffect(piece.Transform.position);

                Object.Destroy(piece.Transform.gameObject);
            }
            else
            {
                Debug.LogWarning(
                    $"DestroyPiece: 駒が存在しません ({index.X}, {index.Y}, {index.Z})"
                );
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
        /// 列インデックスからワールド座標に変換
        /// </summary>
        public void ColumnToWorld(
            in int x,
            in int y,
            in int z,
            out float targetX,
            out float targetY,
            out float targetZ)
        {
            _boardPositionConverter.ColumnToWorldPosition(
                _cellSpacing,
                x,
                y,
                z,
                out targetX,
                out targetY,
                out targetZ
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
            // 指定インデックスから目標ワールド座標を算出
            ColumnToWorld(
                x,
                y,
                z,
                out float targetX,
                out float targetY,
                out float targetZ
            );

            // 駒の生成開始 Y 座標を取得
            float spawnY =
                _boardPositionConverter.GetSpawnWorldY(_cellSpacing);

            // 落下開始位置を生成
            Vector3 startPosition =
                new Vector3(targetX, spawnY, targetZ);

            // 最終到達位置を生成
            Vector3 endPosition =
                new Vector3(targetX, targetY, targetZ);

            // 駒インスタンスを生成
            GameObject piece = CreatePiece(startPosition, player);

            // 落下アニメーションを再生
            await _pieceAnimationView.PlayDropAsync(
                piece.transform,
                startPosition,
                endPosition
            );

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
            // 移動計画リストを生成
            List<MovePlanData> plans = CreateMovePlans(moves);

            // 移動対象が存在しない場合は処理なし
            if (plans.Count == 0)
            {
                return;
            }

            // 移動アニメーションを実行
            await ExecuteMoveAnimations(plans);
        }

        /// <summary>
        /// 指定オブジェクトを軸と方向に応じて回転させる
        /// </summary>
        public async UniTask RotateAsync(
            Transform target,
            RotationAxis axis,
            RotationDirection direction)
        {
            if (target == null)
            {
                return;
            }

            // 回転角度を方向に応じて決定（±90度）
            float angle =
                direction == RotationDirection.Positive
                ? -90f
                : 90f;

            // 経過時間を初期化
            float elapsed = 0f;

            // 開始時の回転状態を保持
            Quaternion startRotation =
                target.rotation;

            // 終了回転を初期値として開始回転を設定
            Quaternion endRotation =
                startRotation;

            // X軸回転の場合の終了回転を算出
            if (axis == RotationAxis.X)
            {
                endRotation =
                    startRotation * Quaternion.Euler(angle, 0f, 0f);
            }
            // Z軸回転の場合の終了回転を算出
            else if (axis == RotationAxis.Z)
            {
                endRotation =
                    startRotation * Quaternion.Euler(0f, 0f, angle);
            }

            // 指定時間まで補間処理を行う
            while (elapsed < _rotationDuration)
            {
                // 経過時間を更新
                elapsed += Time.deltaTime;

                // 補間係数を算出（0 ～ 1）
                float t = elapsed / _rotationDuration;

                // 回転を線形補間で更新
                target.rotation =
                    Quaternion.Lerp(
                        startRotation,
                        endRotation,
                        t
                    );

                // 1フレーム待機
                await UniTask.Yield();
            }

            target.rotation = endRotation;
        }

        /// <summary>
        /// 列選択表示の表示状態を切り替える
        /// </summary>
        public void SetSelectVisible(in bool isVisible)
        {
            _columnSelectView.SetVisible(isVisible);
        }

        /// <summary>
        /// ヒットしたワールド座標から列位置を算出し、選択表示の位置を更新する
        /// </summary>
        public void UpdateColumnSelect(in Vector3 hitPos)
        {
            _columnSelectView.UpdatePosition(hitPos);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 駒の移動計画を生成
        /// </summary>
        private List<MovePlanData> CreateMovePlans(
            IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            // スナップショットを作成
            Dictionary<BoardIndex, PieceData> snapshot =
                new Dictionary<BoardIndex, PieceData>(_pieces);

            // 移動計画リスト
            List<MovePlanData> plans = new List<MovePlanData>(moves.Count);

            for (int i = 0; i < moves.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = moves[i];

                PieceData piece;

                // スナップショットから駒取得
                if (!snapshot.TryGetValue(move.from, out piece))
                {
                    Debug.LogWarning(
                        $"CreateMovePlans: スナップショットに駒が存在しません" +
                        $"{move.from.X}, {move.from.Y}, {move.from.Z}"
                    );
                    continue;
                }

                // 現在の開始位置を取得
                Vector3 startPosition =
                    piece.Transform.position;

                // 移動目標座標
                float targetX;
                float targetY;
                float targetZ;

                // 移動先インデックスからワールド座標を算出
                ColumnToWorld(
                    move.to.X,
                    move.to.Y,
                    move.to.Z,
                    out targetX,
                    out targetY,
                    out targetZ
                );

                // 終了位置を生成
                Vector3 endPosition =
                    new Vector3(targetX, targetY, targetZ);

                // 移動計画リストに追加
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
            // ビュー用移動計画リストを生成
            List<PieceAnimationView.MovePlanData> viewPlans =
                new List<PieceAnimationView.MovePlanData>(plans.Count);

            for (int i = 0; i < plans.Count; i++)
            {
                MovePlanData plan = plans[i];

                viewPlans.Add(
                    new PieceAnimationView.MovePlanData(
                        plan.Piece.Transform,
                        plan.Start,
                        plan.End
                    )
                );
            }

            // アニメーション実行
            await _pieceAnimationView.PlayMovesAsync(viewPlans);
        }
    }
}