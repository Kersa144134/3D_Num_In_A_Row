// ======================================================
// SceneLoader.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-28
// 更新日時 : 2026-04-28
// 概要     : UniTask を使用したシーン遷移処理
// ======================================================

using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

namespace GameSystem.Application
{
    /// <summary>
    /// シーン遷移を管理するローダークラス
    /// </summary>
    public sealed class SceneLoader
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>ロード完了直前とみなす進捗値</summary>
        private const float LOAD_COMPLETE_THRESHOLD = 0.9f;

        /// ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// シーン遷移を非同期で実行する
        /// </summary>
        /// <param name="sceneName">遷移先シーン名</param>
        public async UniTask ChangeSceneAsync(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                return;
            }

            // 非同期シーンロードを開始
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName);

            // シーンアクティベーションを手動制御にする
            loadOperation.allowSceneActivation = false;

            // --------------------------------------------------
            // ロード進捗監視
            // --------------------------------------------------
            // ロード完了直前まで待機
            while (loadOperation.progress < LOAD_COMPLETE_THRESHOLD)
            {
                // 1 フレーム待機
                await UniTask.Yield();
            }

            // --------------------------------------------------
            // ロード完了後の待機ポイント
            // --------------------------------------------------
            // シーンのアクティベーションを許可する
            loadOperation.allowSceneActivation = true;

            // シーンの完全読み込みまで待機する
            await loadOperation.ToUniTask();
        }
    }
}