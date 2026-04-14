// ======================================================
// PhasePresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-24
// 更新日時 : 2026-03-24
// 概要     : フェーズ進行管理用モデルを操作してフェーズ遷移・更新を管理するプレゼンター
// ======================================================

using System;
using UniRx;
using PhaseSystem.Domain;

namespace PhaseSystem.Presentation
{
    /// <summary>
    /// フェーズ進行管理用プレゼンター
    /// </summary>
    public sealed class PhasePresenter
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>モデル</summary>
        private readonly PhaseModel _model = new();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Play フェーズ配列</summary>
        private readonly PhaseType[] _playPhases;

        /// <summary>Ready フェーズから Play フェーズへ遷移するまでの時間（秒）</summary>
        private readonly float _readyToPlayWaitTime;
        
        /// <summary>Play フェーズから Finish フェーズへ遷移するまでの時間（秒）</summary>
        private readonly float _playToFinishWaitTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ゲームプレイ経過時間の取得</summary>
        public float GamePlayElapsedTime => _model.GamePlayElapsedTime;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>残り時間更新用 Subject<</summary>
        private readonly Subject<LimitTimeEvent> _onLimitTimeUpdated = new Subject<LimitTimeEvent>();

        /// <summary>残り時間更新ストリーム</summary>
        public IObservable<LimitTimeEvent> OnLimitTimeUpdated => _onLimitTimeUpdated;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// PhasePresenter の生成
        /// </summary>
        /// <param name="model">Play フェーズから Finish フェーズへ遷移するまでの時間（秒）</param>
        public PhasePresenter(
            in float readyToPlayWaitTime,
            in float playToFinishWaitTime)
        {
            _readyToPlayWaitTime = readyToPlayWaitTime;
            _playToFinishWaitTime = playToFinishWaitTime;

            // Play フェーズをキャッシュ
            _playPhases = new[]
            {
                PhaseType.Play_1,
                PhaseType.Play_2
            };
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ進行の更新処理
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale 影響なしの経過時間</param>
        /// <param name="currentPhase">現在フェーズ</param>
        /// <param name="targetPhase">遷移先フェーズ</param>
        public void Update(
            in float unscaledDeltaTime,
            in PhaseType currentPhase,
            out PhaseType targetPhase)
        {
            targetPhase = currentPhase;

            // 現フェーズのステート取得
            IPhaseState currentState = _model.GetState(currentPhase);
            if (currentState == null)
            {
                return;
            }

            // --------------------------------------------------
            // フェーズ切替判定
            // --------------------------------------------------
            if (currentPhase != _model.PreviousPhase)
            {
                // 前フェーズ終了処理
                IPhaseState prevState = _model.GetState(_model.PreviousPhase);
                prevState?.OnExit();

                // Ready フェーズ開始時にゲームプレイ時間リセット
                if (currentPhase == PhaseType.Ready)
                {
                    _model.ResetElapsedTime();
                }

                // 現フェーズ開始処理
                currentState.OnEnter();
            }

            // --------------------------------------------------
            // フェーズ更新処理
            // --------------------------------------------------
            currentState.OnUpdate(unscaledDeltaTime);

            // Play フェーズか判定
            bool isPlayPhase = false;

            for (int i = 0; i < _playPhases.Length; i++)
            {
                if (currentPhase == _playPhases[i])
                {
                    isPlayPhase = true;
                    break;
                }
            }

            if (isPlayPhase)
            {
                _model.AddElapsedTime(unscaledDeltaTime);

                _onLimitTimeUpdated.OnNext(
                    new LimitTimeEvent(
                        _model.GamePlayElapsedTime,
                        _playToFinishWaitTime));
            }

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------
            // Ready → Play
            if (currentPhase == PhaseType.Ready &&
                currentState.ElapsedTime > _readyToPlayWaitTime)
            {
                targetPhase = PhaseType.Play_1;
            }

            // Play → Finish
            if (currentPhase != PhaseType.Finish &&
                _model.GamePlayElapsedTime > _playToFinishWaitTime)
            {
                targetPhase = PhaseType.Finish;
            }

            // 前フレームフェーズを更新
            _model.SetPreviousPhase(currentPhase);
        }
    }
}