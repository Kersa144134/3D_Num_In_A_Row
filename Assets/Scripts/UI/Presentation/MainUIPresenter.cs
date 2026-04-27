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
using InputSystem.Presentation;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
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
        // スコア
        // --------------------------------------------------
        [Header("スコア")]
        /// <summary>スコアを表示するテキスト</summary>
        [SerializeField]
        private TextMeshProUGUI[] _scoreTexts;

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

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

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

        /// <summary>投影切り替え用 Subject</summary>
        private readonly Subject<bool> _onSwitchProjection = new Subject<bool>();

        /// <summary>投影切り替えストリーム</summary>
        public IObservable<bool> OnSwitchProjection => _onSwitchProjection;

        /// <summary>フェーズ購読</summary>
        private IDisposable _phaseSubscription;

        /// <summary>ライン成立購読</summary>
        private IDisposable _lineCompleteSubscription;

        /// <summary>制限時間状態購読</summary>
        private IDisposable _limitTimeSubscription;

        /// <summary>入力ロック状態購読</summary>
        private IDisposable _inputLockSubscription;

        /// <summary>プレイヤーインデックス購読</summary>
        private IDisposable _playerIndexSubscription;

        /// <summary>回転入力購読</summary>
        private IDisposable _rotationSubscription;

        // ======================================================
        // IUpdatable 派生イベント
        // ======================================================

        protected override void OnEnterInternal()
        {
            base.OnEnterInternal();

            // ビュー生成
            _mainUIView =
                new MainUIView(
                    _scoreTexts,
                    _limitTimeText,
                    _pointerImage);

            // InputManager のインスタンスを取得
            _inputManager = InputManager.Instance;
        }

        protected override void OnLateUpdateInternal(in float unscaledDeltaTime)
        {
            base.OnLateUpdateInternal(unscaledDeltaTime);

            if (_isInputLock)
            {
                return;
            }
            
            // ポインター取得
            Vector2 screenPos = _inputManager.Pointer;

            // ビューへ反映
            _mainUIView.UpdatePointer(screenPos);
        }

        protected override void OnPhaseEnterInternal(in PhaseType phase) { }

        protected override void OnPhaseExitInternal(in PhaseType phase) { }

        protected override void OnExitInternal()
        {
            base.OnExitInternal();

            // イベント購読解除
            UnbindPhaseStream();
            UnbindLineCompleteStream();
            UnbindLimitTimeStream();
            UnbindInputLockStream();
            UnbindPlayerChangeStream();
            UnbindRotateStream();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ変更ストリームを購読し、現在のフェーズに応じて入力の有効・無効を制御する
        /// </summary>
        /// <param name="phase">フェーズ種別を通知するストリーム</param>
        public void BindPhaseStream(in IObservable<PhaseType> phase)
        {
            // 多重購読防止
            _phaseSubscription?.Dispose();

            _phaseSubscription = phase
                .Subscribe(type =>
                {
                    // Ready
                    bool isReady = type == PhaseType.Ready;

                    SetReadyState(isReady);

                    // Play
                    bool isPlay = type == PhaseType.Play;

                    SetLimitTimeVisible(isPlay);
                    SetPointerVisible(isPlay);

                    // Pause
                    bool isPause = type == PhaseType.Pause;
                    SetPauseState(isPause);

                    // Event
                    bool isEvent = type == PhaseType.Event;

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

        /// <summary>
        /// ライン成立イベントストリームを購読し、スコア表示を更新する
        /// </summary>
        /// <param name="stream">スコア更新イベントストリーム</param>
        public void BindLineCompleteStream(in IObservable<ScoreEvent> stream)
        {
            // 多重購読防止
            _lineCompleteSubscription?.Dispose();

            _lineCompleteSubscription = stream
                .Subscribe(e =>
                {
                    _mainUIView.UpdateScore(e.PlayerId, e.LineLength);
                });
        }

        /// <summary>
        /// ライン成立イベント購読を解除する
        /// </summary>
        public void UnbindLineCompleteStream()
        {
            _lineCompleteSubscription?.Dispose();
            _lineCompleteSubscription = null;
        }

        /// <summary>
        /// 制限時間ストリームを購読し、UI表示を更新する
        /// </summary>
        /// <param name="limitTime">制限時間を通知するストリーム</param>
        public void BindLimitTimeStream(in IObservable<float> limitTime)
        {
            // 多重購読防止
            _limitTimeSubscription?.Dispose();

            _limitTimeSubscription = limitTime
                .Subscribe(time =>
                {
                    UpdateLimitTimeDisplay(time);
                });
        }

        /// <summary>
        /// 制限時間ストリームの購読を解除する
        /// </summary>
        public void UnbindLimitTimeStream()
        {
            _limitTimeSubscription?.Dispose();
            _limitTimeSubscription = null;
        }

        /// <summary>
        /// 入力ロック状態を購読する
        /// </summary>
        /// <param name="input">true:ロック / false:解除</param>
        public void BindInputLockStream(in IObservable<bool> input)
        {
            // 多重購読防止
            _inputLockSubscription?.Dispose();

            _inputLockSubscription = input
                .Subscribe(isLock =>
                {
                    // 入力ロック状態を更新
                    _isInputLock = isLock;
                });
        }

        /// <summary>
        /// 入力ロック状態ストリームの購読を解除する
        /// </summary>
        public void UnbindInputLockStream()
        {
            _inputLockSubscription?.Dispose();
            _inputLockSubscription = null;
        }

        /// <summary>
        /// プレイヤー変更ストリームを購読し、現在のプレイヤーインデックスを更新する
        /// </summary>
        /// <param name="player">プレイヤーインデックスを通知するストリーム</param>
        public void BindPlayerChangeStream(in IObservable<int> player)
        {
            // 多重購読防止
            _playerIndexSubscription?.Dispose();

            _playerIndexSubscription = player
                .Subscribe(player =>
                {
                    SetChangePlayerState(player);
                });
        }

        /// <summary>
        /// プレイヤー変更ストリームの購読を解除する
        /// </summary>
        public void UnbindPlayerChangeStream()
        {
            _playerIndexSubscription?.Dispose();
            _playerIndexSubscription = null;
        }

        /// <summary>
        /// 回転入力ストリームを購読する
        /// </summary>
        /// <param name="rotate">回転コマンド通知ストリーム</param>
        public void BindRotateStream(in IObservable<Unit> rotate)
        {
            // 多重購読防止
            _rotationSubscription?.Dispose();

            _rotationSubscription = rotate
                .Subscribe(_ =>
                {
                    SetSwitchProjection(true);
                });
        }

        /// <summary>
        /// 回転入力ストリームの購読を解除する
        /// </summary>
        public void UnbindRotateStream()
        {
            _rotationSubscription?.Dispose();
            _rotationSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// スコア表示更新
        /// </summary>
        /// <param name="playerId">プレイヤーID（1 ベース）</param>
        /// <param name="score">スコア</param>
        private void UpdateScore(in int playerId, in int score)
        {
            _mainUIView.UpdateScore(1, 10);
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
        private void UpdateLimitTimeDisplay(in float limitTime)
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
        private void SetChangePlayerState(in int playerId)
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
        private void SetSwitchProjection(in bool isSwitch)
        {
            if (_effectAnimator == null)
            {
                return;
            }

            _effectAnimator.SetBool(IS_SWITCH_PROJECTION_HASH, isSwitch);
        }

        // ======================================================
        // アニメーションイベントメソッド
        // ======================================================

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