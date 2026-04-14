// ======================================================
// PlayPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-04-02
// 概要     : プレイフェーズの振る舞い
// ======================================================

namespace PhaseSystem.Domain
{
    /// <summary>
    /// プレイフェーズの処理
    /// </summary>
    public sealed class PlayPhaseState : IPhaseState
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在のプレイヤーID</summary>
        private readonly int _playerId;

        /// <summary>フェーズ経過時間</summary>
        private float _elapsedTime = 0.0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在のプレイヤーID</summary>
        public int PlayerId => _playerId;
        
        /// <summary>フェーズ経過時間</summary>
        public float ElapsedTime => _elapsedTime;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playerId">プレイヤーID</param>
        public PlayPhaseState(in int playerId)
        {
            _playerId = playerId;
        }
        
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ開始時処理
        /// </summary>
        public void OnEnter()
        {
        }

        /// <summary>
        /// フェーズ終了時処理
        /// </summary>
        public void OnExit()
        {
        }

        /// <summary>
        /// フェーズ更新処理
        /// </summary>
        public void OnUpdate(in float unscaledDeltaTime)
        {
            _elapsedTime += unscaledDeltaTime;
        }
    }
}