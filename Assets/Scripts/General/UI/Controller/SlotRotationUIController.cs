// ======================================================
// SlotRotationUIController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-01-19
// 概要     : UI スロットを循環表示する汎用コントローラー
//            1回転処理とルーレット回転処理を提供する
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem.Controller
{
    public sealed class SlotRotationUIController
    {
        // ======================================================
        // 列挙体
        // ======================================================

        /// <summary>UI スロットの配置方向を定義する</summary>
        public enum LayoutDirection
        {
            Horizontal,
            Vertical
        }

        /// <summary>スロットが進行する回転方向を定義する</summary>
        public enum RotationSign
        {
            Positive,
            Negative
        }

        // ======================================================
        // 定数
        // ======================================================

        /// <summary各スロット間の表示間隔</summary>
        private const float SLOT_SPACING = 200.0f;

        /// <summary>スロットの移動速度</summary>
        private const float MOVE_SPEED = 1000.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>制御対象となる UI スロット Image 配列</summary>
        private readonly Image[] _slotImages;

        /// <summary>UI スロットの配置方向</summary>
        private readonly LayoutDirection _layoutDirection;

        /// <summary>スロットの回転方向</summary>
        private readonly RotationSign _rotationSign;

        /// <summary>各スロットが目指す最終的な目標座標パターン</summary>
        private Vector2[] _targetPositions;

        /// <summary>各スロットが参照している目標座標のインデックス</summary>
        private int[] _slotTargetIndex;

        /// <summary>ターゲット超過時にクランプ処理を一時的に無効化するためのフラグ</summary>
        private bool[] _isIgnoringClamp;

        /// <summary>現在ルーレット回転中かどうかを示すフラグ</summary>
        private bool _isRouletteRotating;

        /// <summary>初期化時に算出される最小目標座標</summary>
        private float _minTargetCoord;

        /// <summary>初期化時に算出される最大目標座標</summary>
        private float _maxTargetCoord;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// 目標座標 (0,0) を参照するスロットが確定した際に通知される
        /// </summary>
        public event Action<int> OnCenterSlotDecided;
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        public SlotRotationUIController(
            in Image[] slotImages,
            in LayoutDirection layoutDirection,
            in RotationSign rotationSign)
        {
            _slotImages = slotImages;
            _layoutDirection = layoutDirection;
            _rotationSign = rotationSign;

            int slotCount = _slotImages.Length;
            _targetPositions = new Vector2[slotCount];
            _slotTargetIndex = new int[slotCount];
            _isIgnoringClamp = new bool[slotCount];

            // 初期目標座標パターンを生成
            InitializeTargetPositions();

            // 各スロットの初期状態を設定
            for (int i = 0; i < slotCount; i++)
            {
                _slotTargetIndex[i] = i;
                _slotImages[i].rectTransform.anchoredPosition = _targetPositions[i];
                _isIgnoringClamp[i] = false;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // 1回転
        // --------------------------------------------------
        /// <summary>
        /// スロットを1ステップ分回転させる
        /// </summary>
        public void Rotate()
        {
            int slotCount = _slotImages.Length;

            // 参照インデックスを 1 ステップ分更新
            ShiftTargetIndexOneStep();

            for (int i = 0; i < slotCount; i++)
            {
                // 対象スロットの RectTransform を取得
                RectTransform rect = _slotImages[i].rectTransform;

                // 開始時点の位置関係からクランプ無効判定を更新
                EvaluateIgnoreClamp(i, rect);
            }

            // 回転結果確定を通知
            NotifyCenterSlotDecided();
        }

        // --------------------------------------------------
        // ルーレット回転
        // --------------------------------------------------
        /// <summary>
        /// ルーレット回転を開始する
        /// クランプを行わず、無限に線形移動させる
        /// </summary>
        public void StartRouletteRotation()
        {
            // ルーレット回転状態に遷移
            _isRouletteRotating = true;

            // 全スロットのクランプを無効化
            for (int i = 0; i < _slotImages.Length; i++)
            {
                _isIgnoringClamp[i] = true;
            }
        }

        /// <summary>
        /// ルーレット回転を停止する
        /// 指定された要素番号が有効な場合は、その要素が中央に来るよう確定する
        /// 無効値または指定なしの場合は 1 回転処理と同等の挙動とする
        /// </summary>
        public void StopRouletteRotation(int? centerSlotIndex = null)
        {
            // ルーレット回転状態を解除
            _isRouletteRotating = false;

            int slotCount = _slotImages.Length;

            // 指定が有効な場合
            if (centerSlotIndex.HasValue &&
                centerSlotIndex.Value >= 0 &&
                centerSlotIndex.Value < slotCount)
            {
                // 要素0が参照するターゲットを指定インデックスに設定
                _slotTargetIndex[0] = centerSlotIndex.Value;

                // 残りのスロットも順番に設定
                for (int i = 1; i < slotCount; i++)
                {
                    _slotTargetIndex[i] = (_slotTargetIndex[i - 1] + 1) % slotCount;
                }
            }
            // 指定なし or 指定が無効な場合
            else
            {
                // 参照インデックスを 1 ステップ分更新 (Rotate と同一処理)
                ShiftTargetIndexOneStep();
            }

            // クランプ判定
            for (int i = 0; i < slotCount; i++)
            {
                EvaluateIgnoreClamp(i, _slotImages[i].rectTransform);
            }

            // 回転結果確定を通知
            NotifyCenterSlotDecided();
        }

        // --------------------------------------------------
        // 補間更新
        // --------------------------------------------------
        /// <summary>
        /// 毎フレーム呼び出され、スロットの移動を制御する
        /// </summary>
        public void Update(in float deltaTime)
        {
            int slotCount = _slotImages.Length;
            float delta = MOVE_SPEED * deltaTime;

            for (int i = 0; i < slotCount; i++)
            {
                RectTransform rect = _slotImages[i].rectTransform;

                // 主軸方向の現在座標を取得
                float current = GetCurrentCoord(rect);

                float next;

                if (_isRouletteRotating)
                {
                    // 回転方向に応じた移動符号を決定
                    float directionSign =
                        (_rotationSign == RotationSign.Positive)
                            ? 1.0f
                            : -1.0f;

                    // 移動量を加算
                    next = current + (delta * directionSign);

                    // ループ判定
                    next = ApplyLoopCorrection(ref next, slotCount);
                }
                else
                {
                    // 通常回転時はターゲットへ向かう補間移動を行う
                    float target = GetTargetCoord(_slotTargetIndex[i]);
                    next = MoveAlongAxis(current, target, i, delta, slotCount);
                }

                // 計算結果を RectTransform に反映
                if (_layoutDirection == LayoutDirection.Horizontal)
                {
                    rect.anchoredPosition = new Vector2(next, 0f);
                }
                else
                {
                    rect.anchoredPosition = new Vector2(0f, next);
                }
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 初期目標座標パターンを生成する
        /// 要素0は必ず座標0となる
        /// </summary>
        private void InitializeTargetPositions()
        {
            int slotCount = _slotImages.Length;

            _targetPositions = new Vector2[slotCount];

            for (int i = 0; i < slotCount; i++)
            {
                float coord;

                // 回転方向に応じて初期配置を決定
                if (_rotationSign == RotationSign.Positive)
                {
                    coord = -SLOT_SPACING * i;
                }
                else
                {
                    coord = SLOT_SPACING * i;
                }

                // ループ判定
                coord = ApplyLoopCorrection(ref coord, slotCount);

                // 配置方向に応じてスロットを配置
                if (_layoutDirection == LayoutDirection.Horizontal)
                {
                    _targetPositions[i] = new Vector2(coord, 0f);
                }
                else
                {
                    _targetPositions[i] = new Vector2(0f, coord);
                }
            }

            // 最小／最大目標座標を初期化
            _minTargetCoord = float.MaxValue;
            _maxTargetCoord = float.MinValue;

            for (int i = 0; i < slotCount; i++)
            {
                float value = GetTargetCoord(i);

                if (value < _minTargetCoord)
                {
                    _minTargetCoord = value;
                }

                if (value > _maxTargetCoord)
                {
                    _maxTargetCoord = value;
                }
            }
        }

        /// <summary>
        /// 全スロットの参照インデックスを 1 ステップ分循環シフトする
        /// 実座標は変更せず、ターゲット参照のみを更新する
        /// </summary>
        private void ShiftTargetIndexOneStep()
        {
            int slotCount = _slotImages.Length;

            for (int i = 0; i < slotCount; i++)
            {
                // 現在の参照インデックスを 1 つ前に循環シフト
                _slotTargetIndex[i] = (_slotTargetIndex[i] - 1 + slotCount) % slotCount;
            }
        }

        /// <summary>
        /// 回転開始時点でターゲット座標をすでに超えているかを判定し
        /// クランプ無効フラグを設定する
        /// </summary>
        private void EvaluateIgnoreClamp(in int slotIndex, in RectTransform rect)
        {
            // 現在の主軸座標を取得
            float currentCoord = GetCurrentCoord(rect);

            // 次に向かうターゲットの主軸座標を取得
            float targetCoord = GetTargetCoord(_slotTargetIndex[slotIndex]);

            if ((_rotationSign == RotationSign.Positive && currentCoord > targetCoord) ||
                (_rotationSign == RotationSign.Negative && currentCoord < targetCoord))
            {
                // クランプ無効化
                _isIgnoringClamp[slotIndex] = true;
            }
            else
            {
                // クランプ有効化
                _isIgnoringClamp[slotIndex] = false;
            }
        }

        /// <summary>
        /// RectTransform から主軸方向の現在座標を取得する
        /// </summary>
        private float GetCurrentCoord(in RectTransform rect)
        {
            return (_layoutDirection == LayoutDirection.Horizontal)
                ? rect.anchoredPosition.x
                : rect.anchoredPosition.y;
        }

        /// <summary>
        /// 指定インデックスに対応するターゲット座標を取得する
        /// </summary>
        private float GetTargetCoord(in int slotIndex)
        {
            return (_layoutDirection == LayoutDirection.Horizontal)
                ? _targetPositions[slotIndex].x
                : _targetPositions[slotIndex].y;
        }

        /// <summary>
        /// 主軸方向の移動・クランプ・ループ補正をまとめて処理する
        /// </summary>
        private float MoveAlongAxis(
            in float current,
            in float target,
            in int slotIndex,
            in float delta,
            in int slotCount)
        {
            // 回転方向に応じて次の座標を計算
            float next = current + ((_rotationSign == RotationSign.Positive) ? delta : -delta);

            // クランプが有効な場合のみターゲット超過を制限
            if (!_isIgnoringClamp[slotIndex])
            {
                if ((_rotationSign == RotationSign.Positive && next > target) ||
                    (_rotationSign == RotationSign.Negative && next < target))
                {
                    next = target;
                }
            }

            // ループ補正前の値を保持
            float beforeLoop = next;

            // 表示範囲外なら反対側へループ補正
            next = ApplyLoopCorrection(ref next, slotCount);

            // ループが発生した場合はクランプ無効状態を解除
            if (!Mathf.Approximately(beforeLoop, next))
            {
                _isIgnoringClamp[slotIndex] = false;
            }

            return next;
        }

        /// <summary>
        /// 表示範囲を超えた座標を反対側へループ補正する
        /// </summary>
        private float ApplyLoopCorrection(ref float coord, in int slotCount)
        {
            // スロット全体の表示範囲の半分を算出
            float halfRange = SLOT_SPACING * slotCount / 2f;

            // 全スロット分の長さを引いて反対側へ再配置する
            if (coord > halfRange)
            {
                coord -= SLOT_SPACING * slotCount;
            }
            if (coord < -halfRange)
            {
                coord += SLOT_SPACING * slotCount;
            }

            return coord;
        }

        /// <summary>
        /// 現在の参照状態を元に中央スロット決定イベントを通知する
        /// </summary>
        private void NotifyCenterSlotDecided()
        {
            int centerIndex = GetCenterSlotIndexInternal();

            if (centerIndex >= 0)
            {
                OnCenterSlotDecided?.Invoke(centerIndex);
            }
        }

        /// <summary>
        /// 目標座標 (0,0) を参照しているスロットの要素番号を取得する
        /// </summary>
        private int GetCenterSlotIndexInternal()
        {
            int slotCount = _slotTargetIndex.Length;

            for (int i = 0; i < slotCount; i++)
            {
                float targetCoord = GetTargetCoord(_slotTargetIndex[i]);

                if (Mathf.Approximately(targetCoord, 0.0f))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}