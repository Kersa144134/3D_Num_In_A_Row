//// ======================================================
//// LineDeleteService.cs
//// 作成者   : 高橋一翔
//// 作成日時 : 2026-04-07
//// 更新日時 : 2026-04-07
//// 概要     : ライン成立時の駒削除および再配置対象列の収集を行うサービス
//// ======================================================

//using System.Collections.Generic;
//using Cysharp.Threading.Tasks;
//using UnityEngine;
//using BoardSystem;
//using BoardSystem.Data;

//namespace BoardSystem.Application
//{
//    /// <summary>
//    /// ライン削除処理サービス
//    /// </summary>
//    public sealed class LineDeleteService
//    {
//        // ======================================================
//        // 定数
//        // ======================================================

//        /// <summary>ライン削除前の待機時間（ミリ秒）</summary>
//        private const int LINE_DELETE_DELAY_MS = 500;

//        /// <summary>各駒削除のインターバル（ミリ秒）</summary>
//        private const int PIECE_DELETE_DELAY_MS = 100;

//        // ======================================================
//        // フィールド
//        // ======================================================

//        /// <summary>盤面モデル参照</summary>
//        private readonly BoardModel _model;

//        /// <summary>盤面ビュー参照</summary>
//        private readonly BoardView _view;

//        // ======================================================
//        // コンストラクタ
//        // ======================================================

//        /// <summary>
//        /// コンストラクタ
//        /// </summary>
//        public LineDeleteService(BoardModel model, BoardView view)
//        {
//            // モデル参照を保持（盤面データ更新のために使用）
//            _model = model;

//            // ビュー参照を保持（駒の削除演出に使用）
//            _view = view;
//        }

//        // ======================================================
//        // パブリックメソッド
//        // ======================================================

//        /// <summary>
//        /// ライン削除処理を実行し、再配置対象列を返す
//        /// </summary>
//        public async UniTask<List<(int x, int z)>> ExecuteAsync(LineCompleteEvent lineEvent)
//        {
//            // ライン成立直後の演出待機（プレイヤーに成立を認識させるため）
//            await UniTask.Delay(LINE_DELETE_DELAY_MS);

//            // 再配置対象列を一意に管理するための集合
//            HashSet<(int x, int z)> columnSet = new HashSet<(int, int)>();

//            // 全ラインを走査（複数ライン同時成立に対応）
//            for (int i = 0; i < lineEvent.LinePositions.Length; i++)
//            {
//                // 1ライン分のインデックス配列を取得
//                IReadOnlyList<BoardIndex> line = lineEvent.LinePositions[i];

//                // ライン上の各セルを順に処理
//                for (int j = 0; j < line.Count; j++)
//                {
//                    // 対象セルのインデックスを取得
//                    BoardIndex index = line[j];

//                    // ビュー上に駒が存在するか確認（安全性確保）
//                    if (!_view.HasPiece(index))
//                    {
//                        // 不整合検知ログ（デバッグ用途）
//                        Debug.LogWarning($"LineDeleteService: 駒が存在しません ({index.X}, {index.Y}, {index.Z})");
//                        continue;
//                    }

//                    // 駒ごとの削除演出間隔を設ける（視覚的な順次消去演出）
//                    await UniTask.Delay(PIECE_DELETE_DELAY_MS);

//                    // ビュー上の駒オブジェクトを破棄（見た目の削除）
//                    _view.DestroyPiece(index);

//                    // ビューの管理辞書から削除（参照整合性維持）
//                    _view.RemovePiece(index);

//                    // モデルのセル情報をクリア（ロジック上の削除）
//                    _model.ClearCell(index);

//                    // 再配置対象列として記録（重複はHashSetで排除）
//                    columnSet.Add((index.X, index.Z));
//                }
//            }

//            // HashSetをリストへ変換して返却（呼び出し側で順序付き処理可能にする）
//            return new List<(int, int)>(columnSet);
//        }
//    }
//}