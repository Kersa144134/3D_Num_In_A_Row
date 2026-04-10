// ======================================================
// BoardPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-07
// 概要     : 3D 目並べゲームの盤面を制御するクラス
// ======================================================

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using BoardSystem.Domain;
using InputSystem;
using PhaseSystem.Domain;
using SceneSystem.Domain;

namespace BoardSystem.Presentation
{
    /// <summary>
    /// 目並べプレゼンター
    /// </summary>
    public sealed class BoardPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("盤面")]
        /// <summary>
        /// 盤面サイズ
        /// </summary>
        [SerializeField, Min(3)]
        private int _boardSize;

        /// <summary>
        /// 勝利条件の連続駒数
        /// </summary>
        [SerializeField, Min(3)]
        private int _connectCount;

        /// <summary>
        /// 盤面の列選択表示
        /// </summary>
        [SerializeField]
        private GameObject _columnSelect;

        [Header("駒")]
        /// <summary>
        /// 駒の Prefab
        /// </summary>
        [SerializeField]
        private GameObject _piecePrefab;

        /// <summary>
        /// 駒のマテリアル配列
        /// </summary>
        [SerializeField]
        private Material[] _pieceMaterials;

        /// <summary>
        /// 駒削除時に再生するパーティクル
        /// </summary>
        [SerializeField]
        private GameObject _deleteParticle;

        /// <summary>
        /// 駒のスケール倍率
        /// </summary>
        [SerializeField, Range(0.5f, 1.0f)]
        private float _pieceScaleFactor;

        [Header("レイヤー")]
        /// <summary>
        /// Ray判定用レイヤー
        /// </summary>
        [SerializeField]
        private LayerMask _raycastLayerMask;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>モデル</summary>
        private BoardModel _model;

        /// <summary>ビュー</summary>
        private BoardView _view;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>カメラ</summary>
        private Camera _camera; 
        
        /// <summary>現在のプレイヤーID</summary>
        private int _currentPlayer;

        /// <summary>盤面のクリック判定用 Collider</summary>
        private Collider _boardCollider;

        /// <summary>盤面の列選択用 Renderer</summary>
        private Renderer[] _columnSelectRenderers;

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLocked = true;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>入力購読管理</summary>
        private CompositeDisposable _inputDisposables;

        /// <summary>常駐購読管理</summary>
        private readonly CompositeDisposable _otherDisposables =
            new CompositeDisposable();

        /// <summary>ライン成立通知用 Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete = new Subject<LineCompleteEvent>();

        /// <summary>ライン成立ストリーム</summary>
        public IObservable<LineCompleteEvent> OnLineComplete => _onLineComplete;

        /// <summary>フェーズ終了通知用 Subject</summary>
        private readonly Subject<Unit> _onPhaseEnd = new Subject<Unit>();

        /// <summary>フェーズ終了ストリーム</summary>
        public IObservable<Unit> OnPhaseEnd => _onPhaseEnd;

        /// <summary>フェーズ購読</summary>
        private IDisposable _phaseSubscription;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>1P ID</summary>
        private const int PLAYER_ONE = 1;

        /// <summary>2P ID</summary>
        private const int PLAYER_TWO = 2;

        /// <summary>
        /// ライン成立後、削除処理を開始するまでの待機時間（ミリ秒）
        /// </summary>
        private const int LINE_DELETE_DELAY_MS = 600;

        /// <summary>
        /// 各駒を削除する際のインターバル時間（ミリ秒）
        /// </summary>
        private const int PIECE_DELETE_DELAY_MS = 200;

        /// <summary>
        /// 駒削除後、落下処理を開始するまでの待機時間（ミリ秒）
        /// </summary>
        private const int PIECE_DROP_DELAY_MS = 500;

        /// <summary>
        /// 駒配置後、フェーズ切り替えを開始するまでの待機時間（ミリ秒）
        /// </summary>
        private const int PHASE_CHANGE_DELAY_MS = 500;

        /// <summary>
        /// 盤面の回転アニメーション時間（秒）
        /// </summary>
        private const float ROTATION_DURATION = 0.5f;

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void OnEnter()
        {
            _model = new BoardModel(_boardSize, _connectCount);
            _view = new BoardView(
                transform,
                _boardSize,
                _piecePrefab,
                _pieceMaterials,
                _columnSelect,
                _deleteParticle,
                _pieceScaleFactor,
                ROTATION_DURATION
            );

            // シーン内のメインカメラを取得
            _camera = Camera.main;

            // 子階層のコライダーを取得
            _boardCollider = GetComponentInChildren<Collider>(true);

            if (_camera == null || _boardCollider == null || _columnSelect == null)
            {
                Debug.LogError("[BoardPresenter] Camera または BoardCollider の取得に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            // 回転対象外のため親から分離
            _boardCollider.transform.SetParent(null);

            // --------------------------------------------------
            // 常駐購読
            // --------------------------------------------------
            // ライン成立
            _model.OnLineComplete
                .Subscribe(async lineEvent =>
                {
                    _onLineComplete.OnNext(lineEvent);

                    await HandleLineDeleteAsync(lineEvent);
                })
                .AddTo(_otherDisposables);
        }

        public void OnUpdate(in float unscaledDeltaTime, in float elapsedTime)
        {
            // 入力ロック中は列選択表示を非表示
            if (_isInputLocked)
            {
                _view.SetSelectVisible(false);
                return;
            }

            // Ray 生成
            Ray ray = _camera.ScreenPointToRay(InputManager.Instance.Pointer);

            bool isHit = Physics.Raycast(
                ray,
                out RaycastHit hit,
                Mathf.Infinity,
                _raycastLayerMask
            );

            // ヒットしなかった場合は列選択表示を非表示
            if (isHit == false || hit.collider != _boardCollider)
            {
                _view.SetSelectVisible(false);
                return;
            }

            _view.SetSelectVisible(true);

            // 列選択表示の座標更新
            _view.UpdateColumnSelect(hit.point);
        }

        public void OnExit()
        {
            // 入力購読解除
            _inputDisposables?.Dispose();

            // 常駐購読解除
            _otherDisposables.Dispose();

            _model.Dispose();
        }

        public void OnPhaseEnter(in PhaseType phase)
        {
            if (!(phase == PhaseType.Play_1 || phase == PhaseType.Play_2) ||
                _inputDisposables != null)
            {
                return;
            }

            // --------------------------------------------------
            // 入力購読
            // --------------------------------------------------
            _inputDisposables = new CompositeDisposable();

            // ボタン A 押下
            InputManager.Instance.ButtonA.OnDown
                .Subscribe(_ => HandleDropColumn())
                .AddTo(_inputDisposables);

            // ボタン B 押下
            InputManager.Instance.ButtonB.OnDown
                .Subscribe(_ => HandleDropColumn())
                .AddTo(_inputDisposables);

            // ボタン X 押下
            InputManager.Instance.ButtonX.OnDown
                .Subscribe(_ =>
                {
                    HandleRotateAsync(RotationAxis.X, RotationDirection.Positive).Forget();
                })
                .AddTo(_inputDisposables);

            // ボタン Y 押下
            InputManager.Instance.ButtonY.OnDown
                .Subscribe(_ =>
                {
                    HandleRotateAsync(RotationAxis.X, RotationDirection.Negative).Forget();
                })
                .AddTo(_inputDisposables);
        }

        public void OnPhaseExit(in PhaseType phase)
        {
            if (!(phase == PhaseType.Play_1 || phase == PhaseType.Play_2))
            {
                return;
            }

            // 入力のみ購読解除
            _inputDisposables?.Dispose();
            _inputDisposables = null;

            // 列選択表示を非表示にする
            _view.SetSelectVisible(false);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ変更ストリームを購読する
        /// </summary>
        /// <param name="stream">フェーズストリーム</param>
        public void BindPhaseStream(IObservable<PhaseType> stream)
        {
            // 多重購読防止
            _phaseSubscription?.Dispose();

            _phaseSubscription = stream
                .Subscribe(phase =>
                {
                    if (phase == PhaseType.Play_1)
                    {
                        _currentPlayer = PLAYER_ONE;

                        // 入力ロック解除
                        _isInputLocked = false;
                    }
                    else if (phase == PhaseType.Play_2)
                    {
                        _currentPlayer = PLAYER_TWO;

                        // 入力ロック解除
                        _isInputLocked = false;
                    }
                    else
                    {
                        // 入力ロック
                        _isInputLocked = true;
                    }
                });
        }

        /// <summary>
        /// フェーズ変更ストリームの購読を解除する
        /// </summary>
        public void UnbindPhaseStream()
        {
            _phaseSubscription?.Dispose();
            _phaseSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// クリック座標から駒落下処理を開始
        /// </summary>
        private void HandleDropColumn()
        {
            // 入力ロック、または列未選択状態なら処理なし
            if (_isInputLocked || !_view.IsColumnSelectVisible)
            {
                return;
            }

            // 列選択表示座標を列インデックスに変換
            _view.WorldToColumn(
                _columnSelect.transform.position.x,
                _columnSelect.transform.position.z,
                out int x,
                out int z);

            // 駒落下処理
            HandleDropAsync(x, z).Forget();
        }

        /// <summary>
        /// 駒落下処理
        /// </summary>
        private async UniTask HandleDropAsync(int x, int z)
        {
            // 多重実行防止
            if (_isInputLocked)
            {
                return;
            }

            // 入力ロック
            _isInputLocked = true;

            // 配置可能判定
            int y = _model.CalculatePlace(x, z);

            if (y < 0)
            {
                _isInputLocked = false;
                return;
            }

            // 駒生成
            PieceData piece =
                await _view.SpawnPieceAsync(
                    x,
                    y,
                    z,
                    _currentPlayer
                );

            // インデックス生成
            BoardIndex index = new BoardIndex(x, y, z);

            // ビューに駒情報登録
            _view.SetPiece(index, piece);

            // モデルの盤面更新
            _model.ApplyPlace(index, _currentPlayer);

            // ライン成立チェック
            await CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// 盤面回転処理
        /// </summary>
        /// <param name="axis">回転軸</param>
        /// <param name="direction">回転方向</param>
        private async UniTask HandleRotateAsync(
            RotationAxis axis,
            RotationDirection direction)
        {
            // 多重実行防止
            if (_isInputLocked)
            {
                return;
            }

            // 入力ロック
            _isInputLocked = true;

            // モデル回転情報取得
            IReadOnlyList<(BoardIndex from, BoardIndex to)> moves =
                _model.Rotate90(axis, direction);

            // ビュー辞書更新
            ApplyViewMoves(moves);

            // ビュー回転アニメーション
            await _view.RotateAsync(
                transform,
                axis,
                direction
            );

            // 全列再配置
            List<(int x, int z)> allColumns = new List<(int, int)>();
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    allColumns.Add((x, z));
                }
            }
            await PiecesRepositionAsync(allColumns);

            // ラインチェック
            await CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// ライン成立時に駒を削除し再配置を行う
        /// </summary>
        private async UniTask HandleLineDeleteAsync(LineCompleteEvent lineEvent)
        {
            // 削除対象駒
            List<BoardIndex> deleteTargets = new List<BoardIndex>();

            // 再配置対象列
            HashSet<(int x, int z)> pieceSet = new HashSet<(int, int)>();

            // --------------------------------------------------
            // Emission 処理
            // --------------------------------------------------
            for (int i = 0; i < lineEvent.LinePositions.Length; i++)
            {
                IReadOnlyList<BoardIndex> line = lineEvent.LinePositions[i];

                for (int j = 0; j < line.Count; j++)
                {
                    BoardIndex index = line[j];

                    if (_view.HasPiece(index) == false)
                    {
                        Debug.LogWarning($"BoardPresenter: 駒が存在しません ({index.X}, {index.Y}, {index.Z})");
                        continue;
                    }

                    _view.SetPieceEmissionColor(index, Color.white);

                    // 削除対象追加
                    deleteTargets.Add(index);

                    // 再配置対象追加
                    pieceSet.Add((index.X, index.Z));

                    // 演出待機
                    await UniTask.Delay(PIECE_DELETE_DELAY_MS);
                }
            }

            // 演出待機
            await UniTask.Delay(LINE_DELETE_DELAY_MS);

            // --------------------------------------------------
            // 削除処理
            // --------------------------------------------------
            for (int i = 0; i < deleteTargets.Count; i++)
            {
                BoardIndex index = deleteTargets[i];

                _view.DestroyPiece(index);
                _view.RemovePiece(index);
                _model.ClearCell(index);
            }

            // --------------------------------------------------
            // 再配置処理
            // --------------------------------------------------
            List<(int x, int z)> repositionPieces = new List<(int, int)>(pieceSet);

            // 演出待機
            await UniTask.Delay(PIECE_DROP_DELAY_MS);

            await PiecesRepositionAsync(repositionPieces);

            // --------------------------------------------------
            // ライン成立チェック
            // --------------------------------------------------
            await CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// 駒再配置処理
        /// </summary>
        private async UniTask PiecesRepositionAsync(IReadOnlyList<(int x, int z)> pieces)
        {
            List<(BoardIndex from, BoardIndex to)> allMoves =
                new List<(BoardIndex, BoardIndex)>();

            // 再配置対象作成
            for (int i = 0; i < pieces.Count; i++)
            {
                (int x, int z) column = pieces[i];

                IReadOnlyList<(BoardIndex from, BoardIndex to)> moves =
                    _model.CalculateReposition(column.x, column.z);

                allMoves.AddRange(moves);
            }

            if (allMoves.Count > 0)
            {
                // ビューアニメーション実行
                await _view.MovePiecesAsync(allMoves);

                // ビュー辞書更新
                ApplyViewMoves(allMoves);

                // モデル盤面更新
                for (int i = 0; i < pieces.Count; i++)
                {
                    (int x, int z) piece = pieces[i];

                    _model.ApplyReposition(piece.x, piece.z);
                }
            }
        }

        /// <summary>
        /// ライン成立判定時の共通処理
        /// </summary>
        private async UniTask CheckLine(bool isLine)
        {
            // ラインが成立している場合は処理なし
            if (isLine)
            {
                return;
            }

            await UniTask.Delay(PHASE_CHANGE_DELAY_MS);

            // フェーズ終了通知
            _onPhaseEnd.OnNext(Unit.Default);
        }

        /// <summary>
        /// ビューの駒辞書を移動情報に基づいて更新
        /// </summary>
        private void ApplyViewMoves(in IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            // スナップショット作成
            Dictionary<BoardIndex, PieceData> snapshot = new Dictionary<BoardIndex, PieceData>();

            for (int i = 0; i < moves.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = moves[i];

                PieceData piece;
                if (_view.TryGetPiece(move.from, out piece))
                {
                    snapshot[move.from] = piece;
                }
            }

            // 全 from の駒を削除
            foreach (BoardIndex fromIndex in snapshot.Keys)
            {
                _view.RemovePiece(fromIndex);
            }

            // すべての to に駒をセット
            for (int i = 0; i < moves.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = moves[i];

                PieceData piece;
                if (snapshot.TryGetValue(move.from, out piece))
                {
                    _view.SetPiece(move.to, piece);
                }
                else
                {
                    Debug.LogError(
                        $"ApplyViewMoves: スナップショットに存在しない駒" +
                        $"{move.from.X}, {move.from.Y}, {move.from.Z}"
                    );
                }
            }
        }
    }
}