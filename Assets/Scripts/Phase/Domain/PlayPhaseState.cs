// ======================================================
// PlayPhaseState.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-23
// 更新日時 : 2026-04-02
// 概要     : プレイフェーズの振る舞い
// ======================================================

using UnityEngine;
using UniRx;

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

        /// <summary>プレイヤー総数</summary>
        private readonly int _playerCount;

        /// <summary>フェーズ経過時間</summary>
        private float _elapsedTime = 0.0f;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>フェーズ経過時間</summary>
        public float ElapsedTime => _elapsedTime;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>プレイヤー最低人数</summary>
        private const int MIN_PLAYER_COUNT = 2;

        // ======================================================
        // UniR 変数
        // ======================================================

        /// <summary>現在プレイヤーインデックス取得用 Subject</summary>
        private readonly ReactiveProperty<int> _currentPlayerIndex;

        /// <summary>現在プレイヤーインデックスストリーム</summary>
        public IReadOnlyReactiveProperty<int> CurrentPlayerIndex => _currentPlayerIndex;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="playerCount">プレイヤー人数</param>
        public PlayPhaseState(in int playerCount)
        {
            // プレイヤー人数が MIN_PLAYER_COUNT 未満の場合は不正
            if (playerCount < MIN_PLAYER_COUNT)
            {
                UnityEngine.Debug.LogError(
                    "PlayPhaseState: PlayerCount が MIN_PLAYER_COUNT 未満です。"
                );

                // 最低人数に補正
                _playerCount = 2;
            }
            else
            {
                _playerCount = playerCount;
            }

            // --------------------------------------------------
            // 初期プレイヤーをランダムに設定
            // --------------------------------------------------
            int initialPlayerIndex = Random.Range(1, _playerCount + 1);
            _currentPlayerIndex = new ReactiveProperty<int>(initialPlayerIndex);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ開始時処理
        /// </summary>
        public void OnEnter()
        {
            _elapsedTime = 0.0f;

            NextPlayer();
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

        /// <summary>
        /// 次のプレイヤーへ遷移
        /// </summary>
        private void NextPlayer()
        {
            // --------------------------------------------------
            // 1 ベースの循環処理
            // --------------------------------------------------
            // 0 ベースに変換
            int zeroBasedIndex = _currentPlayerIndex.Value - 1;

            // 循環処理
            zeroBasedIndex = (zeroBasedIndex + 1) % _playerCount;

            // 1 ベースに変換
            _currentPlayerIndex.Value = zeroBasedIndex + 1;
        }
    }
}