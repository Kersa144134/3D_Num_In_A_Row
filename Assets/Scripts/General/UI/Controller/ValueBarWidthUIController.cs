// ======================================================
// ValueBarWidthUIController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-01-19
// 更新日時 : 2026-01-19
// 概要     : 値変化に応じてUIバーの横幅を追従倍率指定補間で制御する
// ======================================================

using UnityEngine;
using UnityEngine.UI;

namespace UISystem.Controller
{
    /// <summary>
    /// 最大値・現在値・差分バーの横幅を
    /// 追従倍率指定の線形補間で制御する UI コントローラー
    /// </summary>
    public sealed class ValueBarWidthUIController
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>値 1 あたりの横幅</summary>
        private const float WIDTH_PER_VALUE = 10.0f;

        /// <summary>低速時の追従倍率</summary>
        private const float FOLLOW_RATE_SLOW = 0.4f;

        /// <summary>高速時の追従倍率</summary>
        private const float FOLLOW_RATE_FAST = 0.1f;

        /// <summary>補間終了とみなす最小距離</summary>
        private const float INTERPOLATION_COMPLETE_THRESHOLD = 0.5f;

        /// <summary>低速時の最低移動速度（1秒あたり）</summary>
        private const float MIN_SPEED_SLOW_PER_SECOND = 10.0f;

        /// <summary>高速時の最低移動速度（1秒あたり）</summary>
        private const float MIN_SPEED_FAST_PER_SECOND = 50.0f;

        // ======================================================
        // UI フィールド
        // ======================================================

        /// <summary>最大値バー</summary>
        private readonly Image _maxBar;

        /// <summary>現在値バー</summary>
        private readonly Image _currentBar;

        /// <summary>差分バー</summary>
        private readonly Image _diffBar;

        // ======================================================
        // 状態管理フィールド
        // ======================================================

        /// <summary>現在の最大値</summary>
        private float _currentMaxValue;

