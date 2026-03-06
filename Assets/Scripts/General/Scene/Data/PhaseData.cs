// ======================================================
// PhaseData.cs
// 作成者 : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : フェーズごとの実行対象を型名（string）で登録する ScriptableObject
// ======================================================

using System;
using UnityEngine;

namespace SceneSystem.Data
{
    /// <summary>
    /// フェーズごとの設定内容を保持する ScriptableObject
    /// 型名（string）を元に IUpdatable を解決する
    /// </summary>
    [CreateAssetMenu(fileName = "PhaseData", menuName = "SceneSystem/PhaseData")]
    public class PhaseData : ScriptableObject
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        /// <summary>
        /// この PhaseData が表すフェーズの種別
        /// </summary>
        [SerializeField]
        private PhaseType _phaseType;

        /// <summary>
        /// 実行対象となる IUpdatable 実装クラスの完全修飾型名
        /// 例 : SceneSystem.Update.PlayerUpdate
        /// </summary>
        [SerializeField]
        private string[] _updatableTypeNames;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// この PhaseData に対応するフェーズ種別を取得する
        /// </summary>
        public PhaseType Phase
        {
            get
            {
                return _phaseType;
            }
        }

        // ======================================================
        // 型名取得
        // ======================================================

        /// <summary>
        /// このフェーズで使用する IUpdatable の型名配列を取得する
        /// </summary>
        public string[] GetUpdatableTypeNames()
        {
            // null 安全のため空配列を返却
            if (_updatableTypeNames == null)
            {
                return Array.Empty<string>();
            }

            // 直接参照を返さずコピーを返却
            string[] names = new string[_updatableTypeNames.Length];

            for (int i = 0; i < _updatableTypeNames.Length; i++)
            {
                // インスペクタ設定値をそのままコピー
                names[i] = _updatableTypeNames[i];
            }

            return names;
        }
    }
}