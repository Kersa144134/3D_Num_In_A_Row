// ======================================================
// BoardPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-03
// 概要     : 3D 目並べゲームの盤面を制御するクラス（コメント粒度強化版）
// ======================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using InputSystem;
using PhaseSystem.Data;
using SceneSystem.Data;
using BoardSystem.Data;
using UnityEngine.UIElements;

namespace BoardSystem
{
    /// <summary>
    /// 盤面操作用プレゼンター
    /// </summary>
    public sealed class BoardPresenter : MonoBehaviour, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("盤面")]
        /// <summary>
        /// 盤面サイズ（X,Z方向）
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

        /// <summary>現在のプレイヤーID</summary>
        private int _currentPlayer;

        /// <summary>盤面のクリック判定用 Collider</summary>
        private Collider _boardCollider;

        /// <summary>駒落下・ライン削除中フラグ</summary>
        private bool _isProcessing;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private CompositeDisposable _disposables;

        /// <summary>
        /// ライン成立通知用 Subject
        /// </summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete = new Subject<LineCompleteEvent>();

        /// <summary>ライン成立ストリーム</summary>
        public IObservable<LineCompleteEvent> OnLineComplete => _onLineComplete;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>1P ID</summary>
        private const int PLAYER_ONE = 1;

        /// <summary>2P ID</summary>
        private const int PLAYER_TWO = 2;

        // ======================================================
        // ライフサイクル
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
                throw new InvalidOperationException("BoardCollider が見つかりません。");
            }

            // 初期プレイヤー設定
            // 1Pから開始
            _currentPlayer = PLAYER_ONE;
        }

        /// <summary>
        /// 終了処理
        /// </summary>
        public void OnExit()
        {
            _disposables.Dispose();
            _model.Dispose();
        }

        /// <summary>
        /// フェーズ開始時処理
        /// </summary>
        public void OnPhaseEnter(in PhaseType phase)
        {
            if (phase != PhaseType.Play) return;

            // --------------------------------------------------
            // イベント購読
            // --------------------------------------------------
            _disposables = new CompositeDisposable();

            // ボタンA 押下
            InputManager.Instance.ButtonA.OnDown
                .Subscribe(_ => HandleDropColumn())
                .AddTo(_disposables);

            // ボタンB 押下
            InputManager.Instance.ButtonB.OnDown
                .Subscribe(_ => HandleDropColumn())
                .AddTo(_disposables);

            // モデルからのライン成立通知購読
            _model.OnLineComplete
                .Subscribe(async lineEvent =>
                {
                    // ラインイベントを外部に通知
                    _onLineComplete.OnNext(lineEvent);

                    // ライン削除処理
                    await HandleLineDeleteAsync(lineEvent);
                })
                .AddTo(_disposables);
        }

        /// <summary>
        /// フェーズ終了時処理
        /// </summary>
        public void OnPhaseExit(in PhaseType phase)
        {
            if (phase != PhaseType.Play) return;

            _disposables.Dispose();
            _disposables = null;
        }

        // ======================================================
        // 駒落下関連
        // ======================================================

        /// <summary>
        /// クリック座標から駒落下処理を開始
        /// </summary>
        private void HandleDropColumn()
        {
            // --------------------------------------------------
            // カメラから Ray生成
            // --------------------------------------------------
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.Pointer);

            // --------------------------------------------------
            // Ray 判定
            // --------------------------------------------------
            bool isHit = Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _raycastLayerMask);
            if (!isHit || hit.collider != _boardCollider) return;

            // --------------------------------------------------
            // ヒット座標を列インデックスに変換
            // --------------------------------------------------
            Vector3 hitPos = hit.point;
            _view.WorldToColumn(hitPos.x, hitPos.z, out int x, out int z);

            // --------------------------------------------------
            // 駒落下処理
            // --------------------------------------------------
            HandleDropAsync(x, z).Forget();
        }

        /// <summary>
        /// 駒落下処理
        /// </summary>
        private async UniTask HandleDropAsync(int x, int z)
        {
            // 多重実行防止
            if (_isProcessing)
            {
                return;
            }

            _isProcessing = true;

            // --------------------------------------------------
            // 配置可能判定
            // --------------------------------------------------
            bool canDrop = _model.CanPlace(x, z);

            if (!canDrop)
            {
                _isProcessing = false;
                return;
            }

            // 駒配置
            int y = _model.Place(x, z, _currentPlayer);

            if (y < 0)
            {
                _isProcessing = false;
                return;
            }

            // 駒生成
            await _view.SpawnPieceAsync(x, y, z, _currentPlayer);

            // ライン成立チェック
            _model.CheckLine();

            // プレイヤー切替
            SwitchPlayer();

            _isProcessing = false;
        }

        /// <summary>
        /// プレイヤー切替
        /// </summary>
        private void SwitchPlayer()
        {
            _currentPlayer = _currentPlayer == PLAYER_ONE ? PLAYER_TWO : PLAYER_ONE;
        }

        /// <summary>
        /// ライン成立時に駒を削除して再配置
        /// </summary>
        private async UniTask HandleLineDeleteAsync(LineCompleteEvent lineEvent)
        {
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
                        _view.DeletePiece(index);
                        _model.ClearCell(index);
                        columnSet.Add((index.X, index.Z));
                    }
                }
            }

            // 再配置対象列をリスト化
            List<(int x, int z)> repositionColumns = new List<(int, int)>(columnSet);
            Debug.Log($"columnSet {columnSet.Count}");

            // 駒再配置処理
            await HandleRepositionAsync(repositionColumns);
        }

        /// <summary>
        /// 駒再配置処理
        /// </summary>
        private async UniTask HandleRepositionAsync(IReadOnlyList<(int x, int z)> repositionColumns)
        {
            // 全移動情報リスト
            List<(BoardIndex from, BoardIndex to)> allMoves = new List<(BoardIndex, BoardIndex)>();

            // 各列ごとにモデルから移動情報取得
            for (int i = 0; i < repositionColumns.Count; i++)
            {
                (int x, int z) column = repositionColumns[i];

                IReadOnlyList<(BoardIndex from, BoardIndex to)> moves = _model.Reposition(column.x, column.z);

                for (int j = 0; j < moves.Count; j++)
                {
                    allMoves.Add(moves[j]);
                }
            }

            if (allMoves.Count == 0)
            {
                return;
            }
            Debug.Log($"allMoves {allMoves.Count}");


            // 落下処理
            await _view.MovePiecesAsync(allMoves);
        }
    }
}