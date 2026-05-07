// ======================================================
// OptionButtonHandler.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-07
// 概要     : オプションボタン入力を処理し、
//            GameOptionManager へ設定値を反映するハンドラ
// ======================================================

using UnityEngine;

namespace OptionSystem.Presentation
{
    /// <summary>
    /// オプションボタン入力を GameOptionManager へ設定値を反映するハンドラ
    /// </summary>
    public sealed class OptionButtonHandler : MonoBehaviour
    {
        // ======================================================
        // ボタンイベント
        // ======================================================

        // --------------------------------------------------
        // セーブ
        // --------------------------------------------------
        /// <summary>
        /// 現在設定中のオプションを保存する
        /// </summary>
        public void Save()
        {
            GameOptionManager.Instance.Save();
        }

        // --------------------------------------------------
        // ロード
        // --------------------------------------------------
        /// <summary>
        /// 保存済みオプションを読み込む
        /// </summary>
        public void Load()
        {
            GameOptionManager.Instance.Load();
        }
    }
}