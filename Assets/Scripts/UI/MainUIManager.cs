// ======================================================
// MainUIManager.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : メインシーンで使用される UI 演出を管理するクラス
// ======================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InputSystem.Manager;
using SceneSystem.Data;
using UISystem.Service;

namespace UISystem
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

        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        [Header("ポインター")]
        /// <summary>ポインターを表示する Image</summary>
        [SerializeField]
        private Image _pointerImage;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>タイム表示フォーマットサービス</summary>
        private TextFormatService _timeFormatService;
        
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
        private const string LIMIT_TIME_FORMAT = "{0}:{1}";

        /// <summary>
        /// 制限時間表示桁数
        /// </summary>
        private static readonly int[] LIMIT_TIME_DIGITS = { 2, 2 };

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            if (_limitTimeText != null)
            {
                // 制限時間表示フォーマットクラスを生成する
                _timeFormatService = new TextFormatService(
                    _limitTimeText, LIMIT_TIME_FORMAT, LIMIT_TIME_DIGITS);
            }
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (!_isInGame)
            {
                return;
            }

            if (_pointerImage != null)
            {
                // RectTransform を取得
                RectTransform rectTransform =
                    _pointerImage.rectTransform;

                // InputManager から現在のポインター座標を取得
                Vector2 pointerPosition =
                    InputManager.Instance.Pointer;

                // UI座標としてそのまま適用（Screen Space Overlay 前提）
                rectTransform.anchoredPosition =
                    pointerPosition;
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

            // 残り時間を算出
            float remainingTime = limitTime - elapsedTime;

            // 残り時間が負数にならないよう補正
            if (remainingTime < 0.0f)
            {
                remainingTime = 0.0f;
            }

            // 残り時間を整数へ変換（小数切り捨て）
            int totalSeconds = Mathf.FloorToInt(remainingTime);

            // 前回表示秒と同一の場合は処理なし
            if (totalSeconds == _previousDisplayTotalSeconds)
            {
                return;
            }

            // 現在の表示秒をキャッシュへ保存
            _previousDisplayTotalSeconds = totalSeconds;

            // 分を算出
            int minutes = totalSeconds / 60;

            // 秒を算出
            int seconds = totalSeconds % 60;

            // フォーマットを使用して UI に反映
            _timeFormatService.SetNumberText(new int[] { minutes, seconds });
        }
    }
}