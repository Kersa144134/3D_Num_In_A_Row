// ======================================================
// PiecePlacement.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-20
// 更新日時 : 2026-04-07
// 概要     : 列への駒配置クラス
// ======================================================

using System.Collections.Generic;

namespace BoardSystem.Domain
{
    /// <summary>
    /// 列への駒配置処理サービス
    /// </summary>
    public sealed class PiecePlacement
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>空マスを示す値</summary>
        private const int EMPTY = 0;

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
            in IBoardReader board,
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

        /// <summary>
        /// 配置情報を盤面に適用する
        /// </summary>
        /// <param name="board">盤面データ</param>
        /// <param name="index">盤面インデックス</param>
        /// <param name="player">プレイヤー番号</param>
        public void ApplyPlace(
            in IBoardWriter  board,
            in BoardIndex index,
            in int player)
        {
            // プレイヤー番号が不正値なら処理なし
            if (player < 0)
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
        /// <returns>移動情報リスト</returns>
        public IReadOnlyList<BoardMoveResult> CalculateReposition(
            in IBoardReader board,
            in int columnX,
            in int columnZ)
        {
            // 再配置情報
            List<BoardMoveResult> repositionMoves =
                new List<BoardMoveResult>();
            
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

                // 移動情報記録
                repositionMoves.Add(new BoardMoveResult(fromIndex, toIndex));

                // 書き込みポインタ更新
                writeY++;
            }

            return repositionMoves;
        }

        /// <summary>
        /// 移動元の値一覧を取得
        /// </summary>
        /// <param name="boardReader">参照対象の盤面</param>
        /// <param name="moves">移動情報一覧</param>
        /// <returns>移動元座標と値の対応表</returns>
        public Dictionary<BoardIndex, int> CreateMoveValueMap(
            in IBoardReader boardReader,
            in IReadOnlyList<BoardMoveResult> moves)
        {
            // 移動元座標と値の対応表
            Dictionary<BoardIndex, int> moveValues =
                new Dictionary<BoardIndex, int>();

            // 全移動情報を走査
            for (int i = 0; i < moves.Count; i++)
            {
                // 移動情報取得
                BoardMoveResult move = moves[i];

                // 移動元の値取得
                int value = boardReader.Get(move.From);

                // 空セルは移動対象外
                if (value == EMPTY)
                {
                    continue;
                }

                // 移動元座標と値を登録
                moveValues.Add(move.From, value);
            }

            return moveValues;
        }

        /// <summary>
        /// 再配置情報を盤面へ適用する
        /// </summary>
        /// <param name="boardWriter">更新対象の盤面</param>
        /// <param name="moves">移動情報一覧</param>
        /// <param name="moveValues">移動元座標と値の対応表</param>
        public void ApplyMoves(
            in IBoardWriter boardWriter,
            in IReadOnlyList<BoardMoveResult> moves,
            in IReadOnlyDictionary<BoardIndex, int> moveValues)
        {
            // 全移動情報を走査
            for (int i = 0; i < moves.Count; i++)
            {
                // 移動情報取得
                BoardMoveResult move = moves[i];

                // 移動元の値を取得
                if (!moveValues.TryGetValue(move.From, out int value))
                {
                    continue;
                }

                // 移動先へ値を書き込む
                boardWriter.Set(move.To, value);

                // 移動元セルを空にする
                boardWriter.Clear(move.From);
            }
        }
    }
}