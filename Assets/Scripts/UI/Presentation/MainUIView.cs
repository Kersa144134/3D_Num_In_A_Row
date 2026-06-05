// ======================================================
// MainUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : メイン UI の描画処理を担当するビュー
// ======================================================

using System;
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

        /// <summary>スコアフォーマットクラス</summary>
        private TextFormatter[] _scoreFormatters;

        /// <summary>ターンフォーマットクラス</summary>
        private TextFormatter _turnFormatters;

        /// <summary>コンボフォーマットクラス</summary>
        private TextFormatter _comboFormatter;

        /// <summary>時間フォーマットクラス</summary>
        private TextFormatter _timeFormatter;

        // ======================================================
        // フィールド
        // ======================================================

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>スコア表示テキスト</summary>
        private readonly TextMeshProUGUI[] _scoreTexts;

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
        // スコア
        // --------------------------------------------------
        /// <summary>前回表示スコア</summary>
        private int[] _previousDisplayScores;

        // --------------------------------------------------
        // ターン
        // --------------------------------------------------
        /// <summary>最大ターン数</summary>
        private readonly int _maxTurnCount;

        // --------------------------------------------------
        // コンボ
        // --------------------------------------------------
        /// <summary>最大コンボ表示数</summary>
        private readonly int _maxComboCount;

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

        /// <summary>スコア表示フォーマット</summary>
        private const string SCORE_FORMAT = "Score: {0}";

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

        /// <summary>警告表示用 Subject</summary>
        private readonly Subject<Unit> _onWarningVisible = new Subject<Unit>();

        /// <summary>警告表示ストリーム</summary>
        public IObservable<Unit> OnWarningVisible => _onWarningVisible;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainUIView(
            in TextMeshProUGUI[] scoreTexts,
            in TextMeshProUGUI turnText,
            in TextMeshPro comboText,
            in TextMeshProUGUI[] limitTimeTexts,
            in int maxTurnCount,
            in int maxComboCount,
            in int warningLimitTime)
        {
            _scoreTexts = scoreTexts;
            _turnText = turnText;
            _comboText = comboText;
            _limitTimeTexts = limitTimeTexts;
            _maxTurnCount = maxTurnCount;
            _maxComboCount = maxComboCount;
            _warningLimitTime = warningLimitTime;

            _previousDisplayTotalSeconds = -1;

            // --------------------------------------------------
            // スコア初期化
            // --------------------------------------------------
            if (_scoreTexts != null)
            {
                int length = _scoreTexts.Length;

                // scoreTexts の長さで配列生成
                _scoreFormatters = new TextFormatter[length];
                _previousDisplayScores = new int[length];

                for (int i = 0; i < length; i++)
                {
                    // 各 Text ごとにフォーマッタ生成
                    _scoreFormatters[i] =
                        new TextFormatter(
                            SCORE_FORMAT,
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
        /// サブジェクト終了処理
        /// </summary>
        public void Dispose()
        {
            _onWarningVisible?.Dispose();
        }

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// スコア表示更新
        /// </summary>
        /// <param name="playerId">プレイヤーID（1 ベース）</param>
        /// <param name="score">スコア</param>
        public void UpdateScore(in int playerId, in int score)
        {
            // --------------------------------------------------
            // インデックス変換（1 ベース → 0 ベース）
            // --------------------------------------------------
            int index = playerId - 1;

            if (_scoreTexts == null ||
                index < 0 ||
                index >= _scoreTexts.Length)
            {
                return;
            }

            TMP_Text targetText = _scoreTexts[index];

            if (targetText == null)
            {
                return;
            }

            // --------------------------------------------------
            // スコア計算
            // --------------------------------------------------
            // 同一値なら更新スキップ
            if (score == _previousDisplayScores[index])
            {
                return;
            }

            _previousDisplayScores[index] = score;

            // --------------------------------------------------
            // 表示更新
            // --------------------------------------------------
            // フォーマット済みバッファ取得
            char[] buffer = _scoreFormatters[index].FormatWithPadding(score);

            // TextMeshPro に反映
            targetText.SetCharArray(buffer);
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
            // 現在コンボ数が最大コンボ数より大きい場合、最大コンボを表示
            if (comboCount > _maxComboCount)
            {
                _comboValues[0] = _maxComboCount;
            }
            else
            {
                _comboValues[0] = comboCount;
            }

            // フォーマット済みバッファ取得
            char[] buffer = _comboFormatter.FormatWithSpacePadding(_comboValues);

            // TextMeshPro に反映
            _comboText.SetCharArray(buffer);
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
    }
}