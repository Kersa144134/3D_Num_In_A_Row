// ======================================================
// GameEventRouter.Scene.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            シーン関連処理をまとめたファイル
// ======================================================

using UniRx;
using PhaseSystem.Domain;

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
        /// シーン遷移予約を通知する
        /// </summary>
        private void NotifySceneChangeRequested()
        {
            switch (_currentPhase.Value)
            {
                case PhaseType.Title:
                    // 3 x 3 ボードシーンへ遷移
                    if (_gameOptionManager.BoardSize == BOARD_SIZE_THREE)
                    {
                        _onSceneChangeRequested.OnNext(THREE_SIZE_SCENE_NAME);
                        break;
                    }

                    // 5 x 5 ボードシーンへ遷移
                    if (_gameOptionManager.BoardSize == BOARD_SIZE_FIVE)
                    {
                        _onSceneChangeRequested.OnNext(FIVE_SIZE_SCENE_NAME);
                    }

                    break;

                case PhaseType.Finish:
                    // リザルトシーンへ遷移
                    _onSceneChangeRequested.OnNext(RESULT_SCENE_NAME);

                    break;

                case PhaseType.Pause:
                case PhaseType.Result:
                    // タイトルシーンへ遷移
                    _onSceneChangeRequested.OnNext(TITLE_SCENE_NAME);

                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// シーンロード準備開始時の処理を行う
        /// </summary>
        /// <param name="sceneName">現在のシーン名</param>
        private void HandleLoadPrepareStart(in string sceneName)
        {
            switch (sceneName)
            {
                case TITLE_SCENE_NAME:
                    // スキップ入力
                    // シーン遷移実行通知
                    BindEventSkipStream(_onSceneChangeExecuted, Unit.Default);

                    break;

                case THREE_SIZE_SCENE_NAME:
                    // スキップ入力
                    // シーン遷移実行通知
                    BindEventSkipStream(_onSceneChangeExecuted, Unit.Default);

                    break;

                case FIVE_SIZE_SCENE_NAME:
                    // スキップ入力
                    // シーン遷移実行通知
                    BindEventSkipStream(_onSceneChangeExecuted, Unit.Default);

                    break;

                case RESULT_SCENE_NAME:
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// シーンロード準備開始時の処理を行う
        /// </summary>
        private void HandleLoadPrepareEnd(in float fadeTime)
        {
            // 購読解除
            _sceneLoadSubscription?.Dispose();
            _sceneLoadSubscription = null;

            // フェードイン時間を通知
            _fadeInTrigger.OnNext(fadeTime);
        }
    }
}