// ======================================================
// SceneLoader.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-28
// 更新日時 : 2026-05-11
// 概要     : シーン遷移処理クラス
// ======================================================

using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

namespace GameSystem.Application
{
    /// <summary>
    /// シーン遷移を管理するクラス
    /// </summary>
    public sealed class SceneLoader
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>
        /// シーンロード完了判定の閾値
        /// </summary>
        private const float LOAD_COMPLETE_THRESHOLD = 0.9f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>非同期ロード処理の実体</summary>
        private AsyncOperation _currentLoadOperation;

        /// <summary>直前のアクティブシーン</summary>
        private Scene _previousScene;

        // ======================================================
        // UniRx 変数
        // ======================================================

        /// <summary>ロード進捗イベント通知用 Subject</summary>
        private readonly Subject<float> _onLoadProgressSubject = new Subject<float>();

        /// <summary>ロード進捗通知</summary>
        public IObservable<float> OnLoadProgress => _onLoadProgressSubject;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// シーンの非同期ロードを開始する
        /// </summary>
        /// <param name="sceneName">読み込むシーン名</param>
        public async UniTask BeginLoadSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            // 現在のアクティブシーンを保存
            Scene activeScene = SceneManager.GetActiveScene();
            _previousScene = activeScene;

            // 非同期ロード開始
            _currentLoadOperation = SceneManager.LoadSceneAsync(sceneName);

            // シーンアクティベーションを無効化
            _currentLoadOperation.allowSceneActivation = false;

            // --------------------------------------------------
            // ロード進捗監視ループ
            // --------------------------------------------------
            while (_currentLoadOperation.progress < LOAD_COMPLETE_THRESHOLD)
            {
                // 現在の進捗を通知
                _onLoadProgressSubject.OnNext(_currentLoadOperation.progress);

                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            // 最終進捗を通知
            _onLoadProgressSubject.OnNext(LOAD_COMPLETE_THRESHOLD);
        }

        /// <summary>
        /// ロード済みシーンをアクティブ化し、シーンを切り替える
        /// </summary>
        public async UniTask CommitSceneChangeAsync()
        {
            // ロードが開始されていない場合は処理なし
            if (_currentLoadOperation == null)
            {
                return;
            }

            // シーンアクティベーション有効化
            _currentLoadOperation.allowSceneActivation = true;

            // 完了まで待機
            await _currentLoadOperation.ToUniTask();

            // --------------------------------------------------
            // 前シーン破棄処理
            // --------------------------------------------------
            if (_previousScene.IsValid() && _previousScene.isLoaded)
            {
                await SceneManager.UnloadSceneAsync(_previousScene).ToUniTask();
            }

            // 状態リセット
            _currentLoadOperation = null;
            _previousScene = default;
        }
    }
}