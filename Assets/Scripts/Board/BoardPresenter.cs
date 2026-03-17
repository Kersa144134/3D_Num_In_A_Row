// ======================================================
// BoardPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-03-16
// 概要     : 3D 目並べゲームの進行を制御するクラス
// ======================================================

using UnityEngine;
using InputSystem.Controller;
using InputSystem.Manager;
using SceneSystem.Data;

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

        /// <summary>
        /// 現在プレイヤー
        /// </summary>
        private int _currentPlayer;

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

        /// <summary>
        /// 初期化
        /// </summary>
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

            // --------------------------------------------------
            // 初期プレイヤー設定
            // --------------------------------------------------
            _currentPlayer = PLAYER_ONE;
        }

        /// <summary>
        /// 更新処理
        /// </summary>
        public void OnUpdate(
            in float unscaledDeltaTime,
            in float elapsedTime)
        {
            // --------------------------------------------------
            // 入力検知
            // --------------------------------------------------
            if (!InputManager.Instance.ButtonA.Down)
            {
                return;
            }

            // --------------------------------------------------
            // 入力デバイス確認
            // --------------------------------------------------
            if (InputManager.Instance.DeviceManager.ActiveController
                is not VirtualGamepadInputController virtualController)
            {
                return;
            }

            // --------------------------------------------------
            // Ray生成
            // --------------------------------------------------
            Ray ray =
                Camera.main.ScreenPointToRay(
                    virtualController.GetPointerPosition());

            // --------------------------------------------------
            // Raycast
            // --------------------------------------------------
            if (!Physics.Raycast(ray, out RaycastHit hit))
            {
                return;
            }

            // --------------------------------------------------
            // ワールド座標取得
            // --------------------------------------------------
            Vector3 hitPos = hit.point;

            // --------------------------------------------------
            // Viewに列変換を依頼
            // --------------------------------------------------
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

        // ======================================================
        // プライベートメソッド
        // ======================================================

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
            // 勝利判定
            // --------------------------------------------------
            bool isChain =
                _model.CheckLine(
                    _currentPlayer);

            if (isChain)
            {
                Debug.Log(
                    "Player " +
                    _currentPlayer +
                    " Win");
            }

            // --------------------------------------------------
            // プレイヤー交代
            // --------------------------------------------------
            SwitchPlayer();
        }

        public void OnExit()
        {

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