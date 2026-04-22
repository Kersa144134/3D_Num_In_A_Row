// ======================================================
// UpdatableManagement.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-01-23
// 概要     : Updatable を管理するサービス
// ======================================================

using PhaseSystem.Domain;
using SceneSystem.Domain;

namespace SceneSystem.Application
{
    /// <summary>
    /// Updatable を管理するサービス
    /// </summary>
    public sealed class UpdatableManagement
    {
        // ======================================================
        // コンポーネント参照
        // ======================================================

        /// <summary>毎フレーム処理を実行するランナー</summary>
        private readonly UpdatableExecutor _updatableExecutor = new UpdatableExecutor();

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>現在適用中のフェーズ</summary>
        private PhaseType _currentPhase = PhaseType.None;

        private IUpdatable[] _updatables;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// UpdateManagement を生成する
        /// </summary>
        /// <param name="phaseUpdatablesMap">フェーズごとの IUpdatable 配列を保持する辞書</param>
        public UpdatableManagement(in IUpdatable[] updatables)
        {
            _updatables = updatables;
        }

        // ======================================================
        // IUpdatable イベント
        // ======================================================

        public void Update(in float unscaledDeltaTime)
        {
            _updatableExecutor.OnUpdate(unscaledDeltaTime);
        }

        public void LateUpdate(in float unscaledDeltaTime)
        {
            _updatableExecutor.OnLateUpdate(unscaledDeltaTime);
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズ変更時に Exit / Enter を実行する
        /// </summary>
        /// <param name="nextPhase">遷移先フェーズ</param>
        public void ChangePhase(in PhaseType nextPhase)
        {
            // 現在フェーズの Exit を呼ぶ
            if (_currentPhase != PhaseType.None)
            {
                _updatableExecutor.OnPhaseExit(_currentPhase);
            }

            // UpdateController をリセット
            _updatableExecutor.Clear();

            // UpdatableExecutor に反映
            foreach (IUpdatable updatable in _updatables)
            {
                _updatableExecutor.Add(updatable);
            }

            // 遷移先フェーズの Enter を呼ぶ
            _updatableExecutor.OnPhaseEnter(nextPhase);

            // フェーズを更新
            _currentPhase = nextPhase;
        }
    }
}