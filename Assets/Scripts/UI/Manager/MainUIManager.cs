// ======================================================
// MainUIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : メインシーンで使用される UI 演出を管理するクラス
// ======================================================

using UnityEngine;
using TMPro;
using SceneSystem.Data;

namespace UISystem.Manager
{
    /// <summary>
    /// メインシーンにおける UI 演出およびゲーム連動 UI を管理するクラス
    /// </summary>
    public sealed class MainUIManager : BaseUIManager, IUpdatable
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [Header("メインシーン固有インスペクタ")]

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        [Header("タイマー")]
        /// <summary>制限時間を表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI _limitTimeText;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在インゲーム状態かどうか</summary>
        private bool _isInGame;

        /// <summary>直前に表示した残り秒数を保持する</summary>
        private int _previousDisplayTotalSeconds = -1;

        // ======================================================
        // 定数
        // ======================================================

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        /// <summary>
        /// 制限時間表示フォーマット
        /// </summary>
        private const string LIMIT_TIME_FORMAT = "{0:00}:{1:00}";

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (!_isInGame)
            {
                return;
            }
        }

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnPhaseEnterInternal(in PhaseType phase)
        {
            // Play フェーズ開始時にインゲーム状態
            if (phase == PhaseType.Play)
            {
                _isInGame = true;
            }
        }

        protected override void OnPhaseExitInternal(in PhaseType phase)
        {
            // Play フェーズ終了時にインゲーム状態解除
            if (phase == PhaseType.Play)
            {
                _isInGame = false;
            }
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 経過時間と制限時間から残り時間を計算し、UI に表示する
        /// </summary>
        /// <param name="elapsedTime">現在までの経過時間（秒）</param>
        /// <param name="limitTime">制限時間（秒）</param>
        public void UpdateLimitTimeDisplay(in float elapsedTime, in float limitTime)
        {
            if (_limitTimeText == null)
            {
                return;
            }

            // 残り時間を算出する
            float remainingTime = limitTime - elapsedTime;

            // 残り時間が負数にならないよう補正する
            if (remainingTime < 0.0f)
            {
                remainingTime = 0.0f;
            }

            // 残り時間を整数秒へ変換する（小数切り捨て）
            int totalSeconds = Mathf.FloorToInt(remainingTime);

            // 前回表示秒と同一の場合は処理なし
            if (totalSeconds == _previousDisplayTotalSeconds)
            {
                return;
            }

            // 現在の表示秒をキャッシュへ保存する
            _previousDisplayTotalSeconds = totalSeconds;

            // 分を算出する
            int minutes = totalSeconds / 60;

            // 秒を算出する
            int seconds = totalSeconds % 60;

            // フォーマットを使用して UI に反映
            _limitTimeText.SetText(LIMIT_TIME_FORMAT, minutes, seconds);
        }
    }
}