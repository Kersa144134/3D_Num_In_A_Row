// ======================================================
// BoardDropHandler.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : 駒の落下表示処理を担当するクラス
// ======================================================

using Cysharp.Threading.Tasks;
using BoardSystem.Domain;

namespace BoardSystem.Presentation
{
    /// <summary>
    /// 駒落下処理クラス
    /// </summary>
    public sealed class BoardDropHandler
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>盤面モデル</summary>
        private readonly BoardModel _model;

        /// <summary>盤面モデル</summary>
        private readonly BoardView _view;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BoardDropHandler(
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