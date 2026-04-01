// ======================================================
// BoardPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-03-16
// 概要     : 3D 目並べゲームの盤面を制御するクラス
// ======================================================

using UnityEngine;
using UniRx;
using InputSystem;
using PhaseSystem.Data;
using SceneSystem.Data;
using BoardSystem.Data;
using System;

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

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private CompositeDisposable _disposables;

        /// <summary>ライン成立イベント（外部公開用）</summary>
        public IObservable<LineCompleteEvent> OnLineComplete
        {
            get
            {
                // モデル未生成時は空ストリームを返す
                return _model != null
                    ? _model.OnLineComplete
                    : Observable.Empty<LineCompleteEvent>();
            }
        }

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
            _boardCollider =
                GetComponentInChildren<Collider>(true);

            if (_boardCollider == null)
            {
                throw new InvalidOperationException(
                    "BoardCollider が見つかりません。子オブジェクトに Collider を配置してください。");
            }

            // 初期プレイヤー設定
            _currentPlayer = PLAYER_ONE;
        }

        public void OnExit()
        {
            // --------------------------------------------------
            // イベント購読解除
            // --------------------------------------------------
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
            }
        }

        public void OnPhaseExit(in PhaseType phase)
        {
            if (phase == PhaseType.Play)
            {
                // --------------------------------------------------
                // イベント購読解除
                // --------------------------------------------------
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
            HandleDrop(x, z);
        }

        /// <summary>
        /// 駒落下処理
        /// </summary>
        private void HandleDrop(
            in int x,
            in int z)
        {
            // --------------------------------------------------
            // 落下可能判定
            // --------------------------------------------------
            bool canDrop =
                _model.CanDrop(
                    x,
                    z);

            if (!canDrop)
            {
                return;
            }

            // --------------------------------------------------
            // 落下処理
            // --------------------------------------------------
            int y =
                _model.Drop(
                    x,
                    z,
                    _currentPlayer);

            // --------------------------------------------------
            // ビュー更新
            // --------------------------------------------------
            _view.SpawnPiece(
                x,
                y,
                z,
                _currentPlayer);

            // --------------------------------------------------
            // ライン成立判定
            // --------------------------------------------------
            _model.CheckLine();

            // --------------------------------------------------
            // プレイヤー交代
            // --------------------------------------------------
            SwitchPlayer();
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
    }
}