// ======================================================
// SceneEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-06
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using System;
using UniRx;
using BoardSystem;
using InputSystem;
using PhaseSystem;
using PhaseSystem.Data;
using SceneSystem.Data;
using UISystem;
using BoardSystem.Data;
using System.Diagnostics;
using System.Collections.Generic;

namespace SceneSystem.Utility
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed class SceneEventRouter
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン内で共有されるコンテキスト</summary>
        private readonly UpdatableContext _context;

        /// <summary>InputManager キャッシュ</summary>
        private readonly InputManager _inputManager;

        /// <summary>SceneObjectContainer キャッシュ配列</summary>
        private readonly BoardPresenter[] _boardPresenters;

        /// <summary>MainUIManager キャッシュ</summary>
        private readonly MainUIManager _mainUIManager;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>購読管理</summary>
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        
        /// <summary>フェーズ変更通知用 Subject</summary>
        private readonly Subject<PhaseType> _onPhaseChanged = new Subject<PhaseType>();

        /// <summary>入力マッピング変更用 Subject</summary>
        private readonly Subject<int> _onMappingChanged = new Subject<int>();

        /// <summary>ライン成立通知用 Subject</summary>
        private readonly Subject<LineCompleteEvent> _onLineComplete = new Subject<LineCompleteEvent>();

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseType> OnPhaseChanged => _onPhaseChanged;

        /// <summary>入力マッピング変更ストリーム</summary>
        public IObservable<int> OnMappingChanged => _onMappingChanged;

        /// <summary>ライン成立ストリーム</summary>
        public IObservable<LineCompleteEvent> OnLineComplete => _onLineComplete;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// SceneEventRouter を生成
        /// </summary>
        public SceneEventRouter(UpdatableContext context)
        {
            _context = context;

            // インスタンスからコンポーネントを取得
            _inputManager = InputManager.Instance;

            // Context からコンポーネントを取得
            _boardPresenters = _context.GetAll<BoardPresenter>();
            _mainUIManager = _context.Get<MainUIManager>();
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// イベント購読
        /// </summary>
        public void Subscribe(in PhasePresenter phasePresenter)
        {
            // --------------------------------------------------
            // フェーズ
            // --------------------------------------------------
            phasePresenter.OnStartButtonPressed
                .Subscribe(e => OnStartButtonPressed(e))
                .AddTo(_disposables);

            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            _inputManager.BindMappingStream(OnMappingChanged);
            
            // --------------------------------------------------
            // ボード
            // --------------------------------------------------
            foreach (BoardPresenter boardPresenter in _boardPresenters)
            {
                if (boardPresenter == null)
                {
                    continue;
                }

                boardPresenter.OnLineComplete
                    .Subscribe(e =>
                    {
                        _onLineComplete.OnNext(e);

                        // 成立ライン数を取得
                        int lineCount = e.LinePositions.Length;

                        // 各ラインごとに処理
                        for (int i = 0; i < lineCount; i++)
                        {
                            // 現在のライン
                            IReadOnlyList<BoardIndex> line = e.LinePositions[i];

                            // 各駒の座標を文字列化
                            List<string> positions = new List<string>();
                            foreach (BoardIndex index in line)
                            {
                                // BoardIndex の座標を "(x,y,z)" 形式で追加
                                positions.Add($"({index.X}, {index.Y}, {index.Z})");
                            }

                            // 現在ラインの座標文字列
                            string lineText = string.Join(", ", positions);

                            // ライン番号と座標をログ出力
                            UnityEngine.Debug.Log($"Player {e.Player} 完成ライン {i + 1}/{lineCount}: 駒座標 [{lineText}]");
                        }
                    })
                    .AddTo(_disposables);
            }

            // --------------------------------------------------
            // UI
            // --------------------------------------------------
            phasePresenter.OnLimitTimeUpdated
                .Subscribe(e => _mainUIManager?.UpdateLimitTimeDisplay(e.RemainingTime))
                .AddTo(_disposables);
        }

        /// <summary>
        /// イベント解除
        /// </summary>
        public void Dispose()
        {
            // 購読解除
            _disposables.Dispose();

            // サブジェクト終了
            _onPhaseChanged.OnCompleted();
            _onPhaseChanged.Dispose();

            _onLineComplete.OnCompleted();
            _onLineComplete.Dispose();

            _inputManager.UnbindMappingStream();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================
        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// スタートボタン押下時の処理を行う
        /// </summary>
        /// <param name="e">スタートボタンイベント</param>
        private void OnStartButtonPressed(in StartButtonEvent e)
        {
            // フェーズに応じてマッピングと遷移先を決定
            int mappingIndex;
            PhaseType nextPhase;

            if (e.Phase == PhaseType.Play)
            {
                mappingIndex = 1;
                nextPhase = PhaseType.Pause;
            }
            else if (e.Phase == PhaseType.Pause)
            {
                mappingIndex = 0;
                nextPhase = PhaseType.Play;
            }
            else
            {
                return;
            }

            // 入力マッピング変更通知
            _onMappingChanged.OnNext(mappingIndex);

            // フェーズ変更通知
            _onPhaseChanged.OnNext(nextPhase);
        }
    }
}