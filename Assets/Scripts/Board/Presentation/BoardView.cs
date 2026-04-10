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

        /// <summary>マテリアル適用サービス</summary>
        private readonly PieceMaterialService _materialService;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面の親 Transform</summary>
        private readonly Transform _root;

        /// <summary>駒 Prefab</summary>
        private readonly GameObject _piecePrefab;

        /// <summary>列選択表示のルート Transform</summary>
        private readonly Transform _columnSelectRoot;

        /// <summary>列選択表示に使用する Renderer 配列</summary>
        private Renderer[] _columnSelectRenderers;

        /// <summary>現在の列選択表示の可視状態</summary>
        private bool _isColumnSelectVisible;

        /// <summary>列選択位置の計算に使用する一時キャッシュ座標</summary>
        private Vector3 _cachedSelectPos;

        /// <summary>駒削除時に再生するパーティクル</summary>
        private readonly GameObject _deleteParticle;

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
        public bool IsColumnSelectVisible => _isColumnSelectVisible;

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
            _columnSelectRoot = columnSelectRoot.transform;
            _deleteParticle = deleteParticle;
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

            // 子階層を含めてRendererをすべて取得
            if (_columnSelectRoot == null)
            {
                Debug.LogError("[BoardView] ColumnSelectRoot の取得に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            _columnSelectRenderers = _columnSelectRoot.GetComponentsInChildren<Renderer>(true);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 駒インスタンス生成と初期設定を行う
        /// </summary>
        /// <param name="startPosition">生成位置</param>
        /// <param name="player">プレイヤー情報</param>
        /// <returns>生成された駒</returns>
        private GameObject CreatePiece(in Vector3 startPosition, in int player)
        {
            // プレハブから駒を生成する
            GameObject piece =
                Object.Instantiate(
                    _piecePrefab,
                    startPosition,
                    Quaternion.identity,
                    _root
                );

            // 生成した駒のRendererを取得する
            Renderer renderer =
                piece.GetComponent<Renderer>();

            // プレイヤーに応じたマテリアルを適用する
            _materialService.Apply(
                renderer,
                player
            );

            // ボードサイズに応じたスケール係数を計算する
            float scaleFactor =
                (1f / _boardSize) * _pieceScaleFactor;

            // 駒のスケールを設定する
            piece.transform.localScale =
                Vector3.one * scaleFactor;

            // 生成した駒を返却する
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
        /// <param name="index">対象の盤面インデックス</param>
        /// <param name="emissionColor">設定する発光色</param>
        public void SetPieceEmissionColor(in BoardIndex index, in Color emissionColor)
        {
            // 駒データ取得
            if (_pieces.TryGetValue(index, out PieceData piece) == false)
            {
                // 対象が存在しない場合は警告
                Debug.LogWarning($"SetPieceEmissionColor: 駒が存在しません ({index.X}, {index.Y}, {index.Z})");
                return;
            }

            // Renderer 取得
            Renderer renderer = piece.Transform.GetComponent<Renderer>();

            // Renderer が存在しない場合は処理なし
            if (renderer == null)
            {
                Debug.LogWarning("SetPieceEmissionColor: Rendererが見つかりません");
                return;
            }

            // マテリアル取得
            Material material = renderer.material;

            // Emission有効化
            material.EnableKeyword("_EMISSION");

            // Emissionカラー設定
            material.SetColor("_EmissionColor", emissionColor);
        }

        /// <summary>
        /// 駒オブジェクト破棄
        /// </summary>
        /// <param name="index">ボード上のインデックス</param>
        public void DestroyPiece(in BoardIndex index)
        {
            // 指定インデックスの駒を取得する
            if (_pieces.TryGetValue(index, out PieceData piece))
            {
                // 駒のワールド座標を取得する
                Vector3 position =
                    piece.Transform.position;

                // 削除時パーティクルを再生する
                PlayDeleteParticle(position);

                // 駒オブジェクトを破棄する
                Object.Destroy(
                    piece.Transform.gameObject
                );

                // Dictionaryからも削除する
                _pieces.Remove(index);
            }
            else
            {
                // 存在しない場合は警告ログを出す
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
            // 目標座標算出
            ColumnToWorld(
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

            GameObject piece = CreatePiece(startPosition, player);

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

        /// <summary>
        /// 指定オブジェクトを軸と方向に応じて回転させる
        /// </summary>
        /// <param name="target">回転対象 Transform</param>
        /// <param name="axis">回転軸</param>
        /// <param name="direction">回転方向</param>
        public async UniTask RotateAsync(
            Transform target,
            RotationAxis axis,
            RotationDirection direction)
        {
            if (target == null)
            {
                return;
            }

            // 回転角度（±90）
            float angle = direction == RotationDirection.Positive
                ? -90f
                : 90f;

            // 経過時間
            float elapsed = 0f;

            // 開始回転
            Quaternion startRotation = target.rotation;

            // 終了回転
            Quaternion endRotation = startRotation;

            // X軸回転
            if (axis == RotationAxis.X)
            {
                endRotation = startRotation * Quaternion.Euler(angle, 0f, 0f);
            }

            // Z軸回転
            else if (axis == RotationAxis.Z)
            {
                endRotation = startRotation * Quaternion.Euler(0f, 0f, angle);
            }

            // 補間処理
            while (elapsed < _rotationDuration)
            {
                elapsed += Time.deltaTime;

                float t = elapsed / _rotationDuration;

                target.rotation = Quaternion.Lerp(
                    startRotation,
                    endRotation,
                    t
                );

                await UniTask.Yield();
            }

            target.rotation = endRotation;
        }

        /// <summary>
        /// 列選択表示の表示状態を切り替える
        /// </summary>
        /// <param name="isVisible">表示は true、非表示は false</param>
        public void SetSelectVisible(in bool isVisible)
        {
            // 状態が同じ場合は処理なし
            if (_isColumnSelectVisible == isVisible)
            {
                return;
            }

            // 表示状態を更新
            _isColumnSelectVisible = isVisible;

            if (_columnSelectRenderers == null)
            {
                return;
            }

            for (int i = 0; i < _columnSelectRenderers.Length; i++)
            {
                Renderer renderer = _columnSelectRenderers[i];

                if (renderer == null)
                {
                    continue;
                }

                // 描画切り替え
                renderer.enabled = isVisible;
            }
        }

        /// <summary>
        /// ヒットしたワールド座標から列位置を算出し、選択表示の位置を更新する
        /// </summary>
        /// <param name="hitPos">レイキャストで取得したヒット座標</param>
        public void UpdateColumnSelect(in Vector3 hitPos)
        {
            // --------------------------------------------------
            // ワールド座標 → 列インデックス変換
            // --------------------------------------------------
            WorldToColumn(
                hitPos.x,
                hitPos.z,
                out int x,
                out int z
            );

            // --------------------------------------------------
            // 列インデックス → ワールド座標変換
            // --------------------------------------------------
            // キャッシュ更新
            _cachedSelectPos = hitPos;

            ColumnToWorld(
                x,
                0,
                z,
                out _cachedSelectPos.x,
                out _cachedSelectPos.y,
                out _cachedSelectPos.z
            );

            // --------------------------------------------------
            // Transform 反映
            // --------------------------------------------------
            if (_columnSelectRoot == null)
            {
                return;
            }

            // Y 座標はヒット位置を維持する
            _columnSelectRoot.position = new Vector3(
                _cachedSelectPos.x,
                hitPos.y,
                _cachedSelectPos.z
            );
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
            // スナップショット作成
            Dictionary<BoardIndex, PieceData> snapshot =
                new Dictionary<BoardIndex, PieceData>(_pieces);

            // 移動計画リスト生成
            List<MovePlanData> plans = new List<MovePlanData>(moves.Count);

            for (int i = 0; i < moves.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = moves[i];

                // スナップショットから駒を取得
                PieceData piece;
                if (!snapshot.TryGetValue(move.from, out piece))
                {
                    Debug.LogWarning(
                        $"CreateMovePlans: スナップショットに駒が存在しません" +
                        $"{move.from.X}, {move.from.Y}, {move.from.Z}"
                    );
                    continue;
                }

                // 開始位置取得
                Vector3 startPosition = piece.Transform.position;

                // 終了位置算出
                float targetX;
                float targetY;
                float targetZ;

                ColumnToWorld(
                    move.to.X,
                    move.to.Y,
                    move.to.Z,
                    out targetX,
                    out targetY,
                    out targetZ
                );

                Vector3 endPosition = new Vector3(targetX, targetY, targetZ);

                // 移動計画追加
                plans.Add(new MovePlanData(
                    piece,
                    startPosition,
                    endPosition,
                    move.to
                ));
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

        /// <summary>
        /// 駒削除時パーティクルを再生する
        /// </summary>
        /// <param name="position">再生位置</param>
        private void PlayDeleteParticle(Vector3 position)
        {
            // パーティクル未設定なら何もしない
            if (_deleteParticle == null)
            {
                return;
            }

            // パーティクルインスタンス生成
            GameObject particle =
                Object.Instantiate(
                    _deleteParticle,
                    position,
                    Quaternion.identity
                );

            // ParticleSystem 取得
            ParticleSystem ps =
                particle.GetComponent<ParticleSystem>();

            // ParticleSystem が存在する場合のみ再生
            if (ps != null)
            {
                // 再生
                ps.Play();

                // 再生終了後に破棄
                Object.Destroy(
                    particle,
                    ps.main.duration + ps.main.startLifetime.constantMax
                );
            }
            else
            {
                // ParticleSystemが無い場合は即破棄
                Object.Destroy(particle);
            }
        }
    }
}