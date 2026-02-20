// ======================================================
// BoardManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 概要     : 3×3×3三目並べの統括管理クラス
//            入力受付・ロジック呼び出し・表示生成を担当
// ======================================================

using UnityEngine;
using BoardSystem.Data;
using BoardSystem.Service;

namespace BoardSystem.Manager
{
    /// <summary>
    /// 3D 三目並べ統括クラス
    /// </summary>
    public sealed class BoardManager : MonoBehaviour
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>
        /// 盤面サイズ
        /// </summary>
        [SerializeField, Min(3)]
        private int _boardSize;

        /// <summary>
        /// 勝利条件の連続マス数
        /// </summary>
        [SerializeField, Min(3)]
        private int _connectCount;

        /// <summary>
        /// プレイヤー 1 駒 Prefab
        /// </summary>
        [SerializeField]
        private GameObject _playerOnePrefab;

        /// <summary>
        /// プレイヤー 2 駒 Prefab
        /// </summary>
        [SerializeField]
        private GameObject _playerTwoPrefab;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>
        /// 盤面状態管理クラス
        /// </summary>
        private BoardState _boardState;

        /// <summary>
        /// 勝利判定クラス
        /// </summary>
        private BoardPositionConverter _positionConverter;

        /// <summary>
        /// 落下処理クラス
        /// </summary>
        private ColumnDropService _columnDrop;

        /// <summary>
        /// 勝利判定クラス
        /// </summary>
        private WinJudgeService _winJudge;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// マス間隔
        /// </summary>
        private float _cellSpacing;

        /// <summary>
        /// 現在のプレイヤー
        /// </summary>
        private int _currentPlayer;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// プレイヤー 1 識別値
        /// </summary>
        private const int PLAYER_ONE = 1;

        /// <summary>
        /// プレイヤー 2 識別値
        /// </summary>
        private const int PLAYER_TWO = 2;

        // ======================================================
        // Unity イベント
        // ======================================================

        /// <summary>
        /// Unity Awake
        /// </summary>
        private void Awake()
        {
            // 勝利条件が盤面サイズを超えていた場合、盤面サイズに合わせる
            if (_connectCount > _boardSize)
            {
                _connectCount = _boardSize;
            }

            _boardState = new BoardState(_boardSize);
            _positionConverter = new BoardPositionConverter(_boardSize);
            _columnDrop = new ColumnDropService();
            _winJudge = new WinJudgeService(_boardSize, _connectCount);

            _cellSpacing = transform.localScale.x / _boardSize;
            _currentPlayer = PLAYER_ONE;
        }

        /// <summary>
        /// テスト用入力処理
        /// </summary>
        private void Update()
        {
            // 左クリック検知
            if (Input.GetMouseButtonDown(0))
            {
                // カメラからのRayを作成
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // Raycast実行
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // クリック位置のワールド座標を取得
                    Vector3 hitPos = hit.point;

                    // BoardPositionConverter を使って列インデックス計算
                    _positionConverter.WorldPositionToColumn(
                        _cellSpacing,
                        hitPos.x,
                        hitPos.z,
                        out int x,
                        out int z
                    );

                    // ログ出力：クリック位置と列番号
                    Debug.Log($"Calculated Column: X={x}, Z={z}");

                    // 計算した列座標を使って駒落下
                    HandleDrop(x, z);
                }
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 列に駒を落下させる
        /// </summary>
        private void HandleDrop(in int x, in int z)
        {
            // 落下可能か判定
            bool canDrop = _columnDrop.CanDrop(_boardState, x, z);

            // 落下不可なら終了
            if (!canDrop)
            {
                return;
            }

            // 落下実行
            int y = _columnDrop.Drop(_boardState, x, z, _currentPlayer);

            // 表示生成
            SpawnPieceVisual(x, y, z, _currentPlayer);

            // 勝利判定
            bool isWin = _winJudge.Check(_boardState, _currentPlayer);

            // 勝利している場合ログ出力
            if (isWin)
            {
                Debug.Log("Player " + _currentPlayer + " Win");
            }

            // プレイヤー交代
            SwitchPlayer();
        }

        /// <summary>
        /// 駒の見た目生成
        /// </summary>
        private void SpawnPieceVisual(in int x, in int y, in int z, in int player)
        {
            // BoardPositionConverter を用いて列インデックスからワールド座標に変換
            float worldX;
            float worldY;
            float worldZ;

            _positionConverter.ColumnToWorldPosition(
                _cellSpacing,
                x,
                y,
                z,
                out worldX,
                out worldY,
                out worldZ
            );

            // 駒生成位置
            Vector3 position = new Vector3(
                worldX,
                worldY,
                worldZ
            );

            // 使用Prefab選択
            GameObject prefab = player == PLAYER_ONE
                ? _playerOnePrefab
                : _playerTwoPrefab;
            
            // 駒生成
            GameObject piece = Instantiate(prefab, position, Quaternion.identity, transform);

            // 盤面サイズに応じてスケール調整
            float scaleFactor = 1f / (_boardSize + 0.5f);
            piece.transform.localScale = Vector3.one * scaleFactor;
        }

        /// <summary>
        /// 現在プレイヤー切替
        /// </summary>
        private void SwitchPlayer()
        {
            // プレイヤー番号を反転
            _currentPlayer = _currentPlayer == PLAYER_ONE
                ? PLAYER_TWO
                : PLAYER_ONE;
        }
    }
}