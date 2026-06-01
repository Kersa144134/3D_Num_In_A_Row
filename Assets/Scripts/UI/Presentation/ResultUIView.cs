// ======================================================
// ResultUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-29
// 更新日時 : 2026-05-29
// 概要     : リザルト UI の描画処理を担当するビュー
// ======================================================

using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using ScoreSystem.Domain;
using ShaderSystem.Application;
using UISystem.Application;

namespace UISystem.Presentation
{
    /// <summary>
    /// リザルト UI ビュー
    /// </summary>
    public sealed class ResultUIView : BaseUIView
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>画面中心からの残像エフェクト制御クラス</summary>
        private readonly RadialAfterimageEffectController _radialAfterimage;

        /// <summary>プレイヤー ID フォーマットクラス</summary>
        private TextFormatter[] _playerIdFormatters;

        /// <summary>スコアフォーマットクラス</summary>
        private TextFormatter[] _scoreFormatters;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>ランキングのプレイヤー ID 表示用テキスト</summary>
        private readonly TextMeshProUGUI[] _rankingPlayerIdTexts;

        /// <summary>ランキングのスコア表示用テキスト</summary>
        private readonly TextMeshProUGUI[] _rankingScoreTexts;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>1st プレイヤー ID 表示フォーマット</summary>
        private const string FIRST_PLAYER_ID_FORMAT = "1st: {0}P";

        /// <summary>2nd プレイヤー ID 表示フォーマット</summary>
        private const string SECOND_PLAYER_ID_FORMAT = "2nd: {0}P";

        /// <summary>3rd プレイヤー ID 表示フォーマット</summary>
        private const string THIRD_PLAYER_ID_FORMAT = "3rd: {0}P";

        /// <summary>4th プレイヤー ID 表示フォーマット</summary>
        private const string FOURTH_PLAYER_ID_FORMAT = "4th: {0}P";

        /// <summary>プレイヤー ID 桁数</summary>
        private static readonly int[] PLAYER_ID_DIGIT = { 1 };

        /// <summary>スコア表示フォーマット</summary>
        private const string SCORE_FORMAT = "Score: {0}";

        /// <summary>スコア桁数</summary>
        private static readonly int[] SCORE_DIGIT = { 6 };

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ResultUIView(
            in RawImage radialAfterimageRenderer,
            in TextMeshProUGUI[] rankingPlayerIdTexts,
            in TextMeshProUGUI[] rankingScoreTexts)
        {
            _rankingPlayerIdTexts = rankingPlayerIdTexts;
            _rankingScoreTexts = rankingScoreTexts;

            _radialAfterimage = new RadialAfterimageEffectController(radialAfterimageRenderer.material);

            // --------------------------------------------------
            // スコア初期化
            // --------------------------------------------------
            if (_rankingPlayerIdTexts == null || _rankingScoreTexts == null)
            {
                return;
            }

            if (_rankingPlayerIdTexts.Length != _rankingScoreTexts.Length)
            {
                return;
            }

            int length = _rankingPlayerIdTexts.Length;

            // 配列生成
            _playerIdFormatters = new TextFormatter[length];
            _scoreFormatters = new TextFormatter[length];

            // 1 位 ～ 4 位までの順位表示フォーマットを配列化
            string[] playerIdFormats =
            {
                FIRST_PLAYER_ID_FORMAT,
                SECOND_PLAYER_ID_FORMAT,
                THIRD_PLAYER_ID_FORMAT,
                FOURTH_PLAYER_ID_FORMAT
            };

            for (int i = 0; i < length; i++)
            {
                // --------------------------------------------------
                // プレイヤー ID フォーマッタ生成
                // --------------------------------------------------
                _playerIdFormatters[i] =
                    new TextFormatter(
                        playerIdFormats[i],
                        PLAYER_ID_DIGIT);

                // --------------------------------------------------
                // スコアフォーマッタ生成
                // --------------------------------------------------
                _scoreFormatters[i] =
                    new TextFormatter(
                        SCORE_FORMAT,
                        SCORE_DIGIT);
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // エフェクト
        // --------------------------------------------------
        /// <summary>
        /// 画面中心からの残像エフェクト更新
        /// </summary>
        public void UpdateRadialAfterimageEffect(
            in float propEffectStrength,
            in float propSampleCount,
            in float propScaleStep,
            in float propScaleSpeed,
            in float propAlphaFadeDistance)
        {
            _radialAfterimage.Update(
                propEffectStrength,
                propSampleCount,
                propScaleStep,
                propScaleSpeed,
                propAlphaFadeDistance
            );
        }

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// ランキング情報を基にスコア表示と順位表示を更新する
        /// </summary>
        /// <param name="ranking">ランキングデータ（スコア昇順）</param>
        public void UpdateRankingInfo(List<RankingData> ranking)
        {
            if (_rankingPlayerIdTexts == null ||
                _rankingScoreTexts == null ||
                _playerIdFormatters == null ||
                _scoreFormatters == null)
            {
                return;
            }

            ranking = new List<RankingData>
            {
                new RankingData(2, 500),
                new RankingData(1, 100)
            };

            // テキスト数の取得
            int viewLength = _rankingPlayerIdTexts.Length;

            // ランキングデータ件数の取得
            int dataLength = ranking != null
                ? ranking.Count
                : 0;
            
            for (int i = 0; i < viewLength; i++)
            {
                // --------------------------------------------------
                // テキスト表示制御
                // --------------------------------------------------
                // プレイヤー人数が 4 人に満たない場合、未使用テキストを非表示
                if (i >= dataLength)
                {
                    _rankingPlayerIdTexts[i].enabled = false;
                    _rankingScoreTexts[i].enabled = false;
                    continue;
                }

                _rankingPlayerIdTexts[i].enabled = true;
                _rankingScoreTexts[i].enabled = true;

                // --------------------------------------------------
                // データ取得
                // --------------------------------------------------
                int playerId = ranking[i].PlayerId;
                int score = ranking[i].Score;

                // --------------------------------------------------
                // プレイヤー ID 表示更新
                // --------------------------------------------------
                char[] idBuffer = _playerIdFormatters[i].FormatWithPadding(playerId);
                _rankingPlayerIdTexts[i].SetCharArray(idBuffer);

                // --------------------------------------------------
                // スコア表示更新
                // --------------------------------------------------
                char[] scoreBuffer = _scoreFormatters[i].FormatWithPadding(score);
                _rankingScoreTexts[i].SetCharArray(scoreBuffer);
            }
        }
    }
}