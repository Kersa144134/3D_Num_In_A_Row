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
        /// <param name="lineEvents">成立したラインイベント一覧（複数ライン・複数プレイヤー対応）</param>
        /// <returns>再配置対象となる列情報をまとめた結果</returns>
        public async UniTask<LineDeleteResult> HandleLineDeleteAsync(
            IReadOnlyList<LineCompleteEvent> lineEvents)
        {
            // 全イベントを通して再配置対象となる列
            HashSet<(int x, int z)> allColumns = new HashSet<(int, int)>();

            for (int i = 0; i < lineEvents.Count; i++)
            {
                // 現在処理中のライン成立イベントを取得
                LineCompleteEvent lineEvent = lineEvents[i];

                // 現在イベント内で削除対象となるピース座標集合
                // 重複削除防止のためHashSetで管理
                HashSet<BoardIndex> deleteSet = new HashSet<BoardIndex>();

                // 現在イベント内で再配置対象となる列情報
                HashSet<(int x, int z)> columnSet = new HashSet<(int, int)>();

                // --------------------------------------------------
                // データ取得
                // --------------------------------------------------
                for (int j = 0; j < lineEvent.LinePositions.Count; j++)
                {
                    IReadOnlyList<BoardIndex> line = lineEvent.LinePositions[j];

                    for (int k = 0; k < line.Count; k++)
                    {
                        // 対象セルのインデックスを取得
                        BoardIndex index = line[k];

                        // 既に削除対象に含まれている場合はスキップ
                        if (deleteSet.Contains(index))
                        {
                            continue;
                        }

                        // ビュー上に存在するピースを取得
                        if (_view.TryGetPiece(index, out PieceData piece) == false)
                        {
                            // ピースが存在しない場合は処理対象外
                            continue;
                        }

                        // 中心座標計算用にワールド座標を登録
                        _centerPositionCalculator.AddPosition(piece.Transform);

                        // 削除対象として登録
                        deleteSet.Add(index);

                        // 再配置対象列として登録
                        columnSet.Add((index.X, index.Z));
                    }
                }

                // --------------------------------------------------
                // 中心座標計算
                // --------------------------------------------------
                // 削除対象ピース群の中心位置を算出
                Vector3 centerPosition =
                    _centerPositionCalculator.CalculateCenterPosition();

                // 中心座標通知
                _onCenterPositionCalculated.OnNext(centerPosition);

                // --------------------------------------------------
                // 発光演出
                // --------------------------------------------------
                // List へ変換してインデックスを参照
                List<BoardIndex> deleteList = new List<BoardIndex>(deleteSet);

                for (int j = 0; j < deleteList.Count; j++)
                {
                    BoardIndex index = deleteList[j];

                    // 発光エフェクト適用
                    _view.SetPieceEmissionColor(index);

                    // 1 ピース単位の演出待機
                    await UniTask.Delay(TimeSpan.FromSeconds(PIECE_DELETE_DELAY));
                }

                // ライン単位の演出待機
                await UniTask.Delay(TimeSpan.FromSeconds(LINE_DELETE_DELAY));

                // --------------------------------------------------
                // 削除処理
                // --------------------------------------------------
                for (int j = 0; j < deleteList.Count; j++)
                {
                    BoardIndex index = deleteList[j];

                    // ビュー上に存在しない場合はスキップ
                    if (_view.TryGetPiece(index, out _) == false)
                    {
                        continue;
                    }

                    // ビュー上のオブジェクト削除
                    _view.DestroyPieceObject(index);

                    // ビューからデータ削除
                    _view.RemovePiece(index);

                    // モデルのセル削除
                    _model.ClearCell(index);
                }

                // 現在イベントで発生した列情報を全体集合へ統合
                foreach ((int x, int z) c in columnSet)
                {
                    allColumns.Add(c);
                }
            }

            // 再配置対象列情報をリスト化して結果として返却
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