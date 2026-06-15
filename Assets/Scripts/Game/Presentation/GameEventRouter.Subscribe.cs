// ======================================================
// GameEventRouter.Scene.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            購読関連処理をまとめたファイル
// ======================================================

using BoardSystem.Domain;
using BoardSystem.Presentation;
using Cysharp.Threading.Tasks;
using InputSystem.Domain;
using PhaseSystem.Application;
using PhaseSystem.Domain;
using ScoreSystem.Domain;
using SoundSystem.Domain;
using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace GameSystem.Presentation
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed partial class GameEventRouter
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        // --------------------------------------------------
        // 共通
        // --------------------------------------------------
        /// <summary>
        /// イベント購読
        /// </summary>
        public void Subscribe(in PhaseMachine phaseMachine)
        {
            _phaseMachine = phaseMachine;

            // --------------------------------------------------
            // ルーター
            // --------------------------------------------------
            // ボタン A 離す
            _inputManager.ButtonA.OnUp
                .Subscribe(_ =>
                {
                    // スキップイベント発火
                    _onSkipInput.OnNext(Unit.Default);
                })
                .AddTo(_disposables);
            _onFadeCompleted
                .Subscribe(_ =>
                {
                    _isFadeCompleted = true;

                    // フェーズ購読処理の保留があれば即実行
                    if (_pendingPhase.HasValue)
                    {
                        HandlePhaseChange(_pendingPhase.Value);
                        _pendingPhase = null;
                    }
                })
                .AddTo(_disposables);

            // --------------------------------------------------
            // フェーズ
            // --------------------------------------------------
            _currentPhase
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(phase =>
                {
                    // フェード未完了なら入力購読処理を保留
                    if (!_isFadeCompleted)
                    {
                        _pendingPhase = phase;

                        return;
                    }

                    HandlePhaseChange(phase);
                })
                .AddTo(_disposables);
            _phaseMachine.CurrentPlayerIndex
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(player => _onPlayerChanged.OnNext(player))
                .AddTo(_disposables);
            _phaseMachine.PlayEnterCount
                .DistinctUntilChanged()
                .Skip(1)
                .Subscribe(turn =>
                {
                    // ターン数をプレイヤー人数で割り、小数第 1 位を切り上げた値を算出
                    int turnCount = Mathf.CeilToInt((float)turn / _gameOptionManager.PlayerCount);
                    
                    // ターン変更イベント通知
                    _onTurnChanged.OnNext(turnCount);
                })
                .AddTo(_disposables);

            // --------------------------------------------------
            // オプション
            // --------------------------------------------------
            if (_gameOptionManager != null)
            {
                _gameOptionManager.BindStream(_onGameSpeedChangeRequested);
            }

            // --------------------------------------------------
            // スコア
            // --------------------------------------------------
            if (_scoreManager != null)
            {
                _scoreManager.CumulativeCount
                    .Subscribe(count => _onComboAdded.OnNext(count))
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            if (_inputManager != null)
            {
                _inputManager.BindStreams(_onMappingChanged, _onPointerPositionChanged);

                // 入力デバイス更新
                _inputManager.ActiveDeviceType
                    .DistinctUntilChanged()
                    .Subscribe(type => NotifyActiveControllerChanged(type))
                    .AddTo(_disposables);

                // スタートボタン 押す
                _inputManager.StartButton.OnDown
                    .Subscribe(_ =>
                    {
                        _onExitGameInput.OnNext(Unit.Default);
                        _onPauseInput.OnNext(Unit.Default);
                    })
                    .AddTo(_disposables);

                // X ボタン 押す
                _inputManager.ButtonX.OnDown
                    .Subscribe(_ =>
                    {
                        _isButtonXPressed = true;
                        TryStartRestartGameCommand();
                    })
                    .AddTo(_disposables);

                // X ボタン 離す
                _inputManager.ButtonX.OnUp
                    .Subscribe(_ => _isButtonXPressed = false)
                    .AddTo(_disposables);

                // 右トリガー 押す
                _inputManager.RightTrigger.OnDown
                    .Subscribe(_ =>
                    {
                        _isRightTriggerPressed = true;
                        TryStartRestartGameCommand();
                    })
                    .AddTo(_disposables);

                // 右トリガー 離す
                _inputManager.RightTrigger.OnUp
                    .Subscribe(_ => _isRightTriggerPressed = false)
                    .AddTo(_disposables);

                // セレクトボタン 押す
                _inputManager.SelectButton.OnDown
                    .Subscribe(_ =>
                    {
                        _isSelectButtonPressed = true;
                        TryStartRestartGameCommand();
                    })
                    .AddTo(_disposables);

                // セレクトボタン 離す
                _inputManager.SelectButton.OnUp
                    .Subscribe(_ => _isSelectButtonPressed = false)
                    .AddTo(_disposables);

                // DPad 押す
                _inputManager.DPad.OnDown
                    .Subscribe(_ => TryStartRestartGameCommand())
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            if (_titleUIPresenter != null)
            {
                _titleUIPresenter.BindStreams(
                    _onExitGameInput,
                    _onGamepadUsed,
                    _onSceneStartAnimationSkiped);

                // 直後にキャッシュ状態を同期
                _onGamepadUsed.OnNext(_cachedActiveDevice == InputDeviceType.Gamepad);

                _titleUIPresenter.OnUpdateGameOption
                    .Subscribe(e => HandleGameOptionUpdated(e))
                    .AddTo(_disposables);

                // タイトルスタートアニメーション終了時
                _titleUIPresenter.OnStartTitleAnimationEnd
                    .Subscribe(_ => UnbindEventSkipStream())
                    .AddTo(_disposables);
                // ゲーム開始アニメーション終了時
                _titleUIPresenter.OnStartPlayAnimationEnd
                    .Subscribe(_ => _onSceneChangeExecuted.OnNext(Unit.Default))
                    .AddTo(_disposables);

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _titleUIPresenter.BindBaseStreams(_fadeInTrigger, _fadeOutTrigger, _onFadeCompleted);

                _titleUIPresenter.OnPhaseChangeRequested
                    .Subscribe(phase => NotifyPhaseChangeRequested(phase))
                    .AddTo(_disposables);
                _titleUIPresenter.OnDialogVisibleChanged
                    .DistinctUntilChanged()
                    .Subscribe(_ => UnbindInputCommands())
                    .AddTo(_disposables);
                _titleUIPresenter.OnFocusPosition
                    .Subscribe(e => _onPointerPositionChanged.OnNext(e))
                    .AddTo(_disposables);
                _titleUIPresenter.OnFadeInCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _titleUIPresenter.OnFadeOutCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _titleUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneChangeRequested())
                    .AddTo(_disposables);
                _titleUIPresenter.OnExitGameRequested
                    .Subscribe(_ => _onExitGameRequested.OnNext(Unit.Default))
                    .AddTo(_disposables);
            }

            if (_mainUIPresenter != null)
            {
                _mainUIPresenter.BindStreams(
                    _currentPhase,
                    _onPlayerChanged,
                    _onScoreUpdated,
                    _onScoreAdded,
                    _onPauseInput,
                    _currentPhase.Select(phase => phase != PhaseType.Play && phase != PhaseType.Pause),
                    _onGamepadUsed,
                    _onColumnSelectVisibleChanged,
                    _onDropRequested,
                    _onRotateRequested,
                    _onTurnChanged,
                    _onComboAdded,
                    _phaseMachine.LimitTime);

                // 直後にキャッシュ状態を同期
                _onGamepadUsed.OnNext(_cachedActiveDevice == InputDeviceType.Gamepad);

                // Ready アニメーション終了時
                _mainUIPresenter.OnReadyAnimationEnd
                    .Subscribe(_ =>
                    {
                        UnbindEventSkipStream();
                        
                        NotifyPhaseChangeRequested(PhaseType.ChangePlayer);
                    })
                    .AddTo(_disposables);
                // ChangePlayer アニメーション終了時
                _mainUIPresenter.OnChangePlayerAnimationEnd
                    .Subscribe(_ => NotifyPhaseChangeRequested(PhaseType.Play))
                    .AddTo(_disposables);
                // Finish アニメーション終了時
                _mainUIPresenter.OnFinishAnimationEnd
                    .Subscribe(_ =>
                    {
                        UnbindEventSkipStream();

                        // リザルトシーン遷移実行発火
                        _onSceneChangeExecuted.OnNext(Unit.Default);
                    })
                    .AddTo(_disposables);

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _mainUIPresenter.BindBaseStreams(_fadeInTrigger, _fadeOutTrigger, _onFadeCompleted);

                _mainUIPresenter.OnPhaseChangeRequested
                    .Subscribe(phase => NotifyPhaseChangeRequested(phase))
                    .AddTo(_disposables);
                _mainUIPresenter.OnDialogVisibleChanged
                    .DistinctUntilChanged()
                    .Subscribe(_ => UnbindInputCommands())
                    .AddTo(_disposables);
                _mainUIPresenter.OnFocusPosition
                    .Subscribe(e => _onPointerPositionChanged.OnNext(e))
                    .AddTo(_disposables);
                _mainUIPresenter.OnFadeInCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _mainUIPresenter.OnFadeOutCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _mainUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneChangeRequested())
                    .AddTo(_disposables);
                _mainUIPresenter.OnExitGameRequested
                    .Subscribe(_ => _onExitGameRequested.OnNext(Unit.Default))
                    .AddTo(_disposables);
            }

            if (_resultUIPresenter != null)
            {
                _resultUIPresenter.BindStreams(
                    _onGamepadUsed,
                    _onSceneStartAnimationSkiped);

                // 直後にキャッシュ状態を同期
                _onGamepadUsed.OnNext(_cachedActiveDevice == InputDeviceType.Gamepad);

                // リザルト順位アニメーション終了時
                _resultUIPresenter.OnStartResultAnimationEnd
                    .Subscribe(_ => UnbindEventSkipStream())
                    .AddTo(_disposables);

                // --------------------------------------------------
                // 共通
                // --------------------------------------------------
                _resultUIPresenter.BindBaseStreams(_fadeInTrigger, _fadeOutTrigger, _onFadeCompleted);

                _resultUIPresenter.OnPhaseChangeRequested
                    .Subscribe(phase => NotifyPhaseChangeRequested(phase))
                    .AddTo(_disposables);
                _resultUIPresenter.OnDialogVisibleChanged
                    .DistinctUntilChanged()
                    .Subscribe(_ => UnbindInputCommands())
                    .AddTo(_disposables);
                _resultUIPresenter.OnFocusPosition
                    .Subscribe(e => _onPointerPositionChanged.OnNext(e))
                    .AddTo(_disposables);
                _resultUIPresenter.OnFadeInCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _resultUIPresenter.OnFadeOutCompletedStream
                    .Subscribe(_ => _onFadeCompleted.OnNext(Unit.Default))
                    .AddTo(_disposables);
                _resultUIPresenter.OnSceneChangeRequested
                    .Subscribe(_ => NotifySceneChangeRequested())
                    .AddTo(_disposables);
                _resultUIPresenter.OnExitGameRequested
                    .Subscribe(_ => _onExitGameRequested.OnNext(Unit.Default))
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // カメラ
            // --------------------------------------------------
            if (_cameraPresenter != null)
            {
                _cameraPresenter.BindStreams(
                    _currentPhase
                        .Where(phase => phase == PhaseType.ChangePlayer)
                        .Select(_ => Unit.Default),
                    Observable.CombineLatest(
                        _currentPhase,
                        _currentBoardInputType,
                        (phase, inputType) =>
                        {
                            bool isPlay = phase == PhaseType.Play;
                            bool isRotate = inputType == BoardInputType.Rotate;

                            // Play フェーズでないまたは Rotate 中に有効
                            return !isPlay || isRotate;
                        }),
                    _onGamepadUsed,
                    _mainUIPresenter.OnSwitchProjection,
                    _onLinePositionNotified
                );
            }

            // --------------------------------------------------
            // ボード
            // --------------------------------------------------
            if (_boardPresenters != null)
            {
                foreach (BoardPresenter boardPresenter in _boardPresenters)
                {
                    if (boardPresenter == null)
                    {
                        continue;
                    }

                    boardPresenter.BindPlayerChangeStream(_onPlayerChanged);

                    boardPresenter.OnPlayerEnd
                        .Subscribe(_ => NotifyPhaseChangeRequested(PhaseType.ChangePlayer))
                        .AddTo(_disposables);
                    boardPresenter.OnDropInputted
                        .Subscribe(_ => NotifyPhaseChangeRequested(PhaseType.Event))
                        .AddTo(_disposables);
                    boardPresenter.OnRotateInputted
                        .Subscribe(_ => NotifyPhaseChangeRequested(PhaseType.Event))
                        .AddTo(_disposables);
                    boardPresenter.OnLineComplete
                        .Subscribe(e => HandleLineCompleted(e))
                        .AddTo(_disposables);
                    boardPresenter.OnLineDelete
                        .Subscribe(_ =>
                        {
                            if (!_hasPendingScoreEvent)
                            {
                                return;
                            }

                            // スコア更新通知
                            _onScoreUpdated.OnNext(_pendingUpdateScoreEvent);

                            // リセット
                            _hasPendingScoreEvent = false;
                        })
                        .AddTo(_disposables);
                    boardPresenter.OnLinePositionNotified
                        .Subscribe(linePosition => _onLinePositionNotified.OnNext(linePosition))
                        .AddTo(_disposables);
                    boardPresenter.OnLineEmissionExecuted
                        .Subscribe(_ =>
                        {
                            if (_pendingAddScoreEvents.Count == 0)
                            {
                                return;
                            }

                            // 保留中の加算スコアイベント取得
                            ScoreEvent scoreEvent = _pendingAddScoreEvents.Dequeue();

                            // スコア加算通知
                            _onScoreAdded.OnNext(scoreEvent);
                        })
                        .AddTo(_disposables);
                    boardPresenter.IsColumnSelectVisible
                        .DistinctUntilChanged()
                        .DistinctUntilChanged()
                        .Subscribe(isVisible => _onColumnSelectVisibleChanged.OnNext(isVisible))
                        .AddTo(_disposables);
                }
            }
        }

        /// <summary>
        /// イベント購読解除
        /// </summary>
        public void Dispose()
        {
            // 購読解除
            _disposables.Dispose();

            UnbindSceneLoadProgressStream();
            UnbindEventSkipStream();
        }

        /// <summary>
        /// イベントストリームをまとめて購読する
        /// </summary>
        public void BindStreams(
            in IObservable<string> onPrepareStart,
            in IObservable<Unit> onPrepareEnd,
            in IObservable<float> onSceneChanged,
            in IObservable<float> onFadeStarted)
        {
            onPrepareStart
                .Subscribe(sceneName =>
                {
                    // ロード準備開始
                    HandleLoadPrepareStart(sceneName);
                })
                .AddTo(_disposables);

            onPrepareEnd
                .Subscribe(_ =>
                {
                    // ロード準備完了
                    HandleLoadPrepareEnd();
                })
                .AddTo(_disposables);

            onSceneChanged
                .Subscribe(seconds =>
                {
                    // フェードアウト時間を通知
                    _fadeOutTrigger.OnNext(seconds);

                    // スキップ入力
                    // シーンスタートアニメーションスキップ通知
                    BindEventSkipStream(_onSceneStartAnimationSkiped, Unit.Default);
                })
                .AddTo(_disposables);

            onFadeStarted
                .Subscribe(seconds =>
                {
                    // フェードイン時間を通知
                    _fadeInTrigger.OnNext(seconds);

                    UnbindEventSkipStream();

                    // BGM フェードイン
                    _soundManager.SetBGMVolume(0);
                })
                .AddTo(_disposables);
        }

        // --------------------------------------------------
        // シーン
        // --------------------------------------------------
        /// <summary>
        /// シーンロード進捗ストリームを購読する
        /// </summary>
        public void BindSceneLoadProgressStream(in IObservable<float> onSceneLoadProgress)
        {
            // 多重購読防止
            _sceneLoadSubscription?.Dispose();

            _sceneLoadSubscription = onSceneLoadProgress
                .Subscribe(progress =>
                {
                    // デバッグ用
                    Debug.Log(progress);
                });
        }

        /// <summary>
        /// シーンロード進捗ストリームの購読を解除する
        /// </summary>
        public void UnbindSceneLoadProgressStream()
        {
            _sceneLoadSubscription?.Dispose();
            _sceneLoadSubscription = null;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        // --------------------------------------------------
        // システム
        // --------------------------------------------------
        /// <summary>
        /// イベントスキップストリームを購読する
        /// </summary>
        /// <typeparam name="TEvent">発火イベント型</typeparam>
        /// <param name="subject">発火対象 Subject</param>
        /// <param name="eventValue">発火時に送信する値</param>
        private void BindEventSkipStream<TEvent>(Subject<TEvent> subject, TEvent eventValue)
        {
            // 多重購読防止
            _skipInputSubscription?.Dispose();

            _skipInputSubscription = _onSkipInput
                .Subscribe(_ =>
                {
                    // スキップ入力時に指定イベントを発火
                    subject.OnNext(eventValue);

                    // SE 再生
                    _soundManager?.PlaySE(SeType.UI_Skip, 0.75f);
                });
        }

        /// <summary>
        /// イベントスキップストリームの購読を解除する
        /// </summary>
        private void UnbindEventSkipStream()
        {
            _skipInputSubscription?.Dispose();
            _skipInputSubscription = null;
        }

        /// <summary>
        /// ゲームスピード変更ストリームを購読する
        /// </summary>
        private void BindGameSpeedChangeStream()
        {
            // 多重購読防止
            _gameSpeedChangeDisposables?.Dispose();

            // CompositeDisposable 生成
            _gameSpeedChangeDisposables = new CompositeDisposable();

            // R ショルダー 押す
            _inputManager.RightShoulder.OnDown
                .Subscribe(_ =>
                {
                    // 高速状態へ変更
                    _onGameSpeedChangeRequested.OnNext(GAME_SPEED_FAST);
                })
                .AddTo(_gameSpeedChangeDisposables);

            // R ショルダー 離す
            _inputManager.RightShoulder.OnUp
                .Subscribe(_ =>
                {
                    // 通常状態へ変更
                    _onGameSpeedChangeRequested.OnNext(GAME_SPEED_NORMAL);
                })
                .AddTo(_gameSpeedChangeDisposables);
        }

        /// <summary>
        /// ゲームスピード変更ストリームの購読を解除する
        /// </summary>
        private void UnbindGameSpeedChangeStream()
        {
            // 通常速度へリセット
            _onGameSpeedChangeRequested.OnNext(1.0f);

            // イベント購読解除
            _gameSpeedChangeDisposables?.Dispose();
            _gameSpeedChangeDisposables = null;
        }

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>
        /// スコア更新ストリームを購読する
        /// </summary>
        private void BindScoreUpdateStream()
        {
            if (_scoreManager == null)
            {
                return;
            }

            // プレイヤー人数取得
            int playerCount = _gameOptionManager.PlayerCount;

            // 1 ベースでループ
            for (int playerId = 1; playerId <= playerCount; playerId++)
            {
                int currentPlayerId = playerId;

                _scoreManager.GetTotalScore(currentPlayerId)
                    .Skip(1)
                    .Subscribe(score =>
                    {
                        // 最新スコアを保留イベントとして保持する
                        _pendingUpdateScoreEvent = new ScoreEvent(currentPlayerId, score);

                        _hasPendingScoreEvent = true;
                    })
                    .AddTo(_disposables);
            }
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// 駒落下用の入力コマンド購読を登録する
        /// </summary>
        private void BindDropInputCommands()
        {
            // 入力購読解除
            UnbindInputCommands();

            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            // ボタン A 離す
            _inputManager.ButtonA.OnUp
                .Subscribe(_ =>
                {
                    // 駒落下イベント発火
                    _onDropExecuted.OnNext(Unit.Default);
                })
                .AddTo(_inputDisposables);

            // ボタン B 押す
            _inputManager.ButtonB.OnDown
                .Subscribe(async _ =>
                {
                    // ボード回転準備イベント発火
                    _onRotateRequested.OnNext(Unit.Default);

                    // 入力購読解除
                    UnbindInputCommands();

                    // CompositeDisposable 生成
                    _inputDisposables = new CompositeDisposable();

                    // 入力切替遅延
                    // タイムスケールを無視する
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(INPUT_BIND_DELAY_SECONDS),
                        DelayType.UnscaledDeltaTime
                    );

                    // 回転入力コマンド再登録
                    BindRotateInputCommands();
                })
                .AddTo(_inputDisposables);

            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.BindDropInputStream(_onDropExecuted);
                boardPresenter.UnbindRotateInputStream();
            }

            // ボード入力更新
            _currentBoardInputType.Value = BoardInputType.Drop;
        }

        /// <summary>
        /// ボード回転用の入力コマンド購読を登録する
        /// </summary>
        private void BindRotateInputCommands()
        {
            // 入力購読解除
            UnbindInputCommands();

            // CompositeDisposable 生成
            _inputDisposables = new CompositeDisposable();

            // 左スティック初回入力
            _inputManager.LeftStick.OnDown
                .Subscribe(direction =>
                {
                    // 左方向
                    if (direction == Vector2.left)
                    {
                        // Z- 回転実行イベント発火
                        _onRotateExecuted.OnNext(
                            new RotationCommand(
                                RotationAxis.Z,
                                RotationDirection.Negative
                            )
                        );

                        return;
                    }

                    // 右方向
                    if (direction == Vector2.right)
                    {
                        // Z+ 回転実行イベント発火
                        _onRotateExecuted.OnNext(
                            new RotationCommand(
                                RotationAxis.Z,
                                RotationDirection.Positive
                            )
                        );

                        return;
                    }

                    // 上方向
                    if (direction == Vector2.up)
                    {
                        // X- 回転実行イベント発火
                        _onRotateExecuted.OnNext(
                            new RotationCommand(
                                RotationAxis.X,
                                RotationDirection.Negative
                            )
                        );

                        return;
                    }

                    // 下方向
                    if (direction == Vector2.down)
                    {
                        // X+ 回転実行イベント発火
                        _onRotateExecuted.OnNext(
                            new RotationCommand(
                                RotationAxis.X,
                                RotationDirection.Positive
                            )
                        );
                    }
                })
                .AddTo(_inputDisposables);

            // ボタン B 押す
            _inputManager.ButtonB.OnDown
                .Subscribe(async _ =>
                {
                    // 駒落下準備イベント発火
                    _onDropRequested.OnNext(Unit.Default);

                    // 入力購読解除
                    UnbindInputCommands();

                    // CompositeDisposable 生成
                    _inputDisposables = new CompositeDisposable();

                    // 入力切替遅延
                    // タイムスケールを無視する
                    await UniTask.Delay(
                        TimeSpan.FromSeconds(INPUT_BIND_DELAY_SECONDS),
                        DelayType.UnscaledDeltaTime
                    );

                    // 駒落下時の入力コマンド登録
                    BindDropInputCommands();
                })
                .AddTo(_inputDisposables);

            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.UnbindDropInputStream();
                boardPresenter.BindRotateInputStream(_onRotateExecuted);
            }

            // ボード入力更新
            _currentBoardInputType.Value = BoardInputType.Rotate;
        }

        /// <summary>
        /// 入力コマンド購読を解除する
        /// </summary>
        private void UnbindInputCommands()
        {
            _inputDisposables?.Dispose();
            _inputDisposables = null;

            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.UnbindDropInputStream();
                boardPresenter.UnbindRotateInputStream();
            }
        }
    }
}