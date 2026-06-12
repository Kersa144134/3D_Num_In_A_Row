// ======================================================
// GameEventRouter.Phase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            フェーズ関連処理をまとめたファイル
// ======================================================

using UniRx;
using PhaseSystem.Domain;
using SoundSystem.Domain;

namespace GameSystem.Presentation
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed partial class GameEventRouter
    {
        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// フェーズ変更リクエストを通知する
        /// </summary>
        private void NotifyPhaseChangeRequested(in PhaseType nextPhase)
        {
            _onPhaseChangeRequested.OnNext(
                new PhaseChangeEvent(
                    _currentPhase.Value,
                    nextPhase
                )
            );
        }

        /// <summary>
        /// フェーズ変更時の処理を行う
        /// </summary>
        /// <param name="phase">変更後のフェーズ</param>
        private void HandlePhaseChange(in PhaseType phase)
        {
            // 入力購読解除
            UnbindInputCommands();

            switch (phase)
            {
                // --------------------------------------------------
                // Title, Result
                // --------------------------------------------------
                case PhaseType.Title:
                case PhaseType.Result:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    break;

                // --------------------------------------------------
                // Ready
                // --------------------------------------------------
                case PhaseType.Ready:
                    // スコア計算クラス初期化
                    _scoreManager.Initialize(_gameOptionManager.PlayerCount);

                    // スコア更新購読
                    BindScoreUpdateStream();

                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // スキップ入力
                    // ChangePlayer へフェーズ遷移通知
                    BindEventSkipStream(
                        _onPhaseChangeRequested,
                        new PhaseChangeEvent(
                            _currentPhase.Value,
                            PhaseType.ChangePlayer
                        )
                    );

                    // ゲームスピード変更購読
                    BindGameSpeedChangeStream();

                    break;

                // --------------------------------------------------
                // Play
                // --------------------------------------------------
                case PhaseType.Play:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // --------------------------------------------------
                    // 直前の入力種別に応じて入力コマンドを登録
                    // --------------------------------------------------
                    switch (_currentBoardInputType.Value)
                    {
                        // 駒落下入力コマンドを登録
                        case BoardInputType.Drop:
                            BindDropInputCommands();
                            break;

                        // ボード回転入力コマンドを登録
                        case BoardInputType.Rotate:
                            BindRotateInputCommands();
                            break;
                    }

                    break;

                // --------------------------------------------------
                // Event
                // --------------------------------------------------
                case PhaseType.Event:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    break;

                // --------------------------------------------------
                // ChangePlayer
                // --------------------------------------------------
                case PhaseType.ChangePlayer:
                    // SE 再生
                    _soundManager?.PlaySE(SeType.Effect_Main_ChangePlayer);

                    // スコア累積カウントリセット
                    _scoreManager.ResetAllCumulativeCount();

                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // ボード入力種別キャッシュをリセット
                    _currentBoardInputType.Value = BoardInputType.Drop;

                    // ボード回転準備イベント発火
                    _onDropRequested.OnNext(Unit.Default);

                    // スキップ入力購読解除
                    UnbindEventSkipStream();

                    break;

                // --------------------------------------------------
                // Pause
                // --------------------------------------------------
                case PhaseType.Pause:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(1);

                    break;

                // --------------------------------------------------
                // Finish
                // --------------------------------------------------
                case PhaseType.Finish:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    // ゲームスピード変更購読解除
                    UnbindGameSpeedChangeStream();

                    // リザルトシーン遷移リクエスト発火
                    _onSceneChangeRequested.OnNext(RESULT_SCENE_NAME);

                    break;

                default:
                    break;
            }
        }
    }
}