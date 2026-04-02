// ======================================================
// PhasePresenter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-24
// 更新日時 : 2026-03-24
// 概要     : PhaseModel を操作してフェーズ遷移・更新を管理する Presenter
// ======================================================

using PhaseSystem.Data;

namespace PhaseSystem
{
    /// <summary>
    /// フェーズ進行管理用 Presenter
    /// </summary>
    public sealed class PhasePresenter
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>フェーズ進行管理用 Model</summary>
        private readonly PhaseModel _model = new();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>Play フェーズから Finish フェーズへ遷移するまでの時間（秒）</summary>
        private readonly float _playToFinishWaitTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ゲームプレイ経過時間の取得</summary>
        public float GamePlayElapsedTime => _model.GamePlayElapsedTime;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// PhasePresenter の生成
        /// </summary>
        /// <param name="model">Play フェーズから Finish フェーズへ遷移するまでの時間（秒）</param>
        public PhasePresenter(in float playToFinishWaitTime)
        {
            _playToFinishWaitTime = playToFinishWaitTime;
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

            // Play フェーズなら経過時間加算
            if (currentPhase == PhaseType.Play)
            {
                _model.AddElapsedTime(unscaledDeltaTime);
            }

            // --------------------------------------------------
            // フェーズ遷移判定
            // --------------------------------------------------
            if (currentPhase == PhaseType.Play &&
                _model.GamePlayElapsedTime > _playToFinishWaitTime)
            {
                targetPhase = PhaseType.Finish;
            }

            // --------------------------------------------------
            // 前フレームフェーズ更新
            // --------------------------------------------------
            _model.PreviousPhase = currentPhase;
        }
    }
}