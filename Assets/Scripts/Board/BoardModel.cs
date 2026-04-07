// ======================================================
// BoardModel.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-03-16
// 概要     : 3D 目並べゲームロジックを統括するクラス
// ======================================================

using System;
using System.Collections.Generic;
using BoardSystem.Data;
using BoardSystem.Service;
using UniRx;

namespace BoardSystem
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

        /// <summary>落下処理</summary>
        private readonly PiecePlacementService _piecePlacement;

        /// <summary>ライン判定</summary>
        private readonly LineJudgeService _lineJudge;

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
            _piecePlacement = new PiecePlacementService();
            _lineJudge = new LineJudgeService(
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
        public void CheckLine()
        {
            _lineJudge.CheckAll(_boardState);
        }

        /// <summary>
        /// 指定座標の駒を削除
        /// </summary>
        public void ClearCell(in BoardIndex index)
        {
            _boardState.ClearCell(index);
        }

        /// <summary>
        /// サブジェクト終了処理
        /// </summary>
        public void Dispose()
        {
            _lineJudge.Dispose();
        }

        /// <summary>
        /// 指定列（X,Z）の全Y値を取得
        /// </summary>
        /// <param name="columnX">列X</param>
        /// <param name="columnZ">列Z</param>
        public void GetColumnValues(
            in int columnX,
            in int columnZ)
        {
            _boardState.GetColumnValues(columnX, columnZ);
        }
    }
}