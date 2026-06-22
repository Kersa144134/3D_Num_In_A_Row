// ======================================================
// BoardRepositionUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : 駒再配置処理を担当するユースケース
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using BoardSystem.Domain;

namespace BoardSystem.Application
{
    /// <summary>
    /// 駒再配置ユースケース
    /// </summary>
    public sealed class BoardRepositionUseCase
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>盤面モデル</summary>
        private readonly BoardModel _model;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardRepositionUseCase(in BoardModel model)
        {
            _model = model;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 再配置処理を実行
        /// </summary>
        public UniTask<BoardRepositionResult> HandleRepositionAsync(
            IReadOnlyList<(int x, int z)> columns)
        {
            // 全再配置情報
            List<BoardMoveResult> repositionMoves =
                new List<BoardMoveResult>();

            // --------------------------------------------------
            // 移動計算
            // --------------------------------------------------
            for (int i = 0; i < columns.Count; i++)
            {
                // 列取得
                (int x, int z) column =
                    columns[i];

                // 移動計算取得
                IReadOnlyList<BoardMoveResult> moves =
                    _model.CalculateReposition(
                        column.x,
                        column.z
                    );

                // 結果統合
                repositionMoves.AddRange(moves);
            }

            // 移動なし
            if (repositionMoves.Count == 0)
            {
                return UniTask.FromResult(
                    new BoardRepositionResult(new List<BoardMoveResult>())
                );
            }

            // 再配置適用
            _model.ApplyReposition(repositionMoves);

            // 結果返却
            return UniTask.FromResult(
                new BoardRepositionResult(repositionMoves)
            );
        }
    }
}