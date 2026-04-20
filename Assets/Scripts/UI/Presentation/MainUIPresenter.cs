// ======================================================
// MainUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : メインシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using InputSystem;
using PhaseSystem.Domain;
using SceneSystem.Domain;
using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem.Presentation
{
    /// <summary>
    /// メインシーンにおける UI 演出を管理するプレゼンター
    /// </summary>
    public sealed class MainUIPresenter : BaseUIPresenter, IUpdatable
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

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        [Header("アニメーター")]
        /// <summary>連続更新対象のキャンバス</summary>
        [SerializeField]
        private Animator _continuousCanvasAnimator;

        /// <summary>断続更新対象のキャンバス</summary>
        [SerializeField]
        private Animator _intermittentCanvasAnimator;

        /// <summary>ポーズ状態のキャンバス</summary>
        [SerializeField]
        private Animator _pauseCanvasAnimator;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>ビュー</summary>
        private MainUIView _mainUIView;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ポーズパラメータ名</summary>
        private static readonly int IS_PAUSE_HASH = Animator.StringToHash("IsPause");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>投影切り替え用 Subject</summary>
        private readonly Subject<bool> _onSwitchProjection = new Subject<bool>();

        /// <summary>投影切り替えストリーム</summary>
        public IObservable<bool> OnSwitchProjection => _onSwitchProjection;
        
        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // View生成
            _mainUIView =
                new MainUIView(
                    _limitTimeText,
                    _pointerImage);
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            // Input取得
            Vector2 screenPos =
                InputManager.Instance != null
                    ? InputManager.Instance.Pointer
                    : Vector2.zero;

            // Viewへ反映
            _mainUIView.UpdatePointer(screenPos);
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

        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        /// <summary>
        /// 制限時間テキストの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        public void SetLimitTimeVisible(in bool isVisible)
        {
            _mainUIView.SetLimitTimeVisible(isVisible);
        }

        /// <summary>
        /// 制限時間を UI に表示する
        /// </summary>
        /// <param name="limitTime">残り時間（秒）</param>
        public void UpdateLimitTimeDisplay(in float limitTime)
        {
            _mainUIView.UpdateLimitTime(limitTime);
        }

        // --------------------------------------------------
        // ポインター
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        public void SetPointerVisible(in bool isVisible)
        {
            _mainUIView.SetPointerVisible(isVisible);
        }

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        /// <summary>
        /// ポーズ状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="isPause">ポーズ状態の場合はtrue</param>
        public void SetPauseState(in bool isPause)
        {
            // Animator未設定なら何もしない
            if (_pauseCanvasAnimator == null)
            {
                return;
            }

            _pauseCanvasAnimator.SetBool(IS_PAUSE_HASH, isPause);
        }

        // --------------------------------------------------
        // アニメーションイベント
        // --------------------------------------------------
        /// <summary>
        /// 投影切り替え開始イベント
        /// </summary>
        public void OnSwitchProjectionStart()
        {
            _onSwitchProjection.OnNext(true);
        }

        /// <summary>
        /// 投影切り替え終了イベント
        /// </summary>
        public void OnSwitchProjectionEnd()
        {
            _onSwitchProjection.OnNext(false);
        }
    }
}