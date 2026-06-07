// ======================================================
// BoardDeleteHandler.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : ライン成立時の削除表示処理を担当するクラス
// ======================================================

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
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

        /// <summary>ピース中心座標計算ユーティリティ</summary>
        private readonly PiecesCenterPositionCalculator _centerPositionCalculator
            = new PiecesCenterPositionCalculator();

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>中心座標算出通知用 Subject</summary>
        private readonly Subject<Vector3> _onCenterPositionCalculated = new Subject<Vector3>();

        /// <summary>中心座標算出ストリーム</summary>
        public IObservable<Vector3> OnCenterPositionCalculated => _onCenterPositionCalculated;

        /// <summary>ライン発光開始通知用 Subject</summary>
        private readonly Subject<Unit> _onLineEmissionStarted = new Subject<Unit>();

        /// <summary>ライン発光開始ストリーム</summary>
        public IObservable<Unit> OnLineEmissionStarted => _onLineEmissionStarted;
        
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ライン削除前の待機時間</summary>
        private const float LINE_DELETE_DELAY = 0.6f;

        /// <summary>駒削除インターバル</summary>
        private const float PIECE_DELETE_DELAY = 0.2f;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
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
        /// <param name="lineEvents">成立したラインイベント一覧</param>
        /// <returns>再配置対象となる列情報をまとめた結果</returns>
        public async UniTask<LineDeleteResult> HandleLineDeleteAsync(
            IReadOnlyList<LineCompleteEvent> lineEvents)
        {
            // 削除対象ハッシュセット
            HashSet<BoardIndex> allDeleteSet = new HashSet<BoardIndex>();

            // 再配置対象列ハッシュセット
            HashSet<(int x, int z)> allColumns = new HashSet<(int, int)>();

            // ======================================================
            // 発光演出
            // ======================================================
            for (int i = 0; i < lineEvents.Count; i++)
            {
                // 現在処理中のラインイベント
                LineCompleteEvent lineEvent = lineEvents[i];

                /// 発光対象リスト
                List<BoardIndex> lineEmissionList = new List<BoardIndex>();
                /// 重複チェック用ハッシュセット
                HashSet<BoardIndex> lineEmissionSet = new HashSet<BoardIndex>();

                // --------------------------------------------------
                // ライン構成セル走査
                // --------------------------------------------------
                for (int j = 0; j < lineEvent.LinePositions.Count; j++)
                {
                    BoardIndex index = lineEvent.LinePositions[j];

                    // 既に登録済みの場合はスキップ
                    if (lineEmissionSet.Contains(index))
                    {
                        continue;
                    }

                    // 駒存在確認
                    if (_view.TryGetPiece(index, out PieceData piece) == false)
                    {
                        continue;
                    }

                    // 中心座標計算用に Transform 登録
                    _centerPositionCalculator.AddPosition(piece.Transform);

                    // 発光対象へ追加
                    lineEmissionList.Add(index);
                    lineEmissionSet.Add(index);

                    // 削除対象へ追加
                    allDeleteSet.Add(index);

                    // 再配置対象列登録
                    allColumns.Add((index.X, index.Z));
                }

                // --------------------------------------------------
                // 中心座標通知
                // --------------------------------------------------
                Vector3 centerPosition = _centerPositionCalculator.CalculateCenterPosition();

                _onCenterPositionCalculated.OnNext(centerPosition);

                // --------------------------------------------------
                // 発光演出
                // --------------------------------------------------
                _onLineEmissionStarted.OnNext(Unit.Default);

                foreach (BoardIndex index in lineEmissionList)
                {
                    _view.SetPieceEmissionColor(index);

                    // 駒単位で待機
                    await UniTask.Delay(TimeSpan.FromSeconds(PIECE_DELETE_DELAY));
                }

                // ラインイベント単位で待機
                await UniTask.Delay(TimeSpan.FromSeconds(LINE_DELETE_DELAY));
            }

            // ======================================================
            // 削除処理
            // ======================================================
            // 1 フレーム待機
            await UniTask.Yield(PlayerLoopTiming.Update);

            // 全削除対象をリスト化
            List<BoardIndex> allDeleteList =
                new List<BoardIndex>(allDeleteSet);

            for (int i = 0; i < allDeleteList.Count; i++)
            {
                // 対象インデックス取得
                BoardIndex index = allDeleteList[i];

                // ビュー上に存在しない場合はスキップ
                if (_view.TryGetPiece(index, out _) == false)
                {
                    continue;
                }

                // ビューオブジェクト削除
                _view.DestroyPieceObject(index);

                // ビューデータ削除
                _view.RemovePiece(index);

                // モデルセル削除
                _model.ClearCell(index);
            }

            // ======================================================
            // 結果返却
            // ======================================================
            return new LineDeleteResult(new List<(int x, int z)>(allColumns));
        }

        /// <summary>
        /// サブジェクト終了処理
        /// </summary>
        public void Dispose()
        {
            _onCenterPositionCalculated?.Dispose();
        }
    }
}