// ======================================================
// MainUIPresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : メインシーンで使用される UI 演出を管理するプレゼンター
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using InputSystem;
using PhaseSystem.Domain;
using UpdateSystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// メインシーンにおける UI 演出を管理するプレゼンター
    /// </summary>
    [UpdatableBind(UpdatableType.MainUIPresenter)]
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

        /// <summary>アウトゲーム関連のキャンバス</summary>
        [SerializeField]
        private Animator _outgameCanvasAnimator;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>ビュー</summary>
        private MainUIView _mainUIView;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>Ready パラメータ名</summary>
        private static readonly int IS_READY_HASH = Animator.StringToHash("IsReady");

        /// <summary>PlayerID パラメータ名</summary>
        private static readonly int IS_PLAYER_ID_HASH = Animator.StringToHash("IsPlayerID");

        /// <summary>Pause パラメータ名</summary>
        private static readonly int IS_PAUSE_HASH = Animator.StringToHash("IsPause");

        /// <summary>SwitchProjection パラメータ名</summary>
        private static readonly int IS_SWITCH_PROJECTION_HASH = Animator.StringToHash("IsSwitchProjection");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>フェーズ購読保持</summary>
        private IDisposable _phaseSubscription;

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

        protected override void OnPhaseEnterInternal(in PhaseType phase) { }

        protected override void OnPhaseExitInternal(in PhaseType phase) { }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // フェーズ
        // --------------------------------------------------
        /// <summary>
        /// フェーズ変更ストリームを購読し、現在のフェーズに応じて入力の有効・無効を制御する
        /// </summary>
        /// <param name="stream">フェーズ種別を通知するストリーム</param>
        public void BindPhaseStream(in IObservable<PhaseType> stream)
        {
            // 多重購読防止
            _phaseSubscription?.Dispose();

            _phaseSubscription = stream
                .Subscribe(phase =>
                {
                    // Ready
                    bool isReady = phase == PhaseType.Ready;

                    SetReadyState(isReady);

                    // Play
                    bool isPlay = phase == PhaseType.Play;

                    SetLimitTimeVisible(isPlay);
                    SetPointerVisible(isPlay);

                    // Pause
                    bool isPause = phase == PhaseType.Pause;
                    SetPauseState(isPause);

                    // Event
                    bool isEvent = phase == PhaseType.Event;

                    if (!isEvent)
                    {
                        SetSwitchProjection(false);
                    }
                });
        }

        /// <summary>
        /// フェーズ変更ストリームの購読を解除する
        /// </summary>
        public void UnbindPhaseStream()
        {
            _phaseSubscription?.Dispose();
            _phaseSubscription = null;
        }
        
        // --------------------------------------------------
        // タイマー
        // --------------------------------------------------
        /// <summary>
        /// 制限時間テキストの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        private void SetLimitTimeVisible(in bool isVisible)
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
        private void SetPointerVisible(in bool isVisible)
        {
            _mainUIView.SetPointerVisible(isVisible);
        }

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        /// <summary>
        /// Ready 状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="isReady">Ready 状態の場合はtrue</param>
        private void SetReadyState(in bool isReady)
        {
            if (_outgameCanvasAnimator == null)
            {
                return;
            }

            _outgameCanvasAnimator.SetBool(IS_READY_HASH, isReady);
        }

        /// <summary>
        /// ChangePlayer 状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="playerId">プレイヤーインデックス</param>
        public void SetChangePlayerState(in int playerId)
        {
            if (_intermittentCanvasAnimator == null)
            {
                return;
            }

            _intermittentCanvasAnimator.SetInteger(IS_PLAYER_ID_HASH, playerId);
        }

        /// <summary>
        /// Pause 状態アニメーターの状態を切り替える
        /// </summary>
        /// <param name="isPause">Pause 状態の場合はtrue</param>
        private void SetPauseState(in bool isPause)
        {
            if (_outgameCanvasAnimator == null)
            {
                return;
            }

            _outgameCanvasAnimator.SetBool(IS_PAUSE_HASH, isPause);
        }

        /// <summary>
        /// 投影方式を切り替える
        /// </summary>
        /// <param name="isSwitch">true:透視 / false:平行</param>
        public void SetSwitchProjection(in bool isSwitch)
        {
            if (_effectAnimator == null)
            {
                return;
            }

            _effectAnimator.SetBool(IS_SWITCH_PROJECTION_HASH, isSwitch);
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