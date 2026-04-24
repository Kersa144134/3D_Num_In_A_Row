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
        private readonly PieceMaterialMapper _materialMapper;

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
        // 定数
        // ======================================================

        /// <summary>
        /// Albedo カラーのプロパティ名
        /// </summary>
        private const string ALBEDO_COLOR_PROPERTY = "_Albedo_Color";

        /// <summary>
        /// HDR 強度
        /// </summary>
        private const float ALBEDO_INTENSITY = 3.0f;
        
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
            _materialMapper = new PieceMaterialMapper(pieceMaterials);

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
                    new PieceDropAnimator(),
                    deleteParticle
                );
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // 駒情報
        // --------------------------------------------------
        /// <summary>
        /// 駒インスタンス生成と初期設定を行う
        /// </summary>
        private GameObject CreatePieceObject(in Vector3 startPosition, in int player)
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
            _materialMapper.Apply(
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
        /// 駒オブジェクト破棄
        /// </summary>
        public void DestroyPieceObject(in BoardIndex index)
        {
            if (_pieces.TryGetValue(index, out PieceData piece))
            {
                // 削除演出を再生
                _pieceAnimationView.PlayDeleteEffect(piece.Transform.position);

                Object.Destroy(piece.Transform.gameObject);
            }
            else
            {
                Debug.LogWarning($"DestroyPiece: 駒が存在しません ({index.X}, {index.Y}, {index.Z})");
            }
        }

        /// <summary>
        /// 駒取得
        /// </summary>
        public bool TryGetPiece(in BoardIndex index, out PieceData piece)
        {
            return _pieces.TryGetValue(index, out piece);
        }

        // --------------------------------------------------
        // 座標情報
        // --------------------------------------------------
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

        // --------------------------------------------------
        // アニメーション
        // --------------------------------------------------
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
            GameObject piece = CreatePieceObject(startPosition, player);

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

            // --------------------------------------------------
            // 回転パラメータ初期化
            // --------------------------------------------------
            // 回転角度（±90度）
            float angle =
                direction == RotationDirection.Positive
                ? -90f
                : 90f;

            // 経過時間
            float elapsed = 0f;

            // 開始回転
            Quaternion startRotation =
                target.rotation;

            // 終了回転
            Quaternion endRotation =
                startRotation;

            // --------------------------------------------------
            // 終了回転算出
            // --------------------------------------------------
            // X軸回転
            if (axis == RotationAxis.X)
            {
                endRotation =
                    startRotation * Quaternion.Euler(angle, 0f, 0f);
            }
            // Z軸回転
            else if (axis == RotationAxis.Z)
            {
                endRotation =
                    startRotation * Quaternion.Euler(0f, 0f, angle);
            }

            try
            {
                // --------------------------------------------------
                // 補間ループ
                // --------------------------------------------------
                while (elapsed < _rotationDuration)
                {
                    if (target == null)
                    {
                        return;
                    }

                    // 経過時間加算
                    elapsed += Time.deltaTime;

                    // 補間係数
                    float t =
                        Mathf.Clamp01(elapsed / _rotationDuration);

                    // 回転補間適用
                    target.rotation =
                        Quaternion.Lerp(
                            startRotation,
                            endRotation,
                            t
                        );

                    // Update タイミングで1フレーム待機
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
            }
            finally
            {
                if (target != null)
                {
                    target.rotation = endRotation;
                }
            }
        }

        /// <summary>
        /// 指定した駒の Emission カラーを変更する
        /// </summary>
        public void SetPieceEmissionColor(in BoardIndex index)
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

            // 現在の Albedo カラーを取得
            Color currentColor = material.GetColor(ALBEDO_COLOR_PROPERTY);

            // Intensity を更新
            Color hdrColor = currentColor * ALBEDO_INTENSITY;

            // Albedo カラーに適用
            material.SetColor(ALBEDO_COLOR_PROPERTY, hdrColor);
        }

        /// <summary>
        /// 列選択表示の表示状態を切り替える
        /// </summary>
        public void SeteColumnSelectVisible(in bool isVisible)
        {
            _columnSelectView.SetVisible(isVisible);
        }

        /// <summary>
        /// ヒットしたワールド座標から列位置を算出し、選択表示の位置を更新する
        /// </summary>
        public void UpdateColumnSelectPosition(in Vector3 hitPos)
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