        /// <summary>現在の値</summary>
        private float _currentValue;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// UI バー制御クラスを生成し初期値のみを保持する
        /// </summary>
        public ValueBarWidthUIController(
            in Image maxBar,
            in Image currentBar,
            in Image diffBar,
            in float initialMaxValue,
            in float initialCurrentValue)
        {
            // UI 参照を保持する
            _maxBar = maxBar;
            _currentBar = currentBar;
            _diffBar = diffBar;

            // 初期値を内部状態として保持する
            _currentMaxValue = initialMaxValue;
            _currentValue = initialCurrentValue;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 値変化を通知する
        /// </summary>
        public void NotifyValueChanged(
            in float newMaxValue,
            in float newCurrentValue)
        {
            // 最新の値を内部に保持する
            _currentMaxValue = newMaxValue;
            _currentValue = newCurrentValue;
        }

        /// <summary>
        /// 毎フレーム呼び出して補間処理を進行させる
        /// </summary>
        public void Update(in float deltaTime)
        {
            // --------------------------------------------------
            // 最大値バー
            // --------------------------------------------------

            // 最大値バーの目標 Width を算出する
            float targetMaxWidth =
                _currentMaxValue * WIDTH_PER_VALUE;

            // 最大値バーの現在 Width を取得する
            float currentMaxWidth =
                _maxBar.rectTransform.sizeDelta.x;

            // 最大値バーは常に高速追従
            float nextMaxWidth =
                MoveTowardsByFollowRate(
                    currentMaxWidth,
                    targetMaxWidth,
                    FOLLOW_RATE_FAST,
                    MIN_SPEED_FAST_PER_SECOND,
                    deltaTime);

            // --------------------------------------------------
            // 現在値バー
            // --------------------------------------------------

            // 現在値バーの目標 Width を算出する
            float targetCurrentWidth =
                _currentValue * WIDTH_PER_VALUE;

            // 現在値バーの現在 Width を取得する
            float currentCurrentWidth =
                _currentBar.rectTransform.sizeDelta.x;

            // 最大値・現在値が同時に減少しているか判定する
            bool isMaxDecreasing =
                targetMaxWidth < currentMaxWidth;

            bool isCurrentDecreasing =
                targetCurrentWidth < currentCurrentWidth;

            float nextCurrentWidth;

            if (isMaxDecreasing && isCurrentDecreasing)
            {
                // 同時減少時は現在値バーを停止させる
                nextCurrentWidth =
                    currentCurrentWidth;
            }
            else
            {
                // 上昇・減少方向を判定する
                bool isIncreasing =
                    targetCurrentWidth > currentCurrentWidth;

                // 追従倍率を決定する
                float followRate =
                    isIncreasing
                        ? FOLLOW_RATE_SLOW
                        : FOLLOW_RATE_FAST;

                // 最低移動速度を決定する
                float minSpeedPerSecond =
                    isIncreasing
                        ? MIN_SPEED_SLOW_PER_SECOND
                        : MIN_SPEED_FAST_PER_SECOND;

                nextCurrentWidth =
                    MoveTowardsByFollowRate(
                        currentCurrentWidth,
                        targetCurrentWidth,
                        followRate,
                        minSpeedPerSecond,
                        deltaTime);
            }

            // --------------------------------------------------
            // 差分バー
            // --------------------------------------------------

            // 差分バーの現在 Width を取得する
            float currentDiffWidth =
                _diffBar.rectTransform.sizeDelta.x;

            float nextDiffWidth;

            if (isMaxDecreasing && isCurrentDecreasing)
            {
                // 同時減少時は差分バーも停止させる
                nextDiffWidth =
                    currentDiffWidth;
            }
            else
            {
                // 差分バーの上昇・減少方向を判定する
                bool isDiffIncreasing =
                    targetCurrentWidth > currentDiffWidth;

                // 現在値バーと逆の追従倍率を使用する
                float followRate =
                    isDiffIncreasing
                        ? FOLLOW_RATE_FAST
                        : FOLLOW_RATE_SLOW;

                // 現在値バーと逆の最低移動速度を使用する
                float minSpeedPerSecond =
                    isDiffIncreasing
                        ? MIN_SPEED_FAST_PER_SECOND
                        : MIN_SPEED_SLOW_PER_SECOND;

                nextDiffWidth =
                    MoveTowardsByFollowRate(
                        currentDiffWidth,
                        targetCurrentWidth,
                        followRate,
                        minSpeedPerSecond,
                        deltaTime);
            }

            // --------------------------------------------------
            // 最大値による制限処理
            // --------------------------------------------------

            // 現在値バーが最大値バーを超えないよう制限する
            nextCurrentWidth =
                Mathf.Min(
                    nextCurrentWidth,
                    nextMaxWidth);

            // 差分バーが最大値バーを超えないよう制限する
            nextDiffWidth =
                Mathf.Min(
                    nextDiffWidth,
                    nextMaxWidth);

            // --------------------------------------------------
            // RectTransform 反映
            // --------------------------------------------------

            SetWidth(_maxBar, nextMaxWidth);
            SetWidth(_currentBar, nextCurrentWidth);
            SetWidth(_diffBar, nextDiffWidth);
        }

        // ======================================================
        // 共通補間処理
        // ======================================================

        /// <summary>
        /// 残距離に対する追従倍率と最低速度で目標値へ近づける
        /// </summary>
        private float MoveTowardsByFollowRate(
            in float current,
            in float target,
            in float followRate,
            in float minSpeedPerSecond,
            in float deltaTime)
        {
            // 残り距離を算出する
            float distance =
                Mathf.Abs(target - current);

            // 十分近い場合は目標値にスナップする
            if (distance <= INTERPOLATION_COMPLETE_THRESHOLD)
            {
                return target;
            }

            // 追従倍率による移動量を算出する
            float followStep =
                distance * (deltaTime / followRate);

            // 最低移動速度による移動量を算出する
            float minStep =
                minSpeedPerSecond * deltaTime;

            // 実際に使用する移動量を決定する
            float step =
                Mathf.Max(
                    followStep,
                    minStep);

            // 安全な線形補間で値を更新する
            return
                Mathf.MoveTowards(
                    current,
                    target,
                    step);
        }

        // ======================================================
        // 共通処理
        // ======================================================

        /// <summary>
        /// Image の横幅のみを更新する
        /// </summary>
        private void SetWidth(
            in Image image,
            in float width)
        {
            // RectTransform を取得する
            RectTransform rectTransform =
                image.rectTransform;

            // 横幅のみを更新する
            rectTransform.sizeDelta =
                new Vector2(
                    width,
                    rectTransform.sizeDelta.y);
        }
    }
}