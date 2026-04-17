// ======================================================
// BoardDropUseCase.cs
// 概要 : 駒の落下処理を担当するユースケース
// ======================================================

using Cysharp.Threading.Tasks;
using BoardSystem.Domain;

namespace BoardSystem.Presentation
{
    /// <summary>
    /// 駒落下ユースケース
    /// </summary>
    public sealed class BoardDropUseCase
    {
        /// <summary>盤面モデル</summary>
        private readonly BoardModel _model;

        /// <summary>盤面モデル</summary>
        private readonly BoardView _view;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardDropUseCase(
            BoardModel model,
            BoardView view)
        {
            _model = model;
            _view = view;
        }

        /// <summary>
        /// 駒落下処理
        /// </summary>
        public async UniTask HandleDropAsync(int x, int y, int z, int player)
        {
            // 駒生成
            PieceData piece =
                await _view.SpawnPieceAsync(
                    x,
                    y,
                    z,
                    player
                );

            // インデックス生成
            BoardIndex index = new BoardIndex(x, y, z);

            // ビューに駒情報登録
            _view.SetPiece(index, piece);

            // モデルの盤面更新
            _model.ApplyPlace(index, player);
        }
    }
}