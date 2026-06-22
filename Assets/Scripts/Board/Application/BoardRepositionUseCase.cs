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
        public BoardRepositionUseCase(BoardModel model)
        {
            // モデル参照保持
            _model = model;
        }

        // ======================================================
        // メソッド
        // ======================================================

        /// <summary>
        /// 再配置処理を実行
        /// </summary>
        public UniTask<BoardRepositionResult> HandleRepositionAsync(
            IReadOnlyList<(int x, int z)> columns)
        {
            // 全移動情報
            List<(BoardIndex from, BoardIndex to)> allMoves =
                new List<(BoardIndex from, BoardIndex to)>();

            // --------------------------------------------------
            // 移動計算
            // --------------------------------------------------
            for (int i = 0; i < columns.Count; i++)
            {
                // 列取得
                (int x, int z) column =
                    columns[i];

                // 移動計算取得
                IReadOnlyList<(BoardIndex from, BoardIndex to)> moves =
                    _model.CalculateReposition(
                        column.x,
                        column.z
                    );

                // 結果統合
                allMoves.AddRange(moves);
            }

            // 移動なし
            if (allMoves.Count == 0)
            {
                return UniTask.FromResult(
                    new BoardRepositionResult(allMoves)
                );
            }

            // 再配置適用
            _model.ApplyReposition(allMoves);

            // 結果返却
            return UniTask.FromResult(
                new BoardRepositionResult(allMoves)
            );
        }
    }
}