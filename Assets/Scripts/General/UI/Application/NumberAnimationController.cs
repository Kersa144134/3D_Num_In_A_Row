// ======================================================
// NumberAnimationController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-05
// 更新日時 : 2026-06-05
// 概要     : 数値加算アニメーション制御クラス
// ======================================================

using System;
using Cysharp.Threading.Tasks;
using UniRx;

namespace UISystem.Application
{
    /// <summary>
    /// 数値加算アニメーション制御クラス
    /// </summary>
    public sealed class NumberAnimationController
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>目標値</summary>
        private int _targetValue;

        /// <summary>アニメーション実行中か</summary>
        private bool _isAnimating;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>現在表示値</summary>
        private readonly ReactiveProperty<int> _currentValue = new ReactiveProperty<int>(0);

        /// <summary>現在表示値</summary>
        public IReadOnlyReactiveProperty<int> CurrentValue => _currentValue;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>補間時間（秒）</summary>
        private const float INTERPOLATION_TIME = 0.75f;

        /// <summary>更新間隔（秒）</summary>
        private const float UPDATE_INTERVAL = 0.1f;

        /// <summary>最低増減量</summary>
        private const int MIN_STEP = 1;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// アニメーション開始
        /// </summary>
        /// <param name="targetValue">目標値</param>
        public void AnimateTo(in int targetValue)
        {
            // 現在値が目標値と一致している場合は処理なし
            if (_currentValue.Value == targetValue)
            {
                return;
            }

            _targetValue = targetValue;

            // 実行中の場合、目標値更新のみ更新
            if (_isAnimating)
            {
                return;
            }

            // アニメーション開始
            AnimateAsync().Forget();
        }

        /// <summary>
        /// 現在値を即時設定
        /// </summary>
        /// <param name="value">設定値</param>
        public void SetValue(in int value)
        {
            _currentValue.Value = value;
            _targetValue = value;
        }

        /// <summary>
        /// サブジェクト終了処理
        /// </summary>
        public void Dispose()
        {
            _currentValue.Dispose();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 数値補間アニメーション
        /// </summary>
        private async UniTask AnimateAsync()
        {
            // 実行開始
            _isAnimating = true;

            // 補間時間内の更新回数
            int updateCount =Math.Max(1, (int)(INTERPOLATION_TIME / UPDATE_INTERVAL));

            while (_currentValue.Value != _targetValue)
            {
                // 差分取得
                int difference = _targetValue - _currentValue.Value;

                // 加算方向
                if (difference > 0)
                {
                    // 加算量算出
                    int step = Math.Max(MIN_STEP, difference / updateCount);

                    // 値更新
                    _currentValue.Value += step;

                    // 上限補正
                    if (_currentValue.Value > _targetValue)
                    {
                        _currentValue.Value = _targetValue;
                    }
                }
                // 減算方向
                else
                {
                    // 減算量算出
                    int step = Math.Max(MIN_STEP, -difference / updateCount);

                    // 値更新
                    _currentValue.Value -= step;

                    // 下限補正
                    if (_currentValue.Value < _targetValue)
                    {
                        _currentValue.Value = _targetValue;
                    }
                }

                // タイムスケールに追従して待機
                await UniTask.Delay(TimeSpan.FromSeconds(UPDATE_INTERVAL), DelayType.DeltaTime);
            }

            // アニメーション終了
            _isAnimating = false;
        }
    }
}