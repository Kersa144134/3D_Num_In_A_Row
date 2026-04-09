// ======================================================
// BoardModel.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-04-07
// 概要     : 3D 目並べゲームロジックを統括するクラス
// ======================================================

using System;
using System.Collections.Generic;
using UniRx;

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

        /// <summary>ライン成立イベント</summary>
        public IObservable<LineCompleteEvent> OnLineComplete
        {
            get
            {
                return _lineJudge != null
                    ? _lineJudge.OnLineComplete
                    : Observable.Empty<LineCompleteEvent>();
            }
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// モデル生成
        /// </summary>
        public BoardModel(
            in int boardSize,
            in int connectCount)
        {
            int safeConnect = connectCount;

            if (safeConnect > boardSize)
            {
                safeConnect = boardSize;
            }

            _boardState = new BoardState(boardSize);
            _lineJudge = new LineJudge(
                boardSize,
                safeConnect
            );
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定列の配置計算処理
        /// </summary>
        public int CalculatePlace(
            in int columnX,
            in int columnZ)
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
        public void ApplyPlace(
            in BoardIndex index,
            in int player)
        {
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
        public void ApplyReposition(
            in int columnX,
            in int columnZ)
        {
            _piecePlacement.ApplyReposition(
                _boardState,
                columnX,
                columnZ
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
            _boardState.ClearCell(index);
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
            // 盤面回転処理を実行し、移動情報を取得
            IReadOnlyList<(BoardIndex from, BoardIndex to)> moves =
                _boardRotate.Rotate90(
                    _boardState,
                    axis,
                    direction
                );

            return moves;
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