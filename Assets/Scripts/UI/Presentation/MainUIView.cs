// ======================================================
// MainUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : メイン UI の描画処理を担当するビュー
// ======================================================

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UISystem.Application;
using UniRx;
using UnityEngine;

namespace UISystem.Presentation
{
    /// <summary>
    /// メイン UI ビュー
    /// </summary>
    public sealed class MainUIView : BaseUIView
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>現在スコアフォーマットクラス</summary>
        private readonly TextFormatter[] _currentScoreFormatters;

        /// <summary>加算スコアフォーマットクラス</summary>
        private readonly TextFormatter[] _addScoreFormatters;

        /// <summary>ターンフォーマットクラス</summary>
        private readonly TextFormatter _turnFormatters;

        /// <summary>コンボフォーマットクラス</summary>
        private readonly TextFormatter _comboFormatter;

        /// <summary>時間フォーマットクラス</summary>
        private readonly TextFormatter _timeFormatter;

        /// <summary>プレイヤー毎のスコアアニメーション制御クラス配列</summary>
        private NumberAnimationController[] _scoreAnimationControllers;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>現在スコアを表示するテキスト</summary>
        private readonly TextMeshProUGUI[] _currentScoreTexts;

        /// <summary>スコア加算量表示テキスト</summary>
        private readonly TextMeshProUGUI[] _addScoreTexts;

        /// <summary>ターン表示テキスト</summary>
        private readonly TextMeshProUGUI _turnText;

        /// <summary>コンボを表示するテキスト</summary>
        private readonly TextMeshPro _comboText;

        /// <summary>
        /// 制限時間表示テキスト
        /// インデックス 1 以降はエフェクト用とする
        /// </summary>
        private readonly TextMeshProUGUI[] _limitTimeTexts;

        // --------------------------------------------------
        // ターン
        // --------------------------------------------------
        /// <summary>最大ターン数</summary>
        private readonly int _maxTurnCount;

        /// <summary>ラストターン演出開始ターン数</summary>
        private readonly int _lastTurnEffectStartTurnCount;

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>現在表示中のプレイヤースコア配列</summary>
        private int[] _playerScores;

        /// <summary>ランダムアニメーション開始フラグ</summary>
        private bool _isRandomAnimationStarted = false;
        
        // --------------------------------------------------
        // コンボ
        // --------------------------------------------------
        /// <summary>最大コンボ表示数</summary>
        private readonly int _maxComboCount;

        /// <summary>コンボ表示色配列</summary>
        private readonly Color[] _comboColors;

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        /// <summary>警告開始タイミング（秒）</summary>
        private readonly int _warningLimitTime;

        /// <summary>前回表示秒数</summary>
        private int _previousDisplayTotalSeconds;

        /// <summary>ターン表示用の数値配列</summary>
        private int[] _turnValues = new int[2];

        /// <summary>コンボ表示用の数値配列</summary>
        private int[] _comboValues = new int[1];

        /// <summary>時間表示用の数値配列</summary>
        private int[] _timeValues = new int[2];

        // ======================================================
        // 定数
        // ======================================================
        
        /// <summary>現在スコア表示フォーマット</summary>
        private const string CURRENT_SCORE_FORMAT = "Score: {0}";

        /// <summary>加算スコア表示フォーマット</summary>
        private const string ADD_SCORE_FORMAT = "+ {0}";

        /// <summary>スコア桁数</summary>
        private static readonly int[] SCORE_DIGITS = { 6 };

        /// <summary>ターン表示フォーマット</summary>
        private const string TURN_FORMAT = "{0} / {1}";

        /// <summary>ターン桁数</summary>
        private static readonly int[] TURN_DIGITS = { 3, 3 };

        /// <summary>コンボ表示フォーマット</summary>
        private const string COMBO_FORMAT = "× {0}";

        /// <summary>コンボ桁数</summary>
        private static readonly int[] COMBO_DIGITS = { 2 };

        /// <summary>制限時間表示フォーマット</summary>
        private const string LIMIT_TIME_FORMAT = "{0}:{1}";

        /// <summary>制限時間桁数</summary>
        private static readonly int[] LIMIT_TIME_DIGITS = { 2, 2 };

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>コンボ表示用 Subject</summary>
        private readonly Subject<Unit> _onComboVisible = new Subject<Unit>();

