// ======================================================
// NumberAnimationController.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-05
// 更新日時 : 2026-06-05
// 概要     : 数値加算アニメーション制御クラス
// ======================================================

using System;
using System.Threading;
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

        /// <summary>アニメーション実行中フラグ</summary>
        private bool _isAnimating = false;

        /// <summary>ランダムアニメーション実行中フラグ</summary>
        private bool _isRandomAnimating = false;

        /// <summary>ランダムアニメーション停止用 CancellationTokenSource</summary>
        private CancellationTokenSource _randomAnimationCancellation;

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
        private const float INTERPOLATION_TIME = 0.5f;

        /// <summary>更新間隔（秒）</summary>
        private const float UPDATE_INTERVAL = 0.05f;

        /// <summary>最低増減量</summary>
        private const int MIN_STEP = 1;

        /// <summary>ランダムアニメーション最小値</summary>
        private const int RANDOM_MIN_VALUE = 0;

        /// <summary>ランダムアニメーション最大値</summary>
        private const int RANDOM_MAX_VALUE = 999999;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// アニメーション開始
        /// </summary>
        /// <param name="targetValue">目標値</param>
        public void AnimateTo(in int targetValue)
        {
            // ランダム演出中は処理なし
            if (_isRandomAnimating)
            {
                return;
            }

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
        /// ランダムアニメーション開始
        /// </summary>
        public void StartRandomAnimation()
        {
            // 多重実行防止
            if (_isRandomAnimating)
            {
                return;
            }

            _randomAnimationCancellation = new CancellationTokenSource();

            RandomAnimationAsync(
                RANDOM_MIN_VALUE,
                RANDOM_MAX_VALUE,
                _randomAnimationCancellation.Token).Forget();
        }

        /// <summary>
        /// ランダムアニメーション終了
        /// </summary>
        public void StopRandomAnimation()
        {
            if (!_isRandomAnimating)
            {
                return;
            }

            _randomAnimationCancellation.Cancel();
            _randomAnimationCancellation.Dispose();
            _randomAnimationCancellation = null;
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
            StopRandomAnimation();

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
            int updateCount = Math.Max(1, (int)(INTERPOLATION_TIME / UPDATE_INTERVAL));

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

        /// <summary>
        /// ランダムアニメーション
        /// </summary>
        /// <param name="minValue">最小値</param>
        /// <param name="maxValue">最大値</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        private async UniTask RandomAnimationAsync(
            int minValue,
            int maxValue,
            CancellationToken cancellationToken)
        {
            _isRandomAnimating = true;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // ランダム値生成
                    int randomValue = UnityEngine.Random.Range(
                        minValue,
                        maxValue + 1);

                    // 現在値更新
                    SetValue(randomValue);

                    // 一定時間待機
                    // タイムスケールを無視する
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(UPDATE_INTERVAL),
                        DelayType.UnscaledDeltaTime,
                        PlayerLoopTiming.Update,
                        cancellationToken
                    );
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _isRandomAnimating = false;
            }
        }
    }
}