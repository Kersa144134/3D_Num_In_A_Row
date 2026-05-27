// ======================================================
// BoardViewPieceMapUpdater.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : 駒移動情報を基にビューの駒辞書を更新するクラス
// ======================================================

using System.Collections.Generic;
using BoardSystem.Domain;

namespace BoardSystem.Presentation
{
    /// <summary>
    /// ビューの駒辞書を更新するクラス
    /// </summary>
    public sealed class BoardViewPieceMapUpdater
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>ビュー</summary>
        private readonly BoardView _view;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardViewPieceMapUpdater(in BoardView view)
        {
            _view = view;
        }

        // ======================================================
        // メソッド
        // ======================================================

        /// <summary>
        /// ビューの駒辞書を移動情報に基づいて更新
        /// </summary>
        public void UpdatePieceMap(in IReadOnlyList<(BoardIndex from, BoardIndex to)> moves)
        {
            // --------------------------------------------------
            // 重複排除処理
            // --------------------------------------------------
            HashSet<BoardIndex> uniqueFromSet =
                new HashSet<BoardIndex>();

            HashSet<BoardIndex> uniqueToSet =
                new HashSet<BoardIndex>();

            List<(BoardIndex from, BoardIndex to)> filteredMoves =
                new List<(BoardIndex, BoardIndex)>();

            for (int i = 0; i < moves.Count; i++)
            {
                // 現在の移動情報
                (BoardIndex from, BoardIndex to) move = moves[i];

                // from 重複排除
                if (uniqueFromSet.Contains(move.from))
                {
                    continue;
                }

                // to 重複排除
                if (uniqueToSet.Contains(move.to))
                {
                    continue;
                }

                uniqueFromSet.Add(move.from);
                uniqueToSet.Add(move.to);

                filteredMoves.Add(move);
            }

            // --------------------------------------------------
            // スナップショット生成
            // --------------------------------------------------
            Dictionary<BoardIndex, PieceData> snapshot =
                new Dictionary<BoardIndex, PieceData>();

            for (int i = 0; i < filteredMoves.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = filteredMoves[i];

                PieceData piece;

                if (_view.TryGetPiece(move.from, out piece))
                {
                    snapshot[move.from] = piece;
                }
            }

            // --------------------------------------------------
            // from 削除
            // --------------------------------------------------
            foreach (BoardIndex from in snapshot.Keys)
            {
                _view.RemovePiece(from);
            }

            // --------------------------------------------------
            // to 配置
            // --------------------------------------------------
            for (int i = 0; i < filteredMoves.Count; i++)
            {
                (BoardIndex from, BoardIndex to) move = filteredMoves[i];

                PieceData piece;

                if (snapshot.TryGetValue(move.from, out piece))
                {
                    _view.SetPiece(move.to, piece);
                }
            }
        }
    }
}