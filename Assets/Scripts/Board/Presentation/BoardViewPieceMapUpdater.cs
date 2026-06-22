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
        public void UpdatePieceMap(in IReadOnlyList<BoardMoveResult> moves)
        {
            // --------------------------------------------------
            // 重複排除処理
            // --------------------------------------------------
            HashSet<BoardIndex> uniqueFromSet =
                new HashSet<BoardIndex>();

            HashSet<BoardIndex> uniqueToSet =
                new HashSet<BoardIndex>();

            List<BoardMoveResult> filteredMoves =
                new List<BoardMoveResult>();

            for (int i = 0; i < moves.Count; i++)
            {
                // 現在の移動情報
                BoardMoveResult move = moves[i];

                // from 重複排除
                if (uniqueFromSet.Contains(move.From))
                {
                    continue;
                }

                // to 重複排除
                if (uniqueToSet.Contains(move.To))
                {
                    continue;
                }

                uniqueFromSet.Add(move.From);
                uniqueToSet.Add(move.To);

                filteredMoves.Add(move);
            }

            // --------------------------------------------------
            // スナップショット生成
            // --------------------------------------------------
            Dictionary<BoardIndex, PieceData> snapshot =
                new Dictionary<BoardIndex, PieceData>();

            for (int i = 0; i < filteredMoves.Count; i++)
            {
                BoardMoveResult move = filteredMoves[i];

                PieceData piece;

                if (_view.TryGetPiece(move.From, out piece))
                {
                    snapshot[move.From] = piece;
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
                BoardMoveResult move = filteredMoves[i];

                PieceData piece;

                if (snapshot.TryGetValue(move.From, out piece))
                {
                    _view.SetPiece(move.To, piece);
                }
            }
        }
    }
}