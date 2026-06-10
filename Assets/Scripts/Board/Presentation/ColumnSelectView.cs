// ======================================================
// ColumnSelectView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-10
// 更新日時 : 2026-04-10
// 概要     : 列選択表示オブジェクトの位置制御および表示切替を行うクラス
// ======================================================

using BoardSystem.Application;
using System;
using UniRx;
using UnityEngine;

namespace BoardSystem.Presentation
{
    /// <summary>
    /// 列選択ビュー
    /// </summary>
    public sealed class ColumnSelectView
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>座標変換サービス</summary>
        private readonly BoardPositionConverter _converter;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>列選択表示のルート Transform</summary>
        private readonly Transform _root;

        /// <summary>セル間隔</summary>
        private readonly float _cellSpacing;

        /// <summary>X 軸のみ移動する対象配列</summary>
        private Transform[] _frameXTargets;

        /// <summary>Z 軸のみ移動する対象配列</summary>
        private Transform[] _frameZTargets;

        /// <summary>制限なしで移動する対象配列</summary>
        private Transform[] _freeTargets;

        /// <summary>描画切替対象の Renderer 配列</summary>
        private Renderer[] _renderers;

        /// <summary>位置計算用キャッシュ</summary>
        private Vector3 _cachedPosition;

        /// <summary>前回の選択列インデックス X</summary>
        private int _previousColumnX = -1;

        /// <summary>前回の選択列インデックス Z</summary>
        private int _previousColumnZ = -1;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>FrameX タグ</summary>
        private const string TAG_FRAME_X = "FrameX";

        /// <summary>FrameZ タグ</summary>
        private const string TAG_FRAME_Z = "FrameZ";

        /// <summary>FrameNone タグ</summary>
        private const string TAG_FRAME_NONE = "FrameNone";

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>列選択表示の表示状態通知用 Subject</summary>
        private readonly ReactiveProperty<bool> _isColumnSelectVisible = new ReactiveProperty<bool>(false);

        /// <summary>列選択表示の表示状態通知ストリーム</summary>
        public IReadOnlyReactiveProperty<bool> IsColumnSelectVisible => _isColumnSelectVisible;

        /// <summary>選択列変更通知用 Subject</summary>
        private readonly Subject<Unit> _onSelectColumnChanged = new Subject<Unit>();

