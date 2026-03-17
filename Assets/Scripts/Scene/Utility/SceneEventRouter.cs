// ======================================================
// SceneEventRouter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-03-06
// 概要     : シーン内イベントの仲介を行う
// ======================================================

using System;
using InputSystem.Manager;
using SceneSystem.Data;
using UISystem;

namespace SceneSystem.Utility
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed class SceneEventRouter
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// シーン内で共有されるコンテキスト
        /// </summary>
        private readonly UpdatableContext _context;

        /// <summary>
        /// SceneObjectRegistry キャッシュ
        /// </summary>
        private readonly SceneObjectRegistry _sceneObjectRegistry;

        /// <summary>
        /// MainUIManager キャッシュ
        /// </summary>
        private readonly MainUIManager _mainUIManager;

        /// <summary>
        /// イベント購読状態
        /// </summary>
        private bool _isSubscribed;

        // ======================================================
        // イベント
        // ======================================================

        /// <summary>
        /// フェーズ変更通知
        /// </summary>
        public event Action<PhaseType> OnPhaseChanged;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// SceneEventRouter を生成
        /// </summary>
        /// <param name="context">UpdatableContext</param>
        public SceneEventRouter(UpdatableContext context)
        {
            // コンテキスト保持
            _context = context;

            // --------------------------------------------------
            // Contextから必要サービスを取得
            // --------------------------------------------------
            _sceneObjectRegistry = _context.Get<SceneObjectRegistry>();
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
            // 既に購読済みなら終了
            if (_isSubscribed)
            {
                return;
            }

            // --------------------------------------------------
            // オブジェクト群
            // --------------------------------------------------
            if (_sceneObjectRegistry != null)
            {
                // ここにイベント登録を書く
            }

            // 購読フラグ更新
            _isSubscribed = true;
        }

        /// <summary>
        /// イベント解除
        /// </summary>
        public void Dispose()
        {
            // 未購読なら終了
            if (!_isSubscribed)
            {
                return;
            }

            // --------------------------------------------------
            // オブジェクト群
            // --------------------------------------------------
            if (_sceneObjectRegistry != null)
            {
                // ここにイベント解除を書く
            }

            // 購読フラグ更新
            _isSubscribed = false;
        }

        // --------------------------------------------------
        // 入力
        // --------------------------------------------------
        /// <summary>
        /// オプションボタン押下時の処理を行うハンドラ
        /// SceneManager へフェーズ切り替え通知を行う
        /// </summary>
        public void HandleOptionButtonPressed()
        {
            // 現在適用中の入力マッピングインデックスを取得
            int current = InputManager.Instance.GetCurrentMappingIndex();

            // 次のインデックスを算出
            int next = (current == 0) ? 1 : 0;

            // 入力マッピングを切り替え
            InputManager.Instance.SetInputMapping(next);
        }

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>
        /// 経過時間と制限時間から残り時間を計算し、UI に表示する
        /// </summary>
        /// <param name="elapsedTime">現在までの経過時間（秒）</param>
        /// <param name="limitTime">制限時間（秒）</param>
        public void UpdateLimitTimeDisplay(in float elapsedTime, in float limitTime)
        {
            _mainUIManager?.UpdateLimitTimeDisplay(elapsedTime, limitTime);
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================
    }
}