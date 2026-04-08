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
using PhaseSystem.Data;
using SceneSystem.Data;

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
        /// 1P駒Prefab
        /// </summary>
        [SerializeField]
        private GameObject _playerOnePrefab;

        /// <summary>
        /// 2P駒Prefab
        /// </summary>
        [SerializeField]
        private GameObject _playerTwoPrefab;

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

        /// <summary>現在のプレイヤーID</summary>
        private int _currentPlayer;

        /// <summary>盤面のクリック判定用 Collider</summary>
        private Collider _boardCollider;

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLocked;

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
        private const int LINE_DELETE_DELAY_MS = 500;

        /// <summary>
        /// 各駒を削除する際のインターバル時間（ミリ秒）
        /// </summary>
        private const int PIECE_DELETE_DELAY_MS = 100;

        /// <summary>
        /// 駒削除後、落下処理を開始するまでの待機時間（ミリ秒）
        /// </summary>
        private const int PIECE_DROP_DELAY_MS = 500;
        
        // ======================================================
        // IUpdatable イベント
        // ======================================================

        /// <summary>
        /// 初期化処理
        /// </summary>
        public void OnEnter()
        {
            _model = new BoardModel(_boardSize, _connectCount);
            _view = new BoardView(
                transform,
                _boardSize,
                _playerOnePrefab,
                _playerTwoPrefab
            );

            _boardCollider = GetComponentInChildren<Collider>(true);
            if (_boardCollider == null)
            {
                throw new InvalidOperationException("BoardCollider が見つかりません");
            }

            // 初期プレイヤー設定
            _currentPlayer = PLAYER_ONE;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void OnUpdate(in float unscaledDeltaTime, in float elapsedTime)
        {
            
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void OnExit()
        {
            // 入力購読解除
            _inputDisposables?.Dispose();

            // 常駐購読解除
            _otherDisposables.Dispose();

            _model.Dispose();
        }

        /// <summary>
        /// フェーズ開始時処理
        /// </summary>
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

            // ボタンA 押下
            InputManager.Instance.ButtonA.OnDown
                .Subscribe(_ => HandleDropColumn())
                .AddTo(_inputDisposables);

            // ボタンB 押下
            InputManager.Instance.ButtonB.OnDown
                .Subscribe(_ => HandleDropColumn())
                .AddTo(_inputDisposables);

            // --------------------------------------------------
            // 常駐購読
            // --------------------------------------------------
            if (_otherDisposables.Count == 0)
            {
                _model.OnLineComplete
                    .Subscribe(async lineEvent =>
                    {
                        _onLineComplete.OnNext(lineEvent);

                        // ライン削除
                        await HandleLineDeleteAsync(lineEvent);
                    })
                    .AddTo(_otherDisposables);
            }
        }

        /// <summary>
        /// フェーズ終了時処理
        /// </summary>
        public void OnPhaseExit(in PhaseType phase)
        {
            if (!(phase == PhaseType.Play_1 || phase == PhaseType.Play_2))
            {
                return;
            }

            // 入力のみ解除
            _inputDisposables?.Dispose();
            _inputDisposables = null;
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
                    }
                    else if (phase == PhaseType.Play_2)
                    {
                        _currentPlayer = PLAYER_TWO;
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
            // 入力ロック
            if (_isInputLocked)
            {
                return;
            }
            
            // カメラから Ray生成
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.Pointer);

            // Ray 判定
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _raycastLayerMask);

            if (!isHit || hit.collider != _boardCollider)
            {
                return;
            }

            // ヒット座標を列インデックスに変換
            Vector3 hitPos = hit.point;
            _view.WorldToColumn(hitPos.x, hitPos.z, out int x, out int z);

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

            // --------------------------------------------------
            // 配置可能判定
            // --------------------------------------------------
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
            CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// ライン成立時に駒を削除して再配置
        /// </summary>
        private async UniTask HandleLineDeleteAsync(LineCompleteEvent lineEvent)
        {
            // 入力ロック
            _isInputLocked = true;
            
            // 演出待機
            await UniTask.Delay(LINE_DELETE_DELAY_MS);

            // 再配置対象列の HashSet
            HashSet<(int x, int z)> columnSet = new HashSet<(int, int)>();

            for (int i = 0; i < lineEvent.LinePositions.Length; i++)
            {
                IReadOnlyList<BoardIndex> line = lineEvent.LinePositions[i];

                for (int j = 0; j < line.Count; j++)
                {
                    BoardIndex index = line[j];

                    // 駒が存在する場合のみ削除
                    if (_view.HasPiece(index))
                    {
                        // 演出待機
                        await UniTask.Delay(PIECE_DELETE_DELAY_MS);

                        _view.DestroyPiece(index);
                        _view.RemovePiece(index);

                        _model.ClearCell(index);

                        columnSet.Add((index.X, index.Z));
                    }
                    else
                    {
                        Debug.LogWarning($"BoardPresenter: 駒が存在しません ({index.X}, {index.Y}, {index.Z})");
                    }
                }
            }

            // 再配置対象駒をリスト化
            List<(int x, int z)> repositionColumns = new List<(int, int)>(columnSet);

            // 演出待機
            await UniTask.Delay(PIECE_DROP_DELAY_MS);

            // 駒再配置処理
            await HandleRepositionAsync(repositionColumns);

            // ライン成立チェック
            CheckLine(_model.CheckLine());
        }

        /// <summary>
        /// 駒再配置処理
        /// </summary>
        private async UniTask HandleRepositionAsync(IReadOnlyList<(int x, int z)> columns)
        {
            List<(BoardIndex from, BoardIndex to)> allMoves =
                new List<(BoardIndex, BoardIndex)>();

            // 再配置対象作成
            for (int i = 0; i < columns.Count; i++)
            {
                (int x, int z) column = columns[i];

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
                for (int i = 0; i < columns.Count; i++)
                {
                    (int x, int z) column = columns[i];

                    _model.ApplyReposition(column.x, column.z);
                }
            }
        }

        /// <summary>
        /// ビューの駒辞書を移動情報に基づいて更新
        /// </summary>
        private void ApplyViewMoves(IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            // スナップショット作成
            Dictionary<BoardIndex, PieceData> snapshot =
                new Dictionary<BoardIndex, PieceData>();

            for (int i = 0; i < moves.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = moves[i];

                if (_view.TryGetPiece(move.from, out PieceData piece))
                {
                    snapshot[move.from] = piece;
                }
            }

            // 重複排除用ハッシュセット
            HashSet<BoardIndex> processedFrom =
                new HashSet<BoardIndex>();

            // 辞書更新
            for (int i = 0; i < moves.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = moves[i];

                // 多重実行防止
                if (!processedFrom.Add(move.from))
                {
                    continue;
                }

                if (!snapshot.TryGetValue(move.from, out PieceData piece))
                {
                    continue;
                }

                _view.RemovePiece(move.from);
                _view.SetPiece(move.to, piece);
            }
        }

        /// <summary>
        /// ライン成立判定時の共通処理
        /// </summary>
        private void CheckLine(bool isLine)
        {
            // ラインが成立している場合は処理なし
            if (isLine)
            {
                return;
            }

            // 入力ロック解除
            _isInputLocked = false;

            // フェーズ終了通知
            _onPhaseEnd.OnNext(Unit.Default);
        }
    }
}