        /// <summary>コンボ表示ストリーム</summary>
        public IObservable<Unit> OnComboVisible => _onComboVisible;

        /// <summary>警告表示用 Subject</summary>
        private readonly Subject<Unit> _onWarningVisible = new Subject<Unit>();

        /// <summary>警告表示ストリーム</summary>
        public IObservable<Unit> OnWarningVisible => _onWarningVisible;

        /// <summary>スコア加算アニメーション開始通知ストリーム</summary>
        public IObservable<int> OnAddScoreAnimationStarted => Observable.Merge(
            _scoreAnimationControllers.Select((controller, index) => controller.OnAnimationStarted.Select(_ => index)));

        /// <summary>スコア加算アニメーション終了通知ストリーム</summary>
        public IObservable<int> OnAddScoreAnimationFinished => Observable.Merge(
            _scoreAnimationControllers.Select((controller, index) => controller.OnAnimationFinished.Select(_ => index)));
        
        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainUIView(
            in TextMeshProUGUI[] currentScoreTexts,
            in TextMeshProUGUI[] addScoreTexts,
            in TextMeshProUGUI turnText,
            in TextMeshPro comboText,
            in TextMeshProUGUI[] limitTimeTexts,
            in int maxTurnCount,
            in int maxComboCount,
            in Color[] comboColors,
            in int warningLimitTime,
            in int lastTurnEffectStartTurnCount)
        {
            _currentScoreTexts = currentScoreTexts;
            _addScoreTexts = addScoreTexts;
            _turnText = turnText;
            _comboText = comboText;
            _limitTimeTexts = limitTimeTexts;
            _maxTurnCount = maxTurnCount;
            _maxComboCount = maxComboCount;
            _comboColors = comboColors;
            _warningLimitTime = warningLimitTime;
            _lastTurnEffectStartTurnCount = lastTurnEffectStartTurnCount;

            _previousDisplayTotalSeconds = -1;

            // --------------------------------------------------
            // スコア初期化
            // --------------------------------------------------
            _playerScores = new int[_currentScoreTexts.Length];

            if (_currentScoreTexts != null)
            {
                int length = _currentScoreTexts.Length;

                _currentScoreFormatters = new TextFormatter[length];

                for (int i = 0; i < length; i++)
                {
                    _currentScoreFormatters[i] =
                        new TextFormatter(
                            CURRENT_SCORE_FORMAT,
                            SCORE_DIGITS);
                }
            }

            if (_addScoreTexts != null)
            {
                int length = _addScoreTexts.Length;

                _addScoreFormatters = new TextFormatter[length];

                for (int i = 0; i < length; i++)
                {
                    _addScoreFormatters[i] =
                        new TextFormatter(
                            ADD_SCORE_FORMAT,
                            SCORE_DIGITS);
                }
            }

            // --------------------------------------------------
            // ターン初期化
            // --------------------------------------------------
            if (_turnText != null)
            {
                _turnFormatters =
                    new TextFormatter(
                        TURN_FORMAT,
                        TURN_DIGITS);
            }

            // --------------------------------------------------
            // コンボ初期化
            // --------------------------------------------------
            if (_comboText != null)
            {
                _comboFormatter =
                    new TextFormatter(
                        COMBO_FORMAT,
                        COMBO_DIGITS);
            }

            // --------------------------------------------------
            // タイマー初期化
            // --------------------------------------------------
            if (_limitTimeTexts != null)
            {
                _timeFormatter =
                    new TextFormatter(
                        LIMIT_TIME_FORMAT,
                        LIMIT_TIME_DIGITS);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// イベント購読
        /// </summary>
        public void Subscribe()
        {
            if (_currentScoreTexts == null)
            {
                return;
            }

            // スコア表示数取得
            int length = _currentScoreTexts.Length;

            // アニメーション制御配列生成
            _scoreAnimationControllers = new NumberAnimationController[length];

            for (int i = 0; i < length; i++)
            {
                int cacheIndex = i;

                // アニメーション制御生成
                _scoreAnimationControllers[i] = new NumberAnimationController();

                // 数値変化通知を購読
                _scoreAnimationControllers[i].CurrentValue
                    .Subscribe(score =>
                    {
                        // プレイヤースコア更新
                        _playerScores[cacheIndex] = score;

                        // スコア表示更新
                        UpdateCurrentScoreText(cacheIndex, score);
                    })
                    .AddTo(_disposables);
            }
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        public void Dispose()
        {
            _disposables?.Dispose();
            _onComboVisible?.Dispose();
            _onWarningVisible?.Dispose();

            for (int i = 0; i < _scoreAnimationControllers.Length; i++)
            {
                if (_isRandomAnimationStarted)
                {
                    _scoreAnimationControllers[i].StopRandomAnimation();
                }

                _scoreAnimationControllers[i].Dispose();
            }
        }

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// 現在スコア表示更新
        /// </summary>
        /// <param name="playerId">プレイヤーID（1 ベース）</param>
        /// <param name="score">現在スコア</param>
        public void UpdateCurrentScore(in int playerId, in int score)
        {
            // インデックス変換
            int index = playerId - 1;

            if (_scoreAnimationControllers == null ||
                index < 0 ||
                index >= _scoreAnimationControllers.Length)
            {
                return;
            }

            // アニメーション開始
            _scoreAnimationControllers[index].AnimateTo(score);
        }

        /// <summary>
        /// スコア加算量表示更新
        /// </summary>
        /// <param name="playerId">プレイヤーID（1 ベース）</param>
        /// <param name="score">加算スコア</param>
        public void UpdateAddScore(in int playerId, in int score)
        {
            // インデックス変換
            int index = playerId - 1;

            // テキスト即時更新
            UpdateAddScoreText(index, score);
        }
        
        // --------------------------------------------------
        // ターン
        // --------------------------------------------------
        /// <summary>
        /// ターン数表示更新
        /// </summary>
        public void UpdateTurnCount(in int turnCount)
        {
            if (_turnText == null)
            {
                return;
            }

            // --------------------------------------------------
            // 表示更新
            // --------------------------------------------------
            // 現在ターン数が最大ターン数より大きい場合、処理なし
            if (turnCount > _maxTurnCount)
            {
                return;
            }
            
            // ターン表示配列を更新
            _turnValues[0] = turnCount;
            _turnValues[1] = _maxTurnCount;

            // フォーマット済みバッファ取得
            char[] buffer = _turnFormatters.FormatWithPadding(_turnValues);

            // TextMeshPro に反映
            _turnText.SetCharArray(buffer);

            // --------------------------------------------------
            // スコア演出更新
            // --------------------------------------------------
            // ターン数がラストターン演出開始ターンかつ未実行の場合のみ開始
            if (turnCount >= _lastTurnEffectStartTurnCount &&
                !_isRandomAnimationStarted)
            {
                // 開始済みフラグを設定
                _isRandomAnimationStarted = true;

                // 全スコア演出開始
                for (int i = 0; i < _scoreAnimationControllers.Length; i++)
                {
                    _scoreAnimationControllers[i].StartRandomAnimation();
                }
            }
        }

        // --------------------------------------------------
        // コンボ
        // --------------------------------------------------
        /// <summary>
        /// コンボ数表示更新
        /// </summary>
        public void UpdateComboCount(in int comboCount)
        {
            if (_comboText == null)
            {
                return;
            }

            // --------------------------------------------------
            // 表示更新
            // --------------------------------------------------
            // コンボ数が 0 の場合は非表示にする
            if (comboCount == 0)
            {
                _comboText.SetText(string.Empty);

                return;
            }

            // 現在コンボ数が最大コンボ数より大きい場合、最大コンボを表示
            if (comboCount > _maxComboCount)
            {
                _comboValues[0] = _maxComboCount;
            }
            else
            {
                _comboValues[0] = comboCount;
            }

            // コンボ数に応じた色を設定
            ApplyComboColor(comboCount);

            // フォーマット済みバッファ取得
            char[] buffer = _comboFormatter.FormatWithSpacePadding(_comboValues);

            // TextMeshPro に反映
            _comboText.SetCharArray(buffer);

            // コンボ表示アニメーション通知
            _onComboVisible.OnNext(Unit.Default);
        }

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        /// <summary>
        /// 制限時間テキストの表示状態を切り替える
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        public void SetLimitTimeVisible(in bool isVisible)
        {
            if (_limitTimeTexts == null)
            {
                return;
            }

            for (int i = 0; i < _limitTimeTexts.Length; i++)
            {
                _limitTimeTexts[i].enabled = isVisible;
            }
        }

        /// <summary>
        /// 制限時間表示更新
        /// </summary>
        public void UpdateLimitTime(in float limitTime)
        {
            if (_limitTimeTexts == null)
            {
                return;
            }

            // --------------------------------------------------
            // 時間計算
            // --------------------------------------------------
            // 秒へ変換
            int totalSeconds = Mathf.CeilToInt(limitTime);

            // 同一値なら更新スキップ
            if (totalSeconds == _previousDisplayTotalSeconds)
            {
                return;
            }

            _previousDisplayTotalSeconds = totalSeconds;

            // 分計算
            int minutes = totalSeconds / 60;

            // 秒計算
            int seconds = totalSeconds % 60;

            // --------------------------------------------------
            // 表示更新
            // --------------------------------------------------
            // 時間表示配列を更新
            _timeValues[0] = minutes;
            _timeValues[1] = seconds;

            // フォーマット済みバッファ取得
            char[] buffer = _timeFormatter.FormatWithPadding(_timeValues);

            // TextMeshPro に反映
            for (int i = 0; i < _limitTimeTexts.Length; i++)
            {
                _limitTimeTexts[i].SetCharArray(buffer);
            }

            // 警告状態判定
            if (totalSeconds > 0f && totalSeconds <= _warningLimitTime)
            {
                _onWarningVisible.OnNext(Unit.Default);
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// スコアテキスト更新
        /// </summary>
        /// <param name="index">プレイヤーインデックス（0 ベース）</param>
        /// <param name="score">表示スコア</param>
        private void UpdateCurrentScoreText(in int index, in int score)
        {
            if (_currentScoreTexts == null || _currentScoreFormatters == null)
            {
                return;
            }

            // 範囲チェック
            if (index < 0 || index >= _currentScoreTexts.Length)
            {
                return;
            }

            // 対象テキスト取得
            TMP_Text targetText = _currentScoreTexts[index];

            if (targetText == null)
            {
                return;
            }

            // フォーマット済みバッファ取得
            char[] buffer = _currentScoreFormatters[index].FormatWithPadding(score);

            // TextMeshPro に反映
            targetText.SetCharArray(buffer);
        }

        /// <summary>
        /// スコアテキスト更新
        /// </summary>
        /// <param name="index">プレイヤーインデックス（0 ベース）</param>
        /// <param name="score">表示スコア</param>
        private void UpdateAddScoreText(in int index, in int score)
        {
            if (_addScoreTexts == null || _addScoreFormatters == null)
            {
                return;
            }

            // 範囲チェック
            if (index < 0 || index >= _addScoreTexts.Length)
            {
                return;
            }

            // 対象テキスト取得
            TMP_Text targetText = _addScoreTexts[index];

            if (targetText == null)
            {
                return;
            }

            // フォーマット済みバッファ取得
            char[] buffer = _addScoreFormatters[index].FormatWithSpacePadding(score);

            // TextMeshPro に反映
            targetText.SetCharArray(buffer);
        }

        // --------------------------------------------------
        // コンボ
        // --------------------------------------------------
        /// <summary>
        /// コンボ数に応じた文字色を設定
        /// </summary>
        /// <param name="comboCount">コンボ数</param>
        private void ApplyComboColor(in int comboCount)
        {
            // 色設定が存在しない場合は処理なし
            if (_comboColors == null)
            {
                return;
            }

            // 要素数が 0 の場合は処理なし
            if (_comboColors.Length == 0)
            {
                return;
            }

            // テキスト未設定の場合は処理なし
            if (_comboText == null)
            {
                return;
            }

            // コンボ数を配列インデックスへ変換
            int colorIndex = comboCount - 1;

            // 配列範囲外の場合は最後の色を使用
            if (colorIndex >= _comboColors.Length)
            {
                colorIndex = _comboColors.Length - 1;
            }

            // 念のため下限補正
            if (colorIndex < 0)
            {
                colorIndex = 0;
            }

            // 色反映
            _comboText.color = _comboColors[colorIndex];
        }
    }
}