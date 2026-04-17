// ======================================================
// BoardRotateUseCase.cs
// 概要 : 盤面回転と再配置計算を担当するユースケース
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using BoardSystem.Domain;

namespace BoardSystem.Application
{
    /// <summary>
    /// 盤面回転ユースケース
    /// </summary>
    public sealed class BoardRotateUseCase
    {
        /// <summary>盤面モデル</summary>
        private readonly BoardModel _model;

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardRotateUseCase(BoardModel model, int boardSize)
        {
            // モデル参照を保持
            _model = model;

            // 盤面サイズを保持
            _boardSize = boardSize;
        }

        /// <summary>
        /// 回転処理を実行
        /// </summary>
        public UniTask<RotateResult> ExecuteAsync(
            RotationAxis axis,
            RotationDirection direction)
        {
            // モデルから回転移動情報を取得
            IReadOnlyList<(BoardIndex from, BoardIndex to)> moves =
                _model.Rotate90(axis, direction);

            // 全列リスト生成
            List<(int x, int z)> columns = new List<(int, int)>();

            // 全列を走査
            for (int x = 0; x < _boardSize; x++)
            {
                for (int z = 0; z < _boardSize; z++)
                {
                    // 列を追加
                    columns.Add((x, z));
                }
            }

            // 再配置移動情報
            List<(BoardIndex from, BoardIndex to)> repositionMoves =
                new List<(BoardIndex, BoardIndex)>();

            // 各列ごとに再配置計算
            for (int i = 0; i < columns.Count; i++)
            {
                (int x, int z) col = columns[i];

                // 再配置移動取得
                IReadOnlyList<(BoardIndex from, BoardIndex to)> movesResult =
                    _model.CalculateReposition(col.x, col.z);

                // 結果を追加
                repositionMoves.AddRange(movesResult);
            }

            // モデルに再配置適用
            for (int i = 0; i < columns.Count; i++)
            {
                (int x, int z) col = columns[i];

                _model.ApplyReposition(col.x, col.z);
            }

            // ライン判定
            bool isLine = _model.CheckLine();

            // 結果返却
            return UniTask.FromResult(new RotateResult(moves, repositionMoves, isLine));
        }
    }

    /// <summary>
    /// 回転結果
    /// </summary>
    public readonly struct RotateResult
    {
        /// <summary>回転移動</summary>
        public readonly IReadOnlyList<(BoardIndex from, BoardIndex to)> RotateMoves;

        /// <summary>再配置移動</summary>
        public readonly IReadOnlyList<(BoardIndex from, BoardIndex to)> RepositionMoves;

        /// <summary>ライン成立</summary>
        public readonly bool IsLine;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RotateResult(
            IReadOnlyList<(BoardIndex from, BoardIndex to)> rotateMoves,
            IReadOnlyList<(BoardIndex from, BoardIndex to)> repositionMoves,
            bool isLine)
        {
            // 回転移動を設定
            RotateMoves = rotateMoves;

            // 再配置移動を設定
            RepositionMoves = repositionMoves;

            // ライン状態を設定
            IsLine = isLine;
        }
    }
}