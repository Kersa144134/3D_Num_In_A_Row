// ======================================================
// BoardPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-03-16
// 概要     : 3D 目並べゲームの盤面を制御するクラス
// ======================================================

using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using InputSystem;
using PhaseSystem.Data;
using SceneSystem.Data;
using BoardSystem.Data;
using System.Linq;
using System.Collections.Generic;

namespace BoardSystem
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
        /// <summary>盤面サイズ</summary>
        [SerializeField, Min(3)]
        private int _boardSize;

        /// <summary>勝利条件マス数</summary>
        [SerializeField, Min(3)]
        private int _connectCount;

        /// <summary>1P 駒 Prefab</summary>
        [SerializeField]
        private GameObject _playerOnePrefab;

        /// <summary>2P 駒 Prefab</summary>
        [SerializeField]
        private GameObject _playerTwoPrefab;

        [Header("レイヤー")]
        /// <summary>Ray がヒットするレイヤー</summary>
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

        /// <summary>現在のプレイヤー ID</summary>
        private int _currentPlayer;

        /// <summary>クリック判定対象の Collider</summary>
        private Collider _boardCollider;

        /// <summary>落下処理中フラグ</summary>
        private bool _isProcessing;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private CompositeDisposable _disposables;

        /// <summary>ライン成立用 Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete =
            new Subject<LineCompleteEvent>();

        /// <summary>ライン成立ストリーム</summary>
        public IObservable<LineCompleteEvent> OnLineComplete =>
            _onLineComplete;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>1P</summary>
        private const int PLAYER_ONE = 1;

        /// <summary>2P</summary>
        private const int PLAYER_TWO = 2;

        // ======================================================
        // IUpdatable
        // ======================================================

        public void OnEnter()
        {
            // モデル、ビューの生成
            _model =
                new BoardModel(
                    _boardSize,
                    _connectCount);

            _view =
                new BoardView(
                    transform,
                    _boardSize,
                    _playerOnePrefab,
                    _playerTwoPrefab);

            // コライダー取得
            _boardCollider = GetComponentInChildren<Collider>(true);

            if (_boardCollider == null)
            {
                throw new InvalidOperationException("BoardCollider が見つかりません。");
            }

            // 初期プレイヤー設定
            _currentPlayer = PLAYER_ONE;
        }

        public void OnExit()
        {
            // イベント購読解除
            _disposables.Dispose();
            _model.Dispose();
        }

        public void OnPhaseEnter(in PhaseType phase)
        {
            if (phase == PhaseType.Play)
            {
                // --------------------------------------------------
                // イベント購読
                // --------------------------------------------------
                _disposables = new CompositeDisposable();

                // A ボタン押下時
                InputManager.Instance.ButtonA.OnDown
                    .Subscribe(_ => HandleDropColumn())
                    .AddTo(_disposables);

                // B ボタン押下時
                InputManager.Instance.ButtonB.OnDown
                    .Subscribe(_ => HandleDropColumn())
                    .AddTo(_disposables);

                // ライン成立時
                _model.OnLineComplete
                    .Subscribe(lineEvent =>
                    {
                        // イベント客家
                        _onLineComplete.OnNext(lineEvent);

                        // 成立ラインの駒を削除
                        HandleLineDelete(lineEvent);
                    })
                    .AddTo(_disposables);
            }
        }

        public void OnPhaseExit(in PhaseType phase)
        {
            if (phase == PhaseType.Play)
            {
                // イベント購読解除
                _disposables.Dispose();
                _disposables = null;
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 駒落下時に呼ばれるハンドラ
        /// </summary>
        private void HandleDropColumn()
        {
            // --------------------------------------------------
            // Ray 生成
            // --------------------------------------------------
            Ray ray = Camera.main.ScreenPointToRay(InputManager.Instance.Pointer);

            bool isHit =
                Physics.Raycast(
                    ray,
                    out RaycastHit hit,
                    Mathf.Infinity,
                    _raycastLayerMask);

            // --------------------------------------------------
            // ヒット判定
            // --------------------------------------------------
            // ヒットしなかった場合は処理なし
            if (!isHit)
            {
                return;
            }

            // コライダーが自身と一致しない場合は処理なし
            if (hit.collider != _boardCollider)
            {
                return;
            }

            // --------------------------------------------------
            // ワールド座標取得
            // --------------------------------------------------
            Vector3 hitPos = hit.point;

            // ビューに列変換処理を委譲
            _view.WorldToColumn(
                hitPos.x,
                hitPos.z,
                out int x,
                out int z);

            // --------------------------------------------------
            // 駒落下処理
            // --------------------------------------------------
            HandleDropAsync(x, z).Forget();
        }

        /// <summary>
        /// 駒落下処理
        /// </summary>
        private async UniTask HandleDropAsync(
            int x,
            int z)
        {
            // 多重実行防止
            if (_isProcessing)
            {
                return;
            }

            // 処理開始
            _isProcessing = true;

            // --------------------------------------------------
            // 配置可能判定
            // --------------------------------------------------
            bool canDrop = _model.CanPlace(
                x,
                z
            );

            if (!canDrop)
            {
                // 処理終了
                _isProcessing = false;

                return;
            }

            // --------------------------------------------------
            // 配置処理
            // --------------------------------------------------
            int y = _model.Place(
                x,
                z,
                _currentPlayer
            );

            if (y < 0)
            {
                // 処理終了
                _isProcessing = false;

                return;
            }

            // --------------------------------------------------
            // ビュー更新
            // --------------------------------------------------
            await _view.SpawnPieceAsync(
                x,
                y,
                z,
                _currentPlayer
            );

            // --------------------------------------------------
            // ライン成立判定
            // --------------------------------------------------
            _model.CheckLine();

            // --------------------------------------------------
            // プレイヤー交代
            // --------------------------------------------------
            SwitchPlayer();

            // 処理終了
            _isProcessing = false;
        }

        /// <summary>
        /// プレイヤー切替
        /// </summary>
        private void SwitchPlayer()
        {
            _currentPlayer =
                _currentPlayer == PLAYER_ONE
                ? PLAYER_TWO
                : PLAYER_ONE;
        }

        /// <summary>
        /// ライン成立時の削除処理
        /// </summary>
        /// <param name="lineEvent">ライン情報</param>
        private void HandleLineDelete(LineCompleteEvent lineEvent)
        {
            // --------------------------------------------------
            // 成立ラインの駒を削除
            // --------------------------------------------------
            foreach (IReadOnlyList<BoardIndex> line in lineEvent.LinePositions)
            {
                foreach (BoardIndex index in line)
                {
                    // --------------------------------------------------
                    // ビューに削除委譲
                    // --------------------------------------------------
                    _view.DeletePiece(index);

                    // --------------------------------------------------
                    // モデルも盤面クリア
                    // --------------------------------------------------
                    _model.ClearCell(index);
                }
            }
        }
    }
}