// ======================================================
// BoardModel.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-07
// 概要     : 3D 目並べゲームロジックを統括するクラス
// ======================================================

using System;
using System.Collections.Generic;

namespace BoardSystem.Domain
{
    /// <summary>
    /// 目並べモデル
    /// </summary>
    public sealed class BoardModel
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>盤面状態</summary>
        private readonly BoardState _boardState;

        /// <summary>盤面回転処理</summary>
        private readonly BoardRotate _boardRotate = new BoardRotate();

        /// <summary>ライン判定</summary>
        private readonly LineJudge _lineJudge;

        /// <summary>落下処理</summary>
        private readonly PiecePlacement _piecePlacement = new PiecePlacement();

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>ライン成立ストリーム</summary>
        public IObservable<IReadOnlyList<LineCompleteEvent>> OnLineComplete => _lineJudge.OnLineComplete;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>空マスを示す値</summary>
        private const int EMPTY = 0;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// モデル生成
        /// </summary>
        public BoardModel(in int boardSize, int connectCount)
        {
            if (connectCount > boardSize)
            {
                connectCount = boardSize;
            }

            _boardState = new BoardState(boardSize);
            _lineJudge = new LineJudge(
                boardSize,
                connectCount
            );
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定列の配置計算処理
        /// </summary>
        public int CalculatePlace(in int columnX, in int columnZ)
        {
            return _piecePlacement.CalculatePlace(
                _boardState,
                columnX,
                columnZ
            );
        }

        /// <summary>
        /// 配置適用処理
        /// </summary>
        public void ApplyPlace(in BoardIndex index, in int player)
        {
            // 既に駒が存在する場合は処理なし
            if (_boardState.Get(index) != EMPTY)
            {
                return;
            }

            _piecePlacement.ApplyPlace(
                _boardState,
                index,
                player
            );
        }

        /// <summary>
        /// 指定列の再配置計算処理
        /// </summary>
        public IReadOnlyList<(BoardIndex from, BoardIndex to)> CalculateReposition(
            in int columnX,
            in int columnZ)
        {
            return _piecePlacement.CalculateReposition(
                _boardState,
                columnX,
                columnZ
            );
        }

        /// <summary>
        /// 再配置適用処理
        /// </summary>
        /// <param name="repositionMoves">再配置情報</param>
        public void ApplyReposition(in IReadOnlyList<(BoardIndex from, BoardIndex to)> repositionMoves)
        {
            _piecePlacement.ApplyReposition(
                _boardState,
                _boardState,
                repositionMoves
            );
        }

        /// <summary>
        /// ライン成立判定
        /// </summary>
        public bool CheckLine()
        {
            return _lineJudge.CheckAll(_boardState);
        }

        /// <summary>
        /// 指定座標の駒を削除
        /// </summary>
        public void ClearCell(in BoardIndex index)
        {
            _boardState.Clear(index);
        }

        /// <summary>
        /// 盤面を回転させ、移動情報を取得する
        /// </summary>
        /// <param name="axis">回転軸</param>
        /// <param name="direction">回転方向</param>
        /// <returns>移動情報（from → to）</returns>
        public IReadOnlyList<(BoardIndex from, BoardIndex to)> Rotate90(
            in RotationAxis axis,
            in RotationDirection direction)
        {
            // 回転後の盤面データを取得
            int[,,] rotatedBoard =
                _boardRotate.Rotate90(
                    _boardState,
                    axis,
                    direction,
                    out IReadOnlyList<(BoardIndex from, BoardIndex to)> rotateMoves
                );

            // 回転結果を反映
            _boardState.ApplyBoard(rotatedBoard);

            // 回転移動情報を返却
            return rotateMoves;
        }

        /// <summary>
        /// サブジェクト終了処理
        /// </summary>
        public void Dispose()
        {
            _lineJudge.Dispose();
        }
    }
}