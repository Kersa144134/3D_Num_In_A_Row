// ======================================================
// GameEventRouter.Input.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            入力関連処理をまとめたファイル
// ======================================================

using UniRx;
using InputSystem.Domain;
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
        /// 入力マッピング変更を通知する
        /// </summary>
        private void NotifyMappingChanged(in int mappingIndex)
        {
            // 現在のマッピング番号と一致している場合は処理なし
            if (_currentMappingIndex == mappingIndex)
            {
                return;
            }

            _currentMappingIndex = mappingIndex;

            _onMappingChanged.OnNext(mappingIndex);
        }

        /// <summary>
        /// デバイス変更を通知する
        /// </summary>
        private void NotifyActiveControllerChanged(in InputDeviceType device)
        {
            if (device == InputDeviceType.Gamepad)
            {
                _onGamepadUsed.OnNext(true);
            }
            else
            {
                _onGamepadUsed.OnNext(false);
            }
        }

        /// <summary>
        /// フェーズ変更時の入力購読切替処理を行う
        /// </summary>
        /// <param name="phase">変更後のフェーズ</param>
        private void HandlePhaseInputSwitch(in PhaseType phase)
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
                // Event, Pause
                // --------------------------------------------------
                case PhaseType.Event:
                case PhaseType.Pause:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    break;

                // --------------------------------------------------
                // Ready
                // --------------------------------------------------
                case PhaseType.Ready:
                    // 入力マッピングをインゲーム用に変更
                    NotifyMappingChanged(0);

                    // スキップ入力
                    // ChangePlayer へフェーズ遷移通知
                    BindEventSkipStream(
                        _onPhaseChanged,
                        new PhaseChangeEvent(
                            _currentPhase.Value,
                            PhaseType.ChangePlayer
                        )
                    );

                    // ゲームスピード変更購読
                    BindGameSpeedChangeStream();

                    // スコア更新購読
                    BindScoreUpdateStream();

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
                // ChangePlayer
                // --------------------------------------------------
                case PhaseType.ChangePlayer:
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
                // Finish
                // --------------------------------------------------
                case PhaseType.Finish:
                    // 入力マッピングを UI 用に変更
                    NotifyMappingChanged(1);

                    // ゲームスピード変更購読解除
                    UnbindGameSpeedChangeStream();

                    // スキップ入力
                    // リザルトシーンへ遷移予約通知
                    BindEventSkipStream(
                        _onSceneChangeRequested,
                        RESULT_SCENE_NAME
                    );

                    break;

                default:
                    break;
            }
        }
    }
}