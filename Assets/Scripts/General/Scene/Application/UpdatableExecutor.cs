// ======================================================
// UpdatableExecutor.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2025-12-15
// 概要     : 指定された IUpdatable オブジェクトを保持し OnUpdate を実行するランナー
// ======================================================

using System.Collections.Generic;
using PhaseSystem.Domain;
using SceneSystem.Domain;

namespace SceneSystem.Application
{
    /// <summary>
    /// IUpdatable を保持し、毎フレーム処理を実行するランナー
    /// </summary>
    public sealed class UpdatableExecutor
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>
        /// 登録管理用のセット
        /// 重複登録を防ぐ目的で HashSet を使用
        /// </summary>
        private readonly HashSet<IUpdatable> _updateSet = new();

        /// <summary>毎フレーム実行用の Updatable 配列キャッシュ</summary>
        private IUpdatable[] _updateArray = new IUpdatable[0];

        /// <summary>登録内容に変更があったかどうかを示すフラグ</summary>
        private bool _isDirty = true;

        // ======================================================
        // 定数
        // ======================================================

        /// <summary>インゲーム用入力マッピングインデックス</summary>
        private const int INPUT_MAPPING_INGAME = 0;

        /// <summary>アウトゲーム用入力マッピングインデックス</summary>
        private const int INPUT_MAPPING_OUTGAME = 1;

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// OnUpdate を毎フレーム実行
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        /// <param name="elapsedTime">ゲームの経過時間</param>
        public void OnUpdate(in float unscaledDeltaTime, in float elapsedTime)
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnUpdate(unscaledDeltaTime, elapsedTime);
            }
        }

        /// <summary>
        /// LateUpdate 相当の処理を毎フレーム実行
        /// </summary>
        /// <param name="unscaledDeltaTime">timeScale の影響を受けない経過時間</param>
        public void OnLateUpdate(in float unscaledDeltaTime)
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnLateUpdate(unscaledDeltaTime);
            }
        }

        /// <summary>
        /// フェーズ開始時に呼ばれる初期化処理
        /// </summary>
        /// <param name="phase">遷移先のフェーズ</param>
        public void OnPhaseEnter(in PhaseType phase)
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnPhaseEnter(phase);
            }
        }

        /// <summary>
        /// フェーズ終了時に呼ばれる終了処理
        /// </summary>
        /// <param name="phase">現在のフェーズ</param>
        public void OnPhaseExit(in PhaseType phase)
        {
            // 実行前にキャッシュを最新化する
            RebuildCache();

            for (int i = 0; i < _updateArray.Length; i++)
            {
                _updateArray[i].OnPhaseExit(phase);
            }
        }

        /// <summary>
        /// 更新対象を追加する
        /// </summary>
        /// <param name="updatable">登録する IUpdatable</param>
        public void Add(in IUpdatable updatable)
        {
            if (updatable == null)
            {
                return;
            }

            // 新規追加に成功した場合のみ Dirty を立てる
            if (_updateSet.Add(updatable))
            {
                _isDirty = true;
            }
        }

        /// <summary>
        /// 登録されている全 Updatable を削除する
        /// </summary>
        public void Clear()
        {
            // 登録情報を全削除
            _updateSet.Clear();

            // 実行配列を空にする
            _updateArray = new IUpdatable[0];

            // Dirty を解除
            _isDirty = false;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 登録内容に変更があった場合のみ、実行用配列キャッシュを再構築する
        /// </summary>
        private void RebuildCache()
        {
            // 変更がない場合は処理なし
            if (!_isDirty)
            {
                return;
            }

            // 登録数に応じた配列を新規生成する
            _updateArray = new IUpdatable[_updateSet.Count];

            int index = 0;

            // HashSet から配列へ要素をコピーする
            foreach (IUpdatable updatable in _updateSet)
            {
                _updateArray[index] = updatable;
                index++;
            }

            // Dirty を解除
            _isDirty = false;
        }
    }
}