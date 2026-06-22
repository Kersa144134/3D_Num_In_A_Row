// ======================================================
// BoardState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-07
// 概要     : 盤面データ管理クラス
// ======================================================

namespace BoardSystem.Domain
{
    /// <summary>
    /// 任意サイズ3D盤面状態保持クラス
    /// </summary>
    public sealed class BoardState : IBoardReader, IBoardWriter
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

            // 盤面情報生成
            _board = new int[_boardSize, _boardSize, _boardSize];

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

            // 上面列情報生成
            _columns = new Column[_boardSize, _boardSize];

            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    _columns[x, z] = new Column(x, z);
                }
            }
        }

        // ======================================================
        // IBoardReader 実装
        // ======================================================

        /// <summary>
        /// 指定座標の値取得
        /// </summary>
        public int Get(in BoardIndex index)
        {
            return _board[index.X, index.Y, index.Z];
        }

        /// <summary>
        /// 盤面サイズ取得
        /// </summary>
        public int GetSize()
        {
            return _boardSize;
        }

        // ======================================================
        // IBoardWriter 実装
        // ======================================================

        /// <summary>
        /// 指定座標の値設定
        /// </summary>
        public void Set(in BoardIndex index, in int value)
        {
            _board[index.X, index.Y, index.Z] = value;
        }

        /// <summary>
        /// 指定座標のマスをクリア
        /// </summary>
        public void Clear(in BoardIndex index)
        {
            _board[index.X, index.Y, index.Z] = EMPTY;
        }

        /// <summary>
        /// 盤面データを一括反映
        /// </summary>
        /// <param name="boardData">反映する盤面データ</param>
        public void ApplyBoard(in int[,,] boardData)
        {
            for (int x = 0; x < _boardSize; x++)
            {
                for (int y = 0; y < _boardSize; y++)
                {
                    for (int z = 0; z < _boardSize; z++)
                    {
                        _board[x, y, z] = boardData[x, y, z];
                    }
                }
            }
        }
    }
}