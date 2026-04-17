// ======================================================
// BoardLineUseCase.cs
// 概要 : ライン成立時の削除と再配置を担当するユースケース
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using BoardSystem.Domain;

namespace BoardSystem.Application
{
    /// <summary>
    /// ライン処理ユースケース
    /// </summary>
    public sealed class BoardLineUseCase
    {
        /// <summary>盤面モデル</summary>
        private readonly BoardModel _model;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardLineUseCase(BoardModel model)
        {
            // モデル参照を保持
            _model = model;
        }

        /// <summary>
        /// ライン削除と再配置
        /// </summary>
        public UniTask<LineResult> ExecuteAsync(LineCompleteEvent lineEvent)
        {
            // 削除対象セット
            HashSet<BoardIndex> deleteSet = new HashSet<BoardIndex>();

            // 対象列セット
            HashSet<(int x, int z)> columns = new HashSet<(int, int)>();

            // ラインごとに処理
            for (int i = 0; i < lineEvent.LinePositions.Length; i++)
            {
                IReadOnlyList<BoardIndex> line = lineEvent.LinePositions[i];

                for (int j = 0; j < line.Count; j++)
                {
                    BoardIndex index = line[j];

                    // 重複防止
                    if (deleteSet.Contains(index))
                    {
                        continue;
                    }

                    // 削除対象追加
                    deleteSet.Add(index);

                    // 列追加
                    columns.Add((index.X, index.Z));
                }
            }

            // モデル削除
            foreach (BoardIndex index in deleteSet)
            {
                _model.ClearCell(index);
            }

            // 再配置移動情報
            List<(BoardIndex from, BoardIndex to)> moves =
                new List<(BoardIndex, BoardIndex)>();

            // 再配置計算
            foreach ((int x, int z) col in columns)
            {
                IReadOnlyList<(BoardIndex from, BoardIndex to)> result =
                    _model.CalculateReposition(col.x, col.z);

                moves.AddRange(result);
            }

            // 再配置適用
            foreach ((int x, int z) col in columns)
            {
                _model.ApplyReposition(col.x, col.z);
            }

            // 再度ライン判定
            bool isLine = _model.CheckLine();

            // 結果返却
            return UniTask.FromResult(new LineResult(deleteSet, moves, isLine));
        }
    }

    /// <summary>
    /// ライン処理結果
    /// </summary>
    public readonly struct LineResult
    {
        /// <summary>削除対象</summary>
        public readonly IReadOnlyCollection<BoardIndex> DeleteSet;

        /// <summary>移動情報</summary>
        public readonly IReadOnlyList<(BoardIndex from, BoardIndex to)> Moves;

        /// <summary>再ライン成立</summary>
        public readonly bool IsLine;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LineResult(
            IReadOnlyCollection<BoardIndex> deleteSet,
            IReadOnlyList<(BoardIndex from, BoardIndex to)> moves,
            bool isLine)
        {
            // 削除対象設定
            DeleteSet = deleteSet;

            // 移動情報設定
            Moves = moves;

            // ライン状態設定
            IsLine = isLine;
        }
    }
}