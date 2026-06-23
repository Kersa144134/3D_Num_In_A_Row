// ======================================================
// GameEventRouter.Input.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            入力関連処理をまとめたファイル
// ======================================================

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UniRx;
using InputSystem.Domain;

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
            // 最新の入力デバイスを保存
            _cachedActiveDevice = device;

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
        /// ゲーム再起動コマンド判定
        /// </summary>
        private void TryStartRestartGameCommand()
        {
            // 多重実行防止
            if (_isCheckingRestartGame)
            {
                return;
            }

            // 指定入力が揃っていない場合は処理なし
            if (!IsRestartGameCommandPressed())
            {
                return;
            }

            CheckRestartGameCommandAsync().Forget();
        }

        /// <summary>
        /// ゲーム再起動コマンドの長押し判定を実行する
        /// </summary>
        private async UniTask CheckRestartGameCommandAsync()
        {
            try
            {
                // 判定開始
                _isCheckingRestartGame = true;

                // 指定秒待機
                await UniTask.Delay(
                    TimeSpan.FromSeconds(RESTART_GAME_HOLD_SECONDS),
                    delayType: DelayType.UnscaledDeltaTime,
                    cancellationToken: _restartGameCommandCancellationTokenSource.Token);

                // 指定秒経過後も指定入力が揃っている場合
                if (IsRestartGameCommandPressed())
                {
                    _onRestartGameRequested.OnNext(Unit.Default);
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                // 判定終了
                _isCheckingRestartGame = false;
            }
        }

        /// <summary>
        /// ゲーム再起動コマンドの入力状態かどうかを判定する
        /// </summary>
        private bool IsRestartGameCommandPressed()
        {
            // X ボタン押下状態
            bool isButtonXPressed = _isButtonXPressed;

            // Right Trigger 押下状態
            bool isRightTriggerPressed = _isRightTriggerPressed;

            // Select ボタン押下状態
            bool isSelectButtonPressed = _isSelectButtonPressed;

            // DPad 左入力状態
            bool isDPadLeftPressed = _inputManager?.DPad.Angle == Vector2.left;

            // 全入力が揃っている場合のみ true
            return isButtonXPressed
                && isRightTriggerPressed
                && isSelectButtonPressed
                && isDPadLeftPressed;
        }
    }
}