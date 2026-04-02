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
using PhaseSystem.Data;
using SceneSystem.Data;
using UISystem;

namespace SceneSystem.Utility
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed class SceneEventRouter : IDisposable
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>シーン内で共有されるコンテキスト</summary>
        private readonly UpdatableContext _context;

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

        /// <summary>フェーズ変更ストリーム</summary>
        public IObservable<PhaseType> OnPhaseChanged => _onPhaseChanged;

        /// <summary>入力マッピング変更通知用 Subject</summary>
        private readonly Subject<int> _onMappingChangeRequest = new Subject<int>();

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// SceneEventRouter を生成
        /// </summary>
        public SceneEventRouter(UpdatableContext context)
        {
            _context = context;

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
        public void Subscribe()
        {
            // --------------------------------------------------
            // 入力
            // --------------------------------------------------
            _onMappingChangeRequest
                .Subscribe(index =>
                {
                    // マッピング変更を適用
                    InputManager.Instance.ApplyInputMapping(index);
                })
                .AddTo(_disposables);
            
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
                        // 成立ラインをすべて出力
                        for (int i = 0; i < e.LineCount; i++)
                        {
                            UnityEngine.Debug.Log(
                                $"Player: {e.Player} Line[{i}] Length: {e.Lengths[i]}"
                            );
                        }
                    })
                    .AddTo(_disposables);
            }
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
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// オプションボタン押下時の処理を行う
        /// </summary>
        public void OnOptionButtonPressed()
        {
            // 現在適用中の入力マッピングインデックスを取得
            int current = InputManager.Instance.CurrentMappingIndex;

            // 次のインデックスを算出
            int next = (current == 0) ? 1 : 0;

            // 入力マッピングを切り替え
            _onMappingChangeRequest.OnNext(next);

            // フェーズ変更通知
            _onPhaseChanged.OnNext(PhaseType.Pause);
        }

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>
        /// 経過時間更新時の処理を行うハンドラ
        /// 経過時間と制限時間から残り時間を計算し、UI に表示する
        /// </summary>
        /// <param name="elapsedTime">現在までの経過時間（秒）</param>
        /// <param name="limitTime">制限時間（秒）</param>
        public void HandleLimitTimeUpdated(in float elapsedTime, in float limitTime)
        {
            _mainUIManager?.UpdateLimitTimeDisplay(elapsedTime, limitTime);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================
    }
}