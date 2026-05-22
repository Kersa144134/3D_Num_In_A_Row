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
using AnimationSystem.Infrastructure;
using InputSystem.Presentation;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using UISystem.Infrastructure;
using UpdateSystem.Domain;
using OptionSystem.Presentation;

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
        // キャンバス
        // --------------------------------------------------
        [Header("キャンバス")]
        /// <summary>断続更新対象のキャンバス</summary>
        [SerializeField]
        private GameObject _intermittentCanvas;

        /// <summary>アウトゲーム関連のキャンバス</summary>
        [SerializeField]
        private GameObject _outgameCanvas;

        // --------------------------------------------------
        // プレイヤー情報
        // --------------------------------------------------
        [Header("プレイヤー情報")]
        /// <summary>プレイヤー情報を表示する GameObject 配列</summary>
        [SerializeField]
        private GameObject[] _playerInfoArray;

        // --------------------------------------------------
        // 入力情報
        // --------------------------------------------------
        [Header("入力情報")]
        /// <summary>駒落下時の入力情報を表示する GameObject 配列</summary>
        [SerializeField]
        private GameObject _inputInfoPieceDrop;

        /// <summary>ボード回転時の入力情報を表示する GameObject 配列</summary>
        [SerializeField]
        private GameObject _inputInfoBoardRotation;

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
        private TextMeshProUGUI[] _limitTimeTexts;

        /// <summary>警告開始タイミング（秒）</summary>
        [SerializeField]
        private float _warningLimitTime = 5f;

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        [Header("アニメーター")]
        /// <summary>制限時間のアニメーター</summary>
        [SerializeField]
        private Animator _limitTimeAnimator;

        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>ビュー</summary>
        private MainUIView _mainUIView;

        /// <summary>断続更新対象のキャンバスのアニメーションイベント通知クラス</summary>
        private AnimationEventNotifier _intermittentCanvasAnimationEventNotifier;

        private InputIconCollector _inputIconCollector = new InputIconCollector();

        /// <summary>GameOptionManager キャッシュ</summary>
        private GameOptionManager _gameOptionManager;

        /// <summary>InputManager キャッシュ</summary>
        private InputManager _inputManager;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>入力ロックフラグ</summary>
        private bool _isInputLock = true;

        /// <summary>ポインターターゲット検出中フラグ</summary>
        private bool _isPointerTarget = false;

        /// <summary>制限時間表示中フラグ</summary>
        private bool _isLimitTimeVisible = false;

        /// <summary>警告アニメーション表示中フラグ</summary>
        private bool _isWarning;

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>ゲームパッド入力アイコン群</summary>
        private Image[] _gamepadInputIcons;

        /// <summary>仮想パッド入力アイコン群</summary>
        private Image[] _virtualpadInputIcons;

        // --------------------------------------------------
        // アニメーター
        // --------------------------------------------------
        /// <summary>断続更新対象のキャンバス</summary>
        private Animator _intermittentCanvasAnimator;

        /// <summary>アウトゲーム関連のキャンバス</summary>
        private Animator _outgameCanvasAnimator;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>Ready パラメータ名</summary>
        private static readonly int IS_READY_HASH = Animator.StringToHash("IsReady");

        /// <summary>Pause パラメータ名</summary>
        private static readonly int IS_PAUSE_HASH = Animator.StringToHash("IsPause");

        /// <summary>PlayerID パラメータ名</summary>
        private static readonly int IS_PLAYER_ID_HASH = Animator.StringToHash("IsPlayerID");

        /// <summary>IsWarning パラメータ名</summary>
        private static readonly int IS_WARNING_HASH = Animator.StringToHash("IsWarning");

        /// <summary>SwitchProjection パラメータ名</summary>
        private static readonly int IS_SWITCH_PROJECTION_HASH = Animator.StringToHash("IsSwitchProjection");

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>イベント購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        /// <summary>ChangePlayerアニメーション終了通知用 Subject</summary>
        private readonly Subject<Unit> _onChangePlayerAnimationEnd = new Subject<Unit>();

        /// <summary>ChangePlayerアニメーション終了通知ストリーム</summary>
        public IObservable<Unit> OnChangePlayerAnimationEnd => _onChangePlayerAnimationEnd;
        
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

            // インスタンスからコンポーネント取得
            _gameOptionManager = GameOptionManager.Instance;
            _inputManager = InputManager.Instance;

            if (_gameOptionManager == null ||
                _inputManager == null ||
                _intermittentCanvas == null ||
                _outgameCanvas == null)
            {
                Debug.LogError("[MainUIPresenter] クラスの初期化に失敗しました。");

#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
    UnityEngine.Application.Quit();
#endif

                return;
            }

            // ビュー生成
            _mainUIView =
                new MainUIView(
                    _scoreTexts,
                    _limitTimeTexts,
                    _pointer);

            // アニメーター取得
            _intermittentCanvasAnimator = _intermittentCanvas.GetComponent<Animator>();
            _outgameCanvasAnimator = _outgameCanvas.GetComponent<Animator>();

            // アニメーター速度をタイムスケール非依存に設定
            SetAnimatorUnscaledTime(_outgameCanvasAnimator);
            SetAnimatorUnscaledTime(_limitTimeAnimator);

            // アニメーションイベント通知クラス取得
            _intermittentCanvasAnimationEventNotifier = _intermittentCanvas.GetComponent<AnimationEventNotifier>();

            // プレイヤー情報の非表示処理
            for (int i = _gameOptionManager.PlayerCount; i < _playerInfoArray.Length; i++)
            {
                _playerInfoArray[i].SetActive(false);
            }

            // 入力アイコンの取得
            _inputIconCollector.CollectInputIcons(
                _intermittentCanvas,
                _outgameCanvas,
                out _gamepadInputIcons,
                out _virtualpadInputIcons
            );
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

        protected override void OnPhaseEnterInternal(in PhaseType phase)
        {
            base.OnPhaseEnterInternal(phase);
        }

        protected override void OnPhaseExitInternal(in PhaseType phase)
        {
            base.OnPhaseExitInternal(phase);
        }

        protected override void OnExitInternal()
        {
            base.OnExitInternal();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        /// <param name="phase">フェーズ状態を通知するストリーム</param>
        /// <param name="playerChange">プレイヤーインデックス変更を通知するストリーム</param>
        /// <param name="scoreUpdated">スコア更新を通知するストリーム</param>
        /// <param name="inputLock">入力ロック状態を通知するストリーム</param>
        /// <param name="gamepadUsed">ゲームパッド使用状態を通知するストリーム</param>
        /// <param name="columnSelectVisibleChanged">列選択表示の表示状態を通知するストリーム</param>
        /// <param name="dropRequested">落下入力予約を通知するストリーム</param>
        /// <param name="rotateRequested">回転入力予約を通知するストリーム</param>
        /// <param name="limitTime">制限時間の残り時間を通知するストリーム</param>
        public void BindStreams(
            in IObservable<PhaseType> phase,
            in IObservable<int> playerChange,
            in IObservable<ScoreEvent> scoreUpdated,
            in IObservable<bool> inputLock,
            in IObservable<bool> gamepadUsed,
            in IObservable<bool> columnSelectVisibleChanged,
            in IObservable<Unit> dropRequested,
            in IObservable<Unit> rotateRequested,
            in IObservable<float> limitTime)
        {
            phase
                .Subscribe(type =>
                {
                    // Ready
                    bool isReady = type == PhaseType.Ready;
                    SetReadyState(isReady);

                    // Play
                    bool isPlay = type == PhaseType.Play;
                    SetLimitTimeVisible(isPlay);
                    SetPointerVisible(isPlay);

                    // Event
                    bool isEvent = type == PhaseType.Event;

                    // Pause
                    bool isPause = type == PhaseType.Pause;
                    SetPauseState(isPause);

                    // ChangePlayer
                    bool isChangePlayer = type == PhaseType.ChangePlayer;

                    // 入力情報 UI 更新
                    if (isChangePlayer)
                    {
                        SetInputInfoActive(_inputInfoPieceDrop);
                    }
                })
                .AddTo(_disposables);

            playerChange
                .Subscribe(playerIndex => SetChangePlayerState(playerIndex))
                .AddTo(_disposables);

            scoreUpdated
                .Subscribe(e => UpdateScore(e.PlayerId, e.LineLength))
                .AddTo(_disposables);

            inputLock
                .Subscribe(isLock => _isInputLock = isLock)
                .AddTo(_disposables);

            gamepadUsed
                .Subscribe(isUsed => SetInputIconVisible(isUsed))
                .AddTo(_disposables);

            columnSelectVisibleChanged
                .Subscribe(isVisible =>
                {
                    _isPointerTarget = isVisible;

                    UpdatePointerTargetAnimation(isVisible);
                })
                .AddTo(_disposables);

            dropRequested
                .Subscribe(_ => SetSwitchProjection(false))
                .AddTo(_disposables);

            rotateRequested
                .Subscribe(_ =>
                {
                    SetSwitchProjection(true);

                    SetInputInfoActive(_inputInfoBoardRotation);
                })
                .AddTo(_disposables);

            limitTime
                .Subscribe(time => UpdateLimitTimeDisplay(time))
                .AddTo(_disposables);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // イベント購読
        // --------------------------------------------------
        /// <summary>
        /// イベント購読
        /// </summary>
        protected override void Subscribe()
        {
            // アニメーション終了通知
            _intermittentCanvasAnimationEventNotifier.OnAnimationEnd
                .Subscribe(_ =>
                {
                    _onChangePlayerAnimationEnd.OnNext(Unit.Default);
                })
                .AddTo(_disposables);
        }
        
        /// <summary>
        /// イベント購読解除
        /// </summary>
        protected override void Dispose()
        {
            base.Dispose();

            _disposables?.Dispose();
        }

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// スコア表示更新
        /// </summary>
        /// <param name="playerId">プレイヤー ID（1 ベース）</param>
        /// <param name="score">スコア</param>
        private void UpdateScore(in int playerId, in int score)
        {
            _mainUIView.UpdateScore(playerId, score);
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

            _isLimitTimeVisible = isVisible;
        }

        /// <summary>
        /// 制限時間を UI に表示する
        /// </summary>
        /// <param name="limitTime">残り時間（秒）</param>
        private void UpdateLimitTimeDisplay(in float limitTime)
        {
            // 非表示時は処理なし
            if (!_isLimitTimeVisible)
            {
                return;
            }

            _mainUIView.UpdateLimitTime(limitTime);

            // アニメーション更新
            UpdateWarningAnimation(limitTime);
        }

        /// <summary>
        /// 警告アニメーションの状態を更新する
        /// </summary>
        /// <param name="limitTime">残り時間（秒）</param>
        private void UpdateWarningAnimation(in float limitTime)
        {
            // 非表示時は強制解除
            if (!_isLimitTimeVisible)
            {
                SetWarningState(false);

                return;
            }

            // 警告判定
            bool isWarning = limitTime > 0f && limitTime <= _warningLimitTime;

            // 状態変化なし時は処理なし
            if (_isWarning == isWarning)
            {
                return;
            }

            SetWarningState(isWarning);
        }

        /// <summary>
        /// 警告状態を更新する
        /// </summary>
        /// <param name="isWarning">警告状態</param>
        private void SetWarningState(bool isWarning)
        {
            _isWarning = isWarning;

            _limitTimeAnimator?.SetBool(IS_WARNING_HASH, isWarning);
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// ポインターの表示状態を更新する
        /// </summary>
        /// <param name="isVisible">表示する場合はtrue</param>
        private void SetPointerVisible(in bool isVisible)
        {
            _mainUIView.SetPointerVisible(isVisible);

            UpdatePointerTargetAnimation(_isPointerTarget);
        }

        /// <summary>
        /// 入力情報 UI の表示を切り替える
        /// 指定されたオブジェクトのみ表示し、それ以外は非表示にする
        /// null の場合は全て非表示
        /// </summary>
        /// <param name="target">表示対象の入力情報UI</param>
        private void SetInputInfoActive(in GameObject target)
        {
            if (target == null)
            {
                if (_inputInfoPieceDrop != null)
                {
                    _inputInfoPieceDrop.SetActive(false);
                }

                if (_inputInfoBoardRotation != null)
                {
                    _inputInfoBoardRotation.SetActive(false);
                }

                return;
            }

            if (_inputInfoPieceDrop != null)
            {
                _inputInfoPieceDrop.SetActive(_inputInfoPieceDrop == target);
            }

            if (_inputInfoBoardRotation != null)
            {
                _inputInfoBoardRotation.SetActive(_inputInfoBoardRotation == target);
            }
        }

        /// <summary>
        /// 入力アイコンの表示切り替え
        /// </summary>
        /// <param name="isGamepadUsed">
        /// true: Gamepad 表示 / Virtualpad 非表示
        /// false: Virtualpad 表示 / Gamepad 非表示
        /// </param>
        private void SetInputIconVisible(in bool isGamepadUsed)
        {
            // Gamepad
            if (_gamepadInputIcons != null)
            {
                for (int i = 0; i < _gamepadInputIcons.Length; i++)
                {
                    _gamepadInputIcons[i].enabled = isGamepadUsed;
                }
            }

            // Virtualpad
            if (_virtualpadInputIcons != null)
            {
                for (int i = 0; i < _virtualpadInputIcons.Length; i++)
                {
                    _virtualpadInputIcons[i].enabled = !isGamepadUsed;
                }
            }
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
        // アニメーションイベント
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