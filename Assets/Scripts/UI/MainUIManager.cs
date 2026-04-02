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
using InputSystem;
using PhaseSystem.Data;
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

        /// <summary>直前に表示した残り秒数を保持する</summary>
        private int _previousDisplayTotalSeconds = -1;

        /// <summary>親 Canvas の RectTransform をキャッシュ</summary>
        private RectTransform _canvasRect;

        /// <summary>ポインターの RectTransform をキャッシュ</summary>
        private RectTransform _pointerRect;

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

            if (_pointerImage != null)
            {
                _pointerRect = _pointerImage.rectTransform;

                Canvas canvas = _pointerImage.GetComponentInParent<Canvas>();
                if (canvas != null)
                {
                    _canvasRect = canvas.transform as RectTransform;
                }
            }
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (_pointerImage != null)
            {
                if (_pointerRect == null || _canvasRect == null)
                {
                    return;
                }

                // InputManager からスクリーン座標取得
                Vector2 screenPos = InputManager.Instance != null
                    ? InputManager.Instance.Pointer
                    : Vector2.zero;

                // Canvas の中心を原点とした座標に変換
                Vector2 anchoredPos = screenPos - (_canvasRect.sizeDelta * 0.5f);

                // ポインター UI へ反映
                _pointerRect.anchoredPosition = anchoredPos;
            }
        }

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnPhaseEnterInternal(in PhaseType phase)
        {
        }

        protected override void OnPhaseExitInternal(in PhaseType phase)
        {
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 経過時間と制限時間から残り時間を計算し、UI に表示する
        /// </summary>
        /// <param name="remainingTime">残り時間（秒）</param>
        public void UpdateLimitTimeDisplay(in float remainingTime)
        {
            if (_limitTimeText == null)
            {
                return;
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