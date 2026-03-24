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

        /// <summary>
        /// 盤面状態
        /// </summary>
        private readonly BoardState _boardState;

        /// <summary>
        /// 落下処理
        /// </summary>
        private readonly ColumnDropService _columnDrop;

        /// <summary>
        /// ライン判定
        /// </summary>
        private readonly LineJudgeService _lineJudge;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>ライン成立イベント</summary>
        public IObservable<LineCompleteEvent> OnLineComplete
        {
            get
            {
                // モデル未生成時は空ストリームを返す
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
            _columnDrop = new ColumnDropService();
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
        public bool CanDrop(
            in int x,
            in int z)
        {
            return _columnDrop.CanDrop(
                _boardState,
                x,
                z
            );
        }

        /// <summary>
        /// 落下処理
        /// </summary>
        public int Drop(
            in int x,
            in int z,
            in int player)
        {
            return _columnDrop.Drop(
                _boardState,
                x,
                z,
                player
            );
        }

        /// <summary>
        /// 勝利判定
        /// </summary>
        public void CheckLine()
        {
            _lineJudge.CheckAll(_boardState);
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