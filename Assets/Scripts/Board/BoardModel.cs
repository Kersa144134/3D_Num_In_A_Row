// ======================================================
// BoardModel.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-16
// 更新日時 : 2026-03-16
// 概要     : 3D 目並べゲームロジックを統括するクラス
// ======================================================

using System;
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
        /// 落下可能判定
        /// </summary>
        public bool CanPlace(
            in int x,
            in int z)
        {
            return _piecePlacement.CanPlace(
                _boardState,
                x,
                z
            );
        }

        /// <summary>
        /// 落下処理
        /// </summary>
        public int Place(
            in int x,
            in int z,
            in int player)
        {
            return _piecePlacement.Place(
                _boardState,
                x,
                z,
                player
            );
        }

        /// <summary>
        /// 指定列の再配置処理
        /// </summary>
        public void Reposition(
            in int x,
            in int z)
        {
            _piecePlacement.Reposition(
                _boardState,
                x,
                z
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
    }
}