// ======================================================
// BoardDeleteHandler.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : ライン成立時の削除表示処理を担当するクラス
// ======================================================

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using BoardSystem.Domain;
using BoardSystem.Presentation;

namespace BoardSystem.Application
{
    /// <summary>
    /// ライン削除ユースケース
    /// </summary>
    public sealed class BoardDeleteHandler
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>盤面モデル</summary>
        private readonly BoardModel _model;

        /// <summary>盤面ビュー</summary>
        private readonly BoardView _view;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ライン削除前の待機時間</summary>
        private const int LINE_DELETE_DELAY_MS = 600;

        /// <summary>駒削除インターバル</summary>
        private const int PIECE_DELETE_DELAY_MS = 200;

        // ======================================================
        // コンストラクタ
        // ======================================================

        public BoardDeleteHandler(
            in BoardModel model,
            in BoardView view)
        {
            _model = model;
            _view = view;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ライン削除処理を実行
        /// </summary>
        public async UniTask<LineDeleteResult> HandleLineDeleteAsync(
            LineCompleteEvent lineEvent)
        {
            // 削除対象集合
            HashSet<BoardIndex> deleteSet = new HashSet<BoardIndex>();

            // 再配置対象列
            HashSet<(int x, int z)> columnSet = new HashSet<(int, int)>();

            // --------------------------------------------------
            // Emission演出
            // --------------------------------------------------
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

                    PieceData piece;

                    // Viewに存在しない場合スキップ
                    if (_view.TryGetPiece(index, out piece) == false)
                    {
                        continue;
                    }

                    // 発光演出
                    _view.SetPieceEmissionColor(index);

                    // 削除対象追加
                    deleteSet.Add(index);

                    // 再配置対象列追加
                    columnSet.Add((index.X, index.Z));

                    // 演出待機
                    await UniTask.Delay(PIECE_DELETE_DELAY_MS);
                }
            }

            // 演出待機
            await UniTask.Delay(LINE_DELETE_DELAY_MS);

            // --------------------------------------------------
            // 削除処理
            // --------------------------------------------------
            foreach (BoardIndex index in deleteSet)
            {
                if (_view.TryGetPiece(index, out _) == false)
                {
                    continue;
                }

                _view.DestroyPieceObject(index);
                _view.RemovePiece(index);
                _model.ClearCell(index);
            }

            // --------------------------------------------------
            // 結果返却
            // --------------------------------------------------
            // HashSet → List へ変換
            List<(int x, int z)> resultList =
                new List<(int, int)>(columnSet);

            return new LineDeleteResult(resultList);
        }
    }
}