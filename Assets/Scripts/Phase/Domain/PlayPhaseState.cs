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

        /// <summary>現在のプレイヤーインデックス</summary>
        private int _currentPlayerIndex;

        /// <summary>プレイヤー総数</summary>
        private readonly int _playerCount;

        /// <summary>フェーズ経過時間</summary>
        private float _elapsedTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>現在のプレイヤーインデックス</summary>
        public int CurrentPlayerIndex => _currentPlayerIndex;

        /// <summary>フェーズ経過時間</summary>
        public float ElapsedTime => _elapsedTime;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playerCount">プレイヤー人数</param>
        public PlayPhaseState(in int playerCount)
        {
            _playerCount = playerCount;

            // 初期プレイヤーは0番
            _currentPlayerIndex = 0;

            // 経過時間初期化
            _elapsedTime = 0.0f;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ開始時処理
        /// </summary>
        public void OnEnter()
        {
            // プレイヤー状態をリセットする場合はここ
            _elapsedTime = 0.0f;
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
            // 経過時間加算
            _elapsedTime += unscaledDeltaTime;
        }

        // ======================================================
        // プレイヤー制御
        // ======================================================

        /// <summary>
        /// 次のプレイヤーへ遷移
        /// </summary>
        public void NextPlayer()
        {
            // 循環で次プレイヤーへ
            _currentPlayerIndex =
                (_currentPlayerIndex + 1) % _playerCount;
        }

        /// <summary>
        /// 現在プレイヤーを取得
        /// </summary>
        public int GetCurrentPlayer()
        {
            return _currentPlayerIndex;
        }
    }
}