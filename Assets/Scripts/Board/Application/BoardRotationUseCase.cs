// ======================================================
// BoardRotationUseCase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : 盤面回転と再配置計算を担当するクラス
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using BoardSystem.Domain;

namespace BoardSystem.Application
{
    /// <summary>
    /// 盤面回転ユースケース
    /// </summary>
    public sealed class BoardRotationUseCase
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>盤面モデル</summary>
        private readonly BoardModel _model;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>盤面サイズ</summary>
        private readonly int _boardSize;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardRotationUseCase(
            in BoardModel model,
            in int boardSize)
        {
            _model = model;
            _boardSize = boardSize;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 回転処理を実行
        /// </summary>
        public UniTask<BoardRotationResult> HandleRotateAsync(
            RotationAxis axis,
            RotationDirection direction)
        {
            // モデルから回転移動情報を取得
            IReadOnlyList<(BoardIndex from, BoardIndex to)> rotateMoves =
                _model.Rotate90(
                    axis,
                    direction
                );

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
                new List<(BoardIndex from, BoardIndex to)>();

            // 各列ごとに再配置計算
            for (int i = 0; i < columns.Count; i++)
            {
                // 列取得
                (int x, int z) column = columns[i];

                // 再配置移動取得
                IReadOnlyList<(BoardIndex from, BoardIndex to)> moves =
                    _model.CalculateReposition(
                        column.x,
                        column.z
                    );

                // 結果を統合
                repositionMoves.AddRange(moves);
            }

            // 再配置適用
            _model.ApplyReposition(repositionMoves);

            // 結果返却
            return UniTask.FromResult(
                new BoardRotationResult(
                    rotateMoves,
                    repositionMoves
                )
            );
        }
    }
}