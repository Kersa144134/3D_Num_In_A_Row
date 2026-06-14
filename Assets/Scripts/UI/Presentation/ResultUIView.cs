// ======================================================
// ResultUIView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-29
// 更新日時 : 2026-05-29
// 概要     : リザルト UI の描画処理を担当するビュー
// ======================================================

using ScoreSystem.Domain;
using ShaderSystem.Application;
using System.Collections.Generic;
using TMPro;
using UISystem.Application;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

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

        /// <summary>1 位プレイヤー ID アニメーション表示フォーマットクラス</summary>
        private readonly TextFormatter _firstPlayerIdFormatter;

        /// <summary>プレイヤー ID 順位表示フォーマットクラス</summary>
        private readonly TextFormatter[] _playerIdFormatters;

        /// <summary>スコアフォーマットクラス</summary>
        private readonly TextFormatter[] _scoreFormatters;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>駒の Renderer 配列</summary>
        private readonly Renderer[] _pieceRendererArray;

        /// <summary>駒のパーティクル配列</summary>
        private readonly ParticleSystem[] _pieceParticleArray;

        /// <summary>駒のマテリアル配列</summary>
        private readonly Material[] _pieceMaterialArray;

        /// <summary>プレイヤーのカラー配列</summary>
        private readonly Color[] _playerColorArray;

        /// <summary>1 位のプレイヤー ID 表示用テキスト</summary>
        private readonly TextMeshPro _firstRankingPlayerIdText;

        /// <summary>ランキングのプレイヤー ID 表示用テキスト</summary>
        private readonly TextMeshProUGUI[] _rankingPlayerIdTexts;

        /// <summary>ランキングのスコア表示用テキスト</summary>
        private readonly TextMeshProUGUI[] _rankingScoreTexts;

        /// <summary>リザルト背景用の Renderer</summary>
        private readonly Renderer _resultBackgroundRenderer;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>1st プレイヤー ID リザルトアニメーション表示フォーマット</summary>
        private const string FIRST_PLAYER_ID_RESULT_FORMAT = "{0}P";

        /// <summary>1st プレイヤー ID 順位表示フォーマット</summary>
        private const string FIRST_PLAYER_ID_RANKING_FORMAT = "1st {0}P";

        /// <summary>2nd プレイヤー ID 順位表示フォーマット</summary>
        private const string SECOND_PLAYER_ID_RANKING_FORMAT = "2nd {0}P";

        /// <summary>3rd プレイヤー ID 順位表示フォーマット</summary>
        private const string THIRD_PLAYER_ID_RANKING_FORMAT = "3rd {0}P";

        /// <summary>4th プレイヤー ID 順位表示フォーマット</summary>
        private const string FOURTH_PLAYER_ID_RANKING_FORMAT = "4th {0}P";

        /// <summary>プレイヤー ID 桁数</summary>
        private static readonly int[] PLAYER_ID_DIGIT = { 1 };

        /// <summary>スコア表示フォーマット</summary>
        private const string SCORE_FORMAT = "Score {0}";

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
            in GameObject[] pieceObjectArray,
            in Material[] pieceMaterialArray,
            in Color[] playerColorArray,
            in TextMeshPro firstRankingPlayerIdText,
            in TextMeshProUGUI[] rankingPlayerIdTexts,
            in TextMeshProUGUI[] rankingScoreTexts,
            in Renderer resultBackgroundRenderer)
        {
            _pieceMaterialArray = pieceMaterialArray;
            _playerColorArray = playerColorArray;
            _firstRankingPlayerIdText = firstRankingPlayerIdText;
            _rankingPlayerIdTexts = rankingPlayerIdTexts;
            _rankingScoreTexts = rankingScoreTexts;
            _resultBackgroundRenderer = resultBackgroundRenderer;

            _radialAfterimage = new RadialAfterimageEffectController(radialAfterimageRenderer.material);

            _pieceRendererArray = new Renderer[pieceObjectArray.Length];
            _pieceParticleArray = new ParticleSystem[pieceObjectArray.Length];

            for (int i = 0; i < pieceObjectArray.Length; i++)
            {
                Renderer renderer = pieceObjectArray[i].GetComponent<Renderer>();
                ParticleSystem particle = pieceObjectArray[i].GetComponentInChildren<ParticleSystem>();

                _pieceRendererArray[i] = renderer;
                _pieceParticleArray[i] = particle;
            }

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

            // --------------------------------------------------
            // 1 位プレイヤー ID アニメーションフォーマッタ生成
            // --------------------------------------------------
            _firstPlayerIdFormatter = new TextFormatter(
                FIRST_PLAYER_ID_RESULT_FORMAT,
                PLAYER_ID_DIGIT);
            
            // 1 位 ～ 4 位までの順位表示フォーマットを配列化
            string[] playerIdFormats =
            {
                FIRST_PLAYER_ID_RANKING_FORMAT,
                SECOND_PLAYER_ID_RANKING_FORMAT,
                THIRD_PLAYER_ID_RANKING_FORMAT,
                FOURTH_PLAYER_ID_RANKING_FORMAT
            };

            for (int i = 0; i < length; i++)
            {
                // --------------------------------------------------
                // プレイヤー ID 順位フォーマッタ生成
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
        /// <param name="ranking">ランキングデータ</param>
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
                new RankingData(1, 500),
                new RankingData(2, 400),
                new RankingData(3, 300),
                new RankingData(4, 200),
            };

            // 表示可能な順位数を取得
            int viewLength = _rankingPlayerIdTexts.Length;

            // ランキングデータ件数を取得
            int dataLength = ranking != null
                ? ranking.Count
                : 0;

            // ランキングデータが存在しない場合は処理なし
            if (dataLength <= 0)
            {
                return;
            }

            // 順位表示を更新
            for (int i = 0; i < viewLength; i++)
            {
                // 現在の順位に対応するデータが存在するか判定
                bool isValidRank = i < dataLength;

                // 駒表示状態を更新
                UpdatePieceRendererVisibility(i, isValidRank);

                // ランキングテキストの表示状態を更新
                UpdateRankingTextVisibility(i, isValidRank);

                // データが存在しない順位は更新しない
                if (!isValidRank)
                {
                    continue;
                }

                // 現在順位のランキングデータを取得
                RankingData rankingData = ranking[i];

                // 駒マテリアルを更新
                UpdatePieceMaterial(i, rankingData.PlayerId);

                // テキスト色を更新
                UpdateRankingTextColor(i, rankingData.PlayerId);

                // ランキング用プレイヤー ID 表示を更新
                UpdatePlayerIdRankingText(i, rankingData.PlayerId);

                // スコア表示を更新
                UpdateScoreText(i, rankingData.Score);
            }

            // 1 位プレイヤー ID を取得
            int firstPlayerId = ranking[0].PlayerId;

            // 1 位プレイヤー 表示を取得
            UpdateFirstPlayerIdText(firstPlayerId);

            // 1 位プレイヤーの色を背景へ反映
            UpdateResultBackgroundColor(firstPlayerId);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // 駒
        // --------------------------------------------------
        /// <summary>
        /// 駒 Renderer の表示状態を更新する
        /// </summary>
        /// <param name="index">順位インデックス</param>
        /// <param name="isVisible">表示状態</param>
        private void UpdatePieceRendererVisibility(in int index, in bool isVisible)
        {
            if (_pieceRendererArray == null)
            {
                return;
            }

            if (index >= _pieceRendererArray.Length)
            {
                return;
            }

            // 駒の表示状態を更新
            _pieceRendererArray[index].enabled = isVisible;

            // ParticleSystem の表示状態を更新
            ParticleSystem particle = _pieceParticleArray[index];

            if (isVisible)
            {
                particle?.Play();
            }
            else
            {
                particle?.Stop();
            }
        }

        /// <summary>
        /// 順位に対応する駒マテリアルを更新する
        /// </summary>
        /// <param name="rankIndex">順位インデックス</param>
        /// <param name="playerId">プレイヤー ID</param>
        private void UpdatePieceMaterial(in int rankIndex, in int playerId)
        {
            if (_pieceRendererArray == null || _pieceMaterialArray == null)
            {
                return;
            }

            if (rankIndex >= _pieceRendererArray.Length)
            {
                return;
            }

            // プレイヤー ID をインデックスへ変換
            int materialIndex = playerId - 1;

            if (materialIndex < 0 || materialIndex >= _pieceMaterialArray.Length)
            {
                return;
            }

            _pieceRendererArray[rankIndex].material = _pieceMaterialArray[materialIndex];
        }

        // --------------------------------------------------
        // テキスト
        // --------------------------------------------------
        /// <summary>
        /// ランキングテキストの表示状態を更新する
        /// </summary>
        /// <param name="index">順位インデックス</param>
        /// <param name="isVisible">表示状態</param>
        private void UpdateRankingTextVisibility(in int index, in bool isVisible)
        {
            // プレイヤーID表示状態を更新
            _rankingPlayerIdTexts[index].enabled = isVisible;

            // スコア表示状態を更新
            _rankingScoreTexts[index].enabled = isVisible;
        }

        /// <summary>
        /// ランキングテキストの色を更新する
        /// </summary>
        /// <param name="rankIndex">順位インデックス</param>
        /// <param name="playerId">プレイヤー ID</param>
        private void UpdateRankingTextColor(in int rankIndex, in int playerId)
        {
            if (_playerColorArray == null)
            {
                return;
            }

            // プレイヤー ID をインデックスへ変換
            int colorIndex = playerId - 1;

            if (colorIndex < 0 || colorIndex >= _playerColorArray.Length)
            {
                return;
            }

            // プレイヤーカラーを取得
            Color playerColor = _playerColorArray[colorIndex];

            // プレイヤーIDテキスト色を更新
            _rankingPlayerIdTexts[rankIndex].color = playerColor;

            // スコアテキスト色を更新
            _rankingScoreTexts[rankIndex].color = playerColor;
        }

        /// <summary>
        /// プレイヤー ID 表示を更新する
        /// </summary>
        /// <param name="rankIndex">順位インデックス</param>
        /// <param name="playerId">プレイヤー ID</param>
        private void UpdatePlayerIdRankingText(in int rankIndex, in int playerId)
        {
            // 表示用文字列を生成
            char[] buffer = _playerIdFormatters[rankIndex].FormatWithPadding(playerId);

            // テキストへ反映
            _rankingPlayerIdTexts[rankIndex].SetCharArray(buffer);
        }

        /// <summary>
        /// 1 位プレイヤー ID 表示を更新する
        /// </summary>
        /// <param name="rankIndex">順位インデックス</param>
        /// <param name="playerId">プレイヤー ID</param>
        private void UpdateFirstPlayerIdText(in int playerId)
        {
            // 表示用文字列を生成
            char[] buffer = _firstPlayerIdFormatter.FormatWithPadding(playerId);

            // テキストへ反映
            _firstRankingPlayerIdText.SetCharArray(buffer);
        }

        /// <summary>
        /// スコア表示を更新する
        /// </summary>
        /// <param name="rankIndex">順位インデックス</param>
        /// <param name="score">スコア</param>
        private void UpdateScoreText(in int rankIndex, in int score)
        {
            // 表示用文字列を生成
            char[] buffer = _scoreFormatters[rankIndex].FormatWithPadding(score);

            // テキストへ反映
            _rankingScoreTexts[rankIndex].SetCharArray(buffer);
        }

        // --------------------------------------------------
        // 背景
        // --------------------------------------------------
        /// <summary>
        /// 1位プレイヤーの色を背景へ反映する
        /// </summary>
        /// <param name="playerId">1位プレイヤーID</param>
        private void UpdateResultBackgroundColor(in int playerId)
        {
            if (_resultBackgroundRenderer == null || _playerColorArray == null)
            {
                return;
            }

            // プレイヤー ID をインデックスへ変換
            int colorIndex = playerId - 1;

            if (colorIndex < 0 || colorIndex >= _playerColorArray.Length)
            {
                return;
            }

            // 背景色を更新
            _resultBackgroundRenderer.material.color = _playerColorArray[colorIndex];
        }
    }
}