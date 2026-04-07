// ======================================================
// PiecePlacementService.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-07
// 概要     : 列への駒配置ロジックサービス
// ======================================================

using System.Collections.Generic;
using BoardSystem.Data;

namespace BoardSystem.Service
{
    /// <summary>
    /// 列への駒配置処理サービス
    /// </summary>
    public sealed class PiecePlacementService
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>空マスを示す値</summary>
        private const int EMPTY = 0;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 再配置用移動情報キャッシュ
        /// </summary>
        private readonly List<(BoardIndex from, BoardIndex to)> _repositionCache
            = new List<(BoardIndex, BoardIndex)>(32);

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PiecePlacementService()
        {
            // キャッシュ初期化
            _repositionCache.Clear();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定列に配置可能な Y 座標を取得
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <returns>配置可能な Y　存在しない場合は -1</returns>
        public int CalculatePlace(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            return FindEmptyY(board, columnX, columnZ);
        }

        /// <summary>
        /// 配置情報を盤面に適用する
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="index">盤面インデックス</param>
        /// <param name="player">プレイヤー番号</param>
        public void ApplyPlace(
            in BoardState board,
            in BoardIndex index,
            in int player)
        {
            // 既に駒が存在する場合は配置不可
            int currentValue = board.Get(index);

            if (currentValue != EMPTY)
            {
                return;
            }

            // 駒配置
            board.Set(index, player);
        }

        /// <summary>
        /// 指定列の再配置可能な移動情報を取得
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        /// <returns>移動情報リスト（内部キャッシュ参照）</returns>
        public IReadOnlyList<(BoardIndex from, BoardIndex to)> CalculateReposition(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            // 盤面サイズ取得
            int boardSize = board.GetSize();

            // 書き込みポインタ
            int writeY = 0;

            // 下から探索
            for (int readY = 0; readY < boardSize; readY++)
            {
                // 移動元インデックス生成
                BoardIndex fromIndex = new BoardIndex(columnX, readY, columnZ);

                int value = board.Get(fromIndex);

                // 空セルはスキップ
                if (value == EMPTY)
                {
                    continue;
                }

                // 既に正しい位置にある場合
                if (readY == writeY)
                {
                    writeY++;
                    continue;
                }

                // 移動先インデックス生成
                BoardIndex toIndex = new BoardIndex(columnX, writeY, columnZ);

                // 移動情報のみ記録
                _repositionCache.Add((fromIndex, toIndex));

                // 書き込みポインタ更新
                writeY++;
            }

            return _repositionCache;
        }

        /// <summary>
        /// 再配置情報を盤面に適用する
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列 X インデックス</param>
        /// <param name="columnZ">列 Z インデックス</param>
        public void ApplyReposition(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            // キャッシュが空なら処理なし
            if (_repositionCache.Count == 0)
            {
                return;
            }

            // --------------------------------------------------
            // 移動適用
            // --------------------------------------------------
            for (int i = 0; i < _repositionCache.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = _repositionCache[i];

                // 移動元の値取得
                int value = board.Get(move.from);

                // 念のため空チェック
                if (value == EMPTY)
                {
                    continue;
                }

                // 移動先に書き込み
                board.Set(move.to, value);

                // 移動元をクリア
                board.Set(move.from, EMPTY);

                board.GetColumnValues(columnX, columnZ);
            }

            // キャッシュクリア
            _repositionCache.Clear();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定列の空マス Y を取得（上から落下基準）
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="columnX">列X</param>
        /// <param name="columnZ">列Z</param>
        /// <returns>配置可能な Y　無ければ -1</returns>
        private int FindEmptyY(
            in BoardState board,
            in int columnX,
            in int columnZ)
        {
            // 盤面サイズ取得
            int boardSize = board.GetSize();

            // 上から探索
            for (int y = boardSize - 1; y >= 0; y--)
            {
                // インデックス生成
                BoardIndex index = new BoardIndex(columnX, y, columnZ);

                // --------------------------------------------------
                // 駒が存在する場合
                // --------------------------------------------------
                if (board.Get(index) != EMPTY)
                {
                    // その1つ上が配置位置
                    int placeY = y + 1;

                    // 範囲外なら配置不可
                    if (placeY >= boardSize)
                    {
                        return -1;
                    }

                    return placeY;
                }
            }

            // 全て空の場合は最下段に配置
            return 0;
        }
    }
}