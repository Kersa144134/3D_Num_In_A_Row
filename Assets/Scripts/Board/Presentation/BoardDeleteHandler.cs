// ======================================================
// BoardDeleteHandler.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-17
// 更新日時 : 2026-04-17
// 概要     : ライン成立時の削除表示処理を担当するクラス
// ======================================================

using BoardSystem.Domain;
using BoardSystem.Presentation;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

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
            // 全ライン共通の削除対象一覧
            HashSet<BoardIndex> allDeleteSet = new HashSet<BoardIndex>();

            // 全ライン共通の再配置対象列一覧
            HashSet<(int x, int z)> allColumns = new HashSet<(int, int)>();

            // ライン単位で保持する削除リスト
            List<List<BoardIndex>> lineDeleteLists = new List<List<BoardIndex>>();

            // ======================================================
            // 全ラインのデータ収集
            // ======================================================
            for (int i = 0; i < lineEvents.Count; i++)
            {
                // 現在処理中のラインイベント
                LineCompleteEvent lineEvent = lineEvents[i];

                // 現在ライン専用の削除対象
                HashSet<BoardIndex> lineDeleteSet = new HashSet<BoardIndex>();

                // --------------------------------------------------
                // ライン構成セル走査
                // --------------------------------------------------
                for (int j = 0; j < lineEvent.LinePositions.Count; j++)
                {
                    // ライン座標一覧取得
                    IReadOnlyList<BoardIndex> line =
                        lineEvent.LinePositions[j];

                    for (int k = 0; k < line.Count; k++)
                    {
                        // 対象インデックス取得
                        BoardIndex index = line[k];

                        // 既に現在ラインへ登録済みの場合はスキップ
                        if (lineDeleteSet.Contains(index))
                        {
                            continue;
                        }

                        // ピース存在確認
                        if (_view.TryGetPiece(index, out PieceData piece) == false)
                        {
                            // 存在しない場合は対象外
                            continue;
                        }

                        // 中心座標計算用にTransform登録
                        _centerPositionCalculator.AddPosition(piece.Transform);

                        // ライン削除対象へ追加
                        lineDeleteSet.Add(index);

                        // 全体削除対象へ追加
                        allDeleteSet.Add(index);

                        // 再配置対象列登録
                        allColumns.Add((index.X, index.Z));
                    }
                }

                // --------------------------------------------------
                // 中心座標通知
                // --------------------------------------------------
                // ライン中心座標計算
                Vector3 centerPosition =
                    _centerPositionCalculator.CalculateCenterPosition();

                // 中心座標通知
                _onCenterPositionCalculated.OnNext(centerPosition);

                // ライン単位削除リストとして保持
                lineDeleteLists.Add(new List<BoardIndex>(lineDeleteSet));
            }

            // ======================================================
            // 発光演出
            // ======================================================
            for (int i = 0; i < lineDeleteLists.Count; i++)
            {
                // 現在ラインの削除一覧取得
                List<BoardIndex> deleteList = lineDeleteLists[i];

                // --------------------------------------------------
                // ピース単位発光演出
                // --------------------------------------------------
                for (int j = 0; j < deleteList.Count; j++)
                {
                    // 対象インデックス取得
                    BoardIndex index = deleteList[j];

                    // 発光演出適用
                    _view.SetPieceEmissionColor(index);

                    // ピース単位待機
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(PIECE_DELETE_DELAY));
                }

                // --------------------------------------------------
                // ライン単位待機
                // --------------------------------------------------
                await UniTask.Delay(
                    TimeSpan.FromSeconds(LINE_DELETE_DELAY));
            }

            // ======================================================
            // 全削除処理
            // ======================================================
            // 同一フレーム削除を保証するため1回だけ待機
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
            return new LineDeleteResult(
                new List<(int x, int z)>(allColumns));
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