        /// <summary>選択列変更通知ストリーム</summary>
        public IObservable<Unit> OnSelectColumnChanged => _onSelectColumnChanged;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="root">列選択表示のルート</param>
        /// <param name="converter">座標変換サービス</param>
        /// <param name="cellSpacing">セル間隔</param>
        public ColumnSelectView(
            Transform root,
            BoardPositionConverter converter,
            float cellSpacing)
        {
            _root = root;
            _converter = converter;
            _cellSpacing = cellSpacing;

            Initialize();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 列選択表示の可視状態を切り替える
        /// </summary>
        /// <param name="isVisible">表示状態</param>
        public void SetVisible(in bool isVisible)
        {
            // 既に同じ状態の場合は処理しない
            if (_isColumnSelectVisible.Value == isVisible)
            {
                return;
            }

            // 表示状態を更新する
            _isColumnSelectVisible.Value = isVisible;

            if (_renderers == null)
            {
                return;
            }

            // 全 Renderer の有効状態を切り替える
            for (int i = 0; i < _renderers.Length; i++)
            {
                // 対象Rendererを取得する
                Renderer renderer = _renderers[i];

                if (renderer == null)
                {
                    continue;
                }

                // 描画の ON / OFF を設定する
                renderer.enabled = isVisible;
            }
        }

        /// <summary>
        /// 列選択表示の位置を更新する
        /// </summary>
        /// <param name="hitPosition">ヒットしたワールド座標</param>
        public void UpdatePosition(in Vector3 hitPosition)
        {
            // --------------------------------------------------
            // ワールド座標 → 列インデックス変換
            // --------------------------------------------------
            // 列インデックスを算出する
            _converter.WorldPositionToColumn(
                _cellSpacing,
                hitPosition.x,
                hitPosition.z,
                out int x,
                out int z
            );

            // --------------------------------------------------
            // 列インデックス → ワールド座標変換
            // --------------------------------------------------
            // 入力座標をキャッシュにコピーする
            _cachedPosition = hitPosition;

            // スナップ後のワールド座標を算出する
            _converter.ColumnToWorldPosition(
                _cellSpacing,
                x,
                0,
                z,
                out _cachedPosition.x,
                out _cachedPosition.y,
                out _cachedPosition.z
            );

            // --------------------------------------------------
            // Transform 反映
            // --------------------------------------------------
            if (_root == null)
            {
                return;
            }

            // X 軸のみ移動対象を更新する
            for (int i = 0; i < _frameXTargets.Length; i++)
            {
                Transform target = _frameXTargets[i];

                // 現在位置を取得する
                Vector3 current = target.position;

                // X のみ更新して位置を設定する
                target.position =
                    new Vector3(
                        _cachedPosition.x,
                        0f,
                        current.z
                    );
            }

            // Z 軸のみ移動対象を更新する
            for (int i = 0; i < _frameZTargets.Length; i++)
            {
                Transform target = _frameZTargets[i];

                // 現在位置を取得する
                Vector3 current = target.position;

                // Z のみ更新して位置を設定する
                target.position =
                    new Vector3(
                        current.x,
                        0f,
                        _cachedPosition.z
                    );
            }

            // --------------------------------------------------
            // 制限なし移動対象
            // --------------------------------------------------
            // 全軸更新対象を更新する
            for (int i = 0; i < _freeTargets.Length; i++)
            {
                Transform target = _freeTargets[i];

                // X, Z を更新して位置を設定する
                target.position =
                    new Vector3(
                        _cachedPosition.x,
                        0f,
                        _cachedPosition.z
                    );
            }

            // --------------------------------------------------
            // 列インデックス変更判定
            // --------------------------------------------------
            bool isColumnChanged =
                _previousColumnX != x ||
                _previousColumnZ != z;

            // 選択列変更通知
            if (isColumnChanged)
            {
                _onSelectColumnChanged.OnNext(Unit.Default);
            }

            // 最新値を保存
            _previousColumnX = x;
            _previousColumnZ = z;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        private void Initialize()
        {
            // 子階層を含めた Renderer を取得する
            _renderers = _root.GetComponentsInChildren<Renderer>(true);

            // 直下の子オブジェクト数を取得する
            int childCount = _root.childCount;

            // 各配列の要素数カウント
            int xCount = 0;
            int zCount = 0;
            int freeCount = 0;

            // タグに応じて分類数をカウントする
            for (int i = 0; i < childCount; i++)
            {
                // 子 Transform を取得する
                Transform child = _root.GetChild(i);

                // FrameNone は対象外
                if (child.CompareTag(TAG_FRAME_NONE))
                {
                    continue;
                }

                // FrameX の場合
                if (child.CompareTag(TAG_FRAME_X))
                {
                    xCount++;
                    continue;
                }

                // FrameZ の場合
                if (child.CompareTag(TAG_FRAME_Z))
                {
                    zCount++;
                    continue;
                }

                // その他は自由移動対象
                freeCount++;
            }

            // 配列を確保する
            _frameXTargets = new Transform[xCount];
            _frameZTargets = new Transform[zCount];
            _freeTargets = new Transform[freeCount];

            // 各配列のインデックス
            int xIndex = 0;
            int zIndex = 0;
            int freeIndex = 0;

            // 各配列へ Transform を格納する
            for (int i = 0; i < childCount; i++)
            {
                // 子 Transform を取得する
                Transform child = _root.GetChild(i);

                // FrameNone は対象外
                if (child.CompareTag(TAG_FRAME_NONE))
                {
                    continue;
                }

                // FrameX の場合
                if (child.CompareTag(TAG_FRAME_X))
                {
                    _frameXTargets[xIndex] = child;
                    xIndex++;
                    continue;
                }

                // FrameZ の場合
                if (child.CompareTag(TAG_FRAME_Z))
                {
                    _frameZTargets[zIndex] = child;
                    zIndex++;
                    continue;
                }

                // その他
                _freeTargets[freeIndex] = child;
                freeIndex++;
            }
        }
    }
}