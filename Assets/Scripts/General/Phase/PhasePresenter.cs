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
        // フィールド
        // ======================================================

        /// <summary>操作対象の Model</summary>
        private readonly PhaseModel _model;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// PhasePresenter の生成
        /// </summary>
        /// <param name="model">管理対象の PhaseModel</param>
        public PhasePresenter(in PhaseModel model)
        {
            _model = model;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ進行の更新処理
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale 影響なしの経過時間</param>
        /// <param name="currentPhase">現在フェーズ</param>
        /// <param name="targetPhase">遷移先フェーズ（判定結果）</param>
        public void Update(
            in float unscaledDeltaTime,
            in PhaseType currentPhase,
            out PhaseType targetPhase)
        {
            // 初期値として現フェーズを設定
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
            // 現フェーズ更新処理
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
                _model.GamePlayElapsedTime > PhaseModel.PLAY_TO_FINISH_WAIT_TIME)
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