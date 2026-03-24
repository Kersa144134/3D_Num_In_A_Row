// ======================================================
// BoardState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-02-20
// 概要     : 盤面データ管理クラス
//            任意サイズの3D盤面に対応
//            上面列情報も保持
// ======================================================

namespace BoardSystem.Data
{
    /// <summary>
    /// 任意サイズ3D盤面状態保持クラス
    /// </summary>
    public sealed class BoardState
    {
        // ======================================================
        // 構造体
        // ======================================================

        /// <summary>
        /// 列情報
        /// </summary>
        private struct Column
        {
            /// <summary>X 座標</summary>
            public int X;

            /// <summary>Z 座標</summary>
            public int Z;

            /// <summary>コンストラクタ</summary>
            public Column(int x, int z)
            {
                X = x;
                Z = z;
            }
        }

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 内部盤面サイズ
        /// </summary>
        private readonly int _boardSize;

        /// <summary>
        /// 内部盤面データ
        /// </summary>
        private readonly int[,,] _board;

        /// <summary>
        /// 上面列情報
        /// </summary>
        private readonly Column[,] _columns;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// 空マス
        /// </summary>
        private const int EMPTY = 0;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="boardSize">盤面サイズ</param>
        public BoardState(in int boardSize)
        {
            _boardSize = boardSize;
            _board = new int[_boardSize, _boardSize, _boardSize];

            // 上面列情報生成
            _columns = new Column[_boardSize, _boardSize];
            InitializeColumns();

            Initialize();
        }

        // ======================================================
        // セッター
        // ======================================================

        /// <summary>
        /// 指定座標の値設定
        /// </summary>
        public void Set(in int x, in int y, in int z, in int value)
        {
            _board[x, y, z] = value;
        }

        // ======================================================
        // ゲッター
        // ======================================================

        /// <summary>
        /// 盤面サイズ取得
        /// </summary>
        public int GetSize()
        {
            return _boardSize;
        }

        /// <summary>
        /// 指定座標の値取得
        /// </summary>
        public int Get(in int x, in int y, in int z)
        {
            return _board[x, y, z];
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 盤面の初期化
        /// </summary>
        public void Initialize()
        {
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    for (int z = 0; z < _boardSize; z++)
                    {
                        _board[x, y, z] = EMPTY;
                    }
                }
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 上面列情報の初期化
        /// </summary>
        private void InitializeColumns()
        {
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    _columns[x, z] = new Column(x, z);
                }
            }
        }
    }
}