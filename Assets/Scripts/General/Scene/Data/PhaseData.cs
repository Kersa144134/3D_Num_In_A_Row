// ======================================================
// PhaseData.cs
// 作成者 : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2026-03-06
// 概要     : フェーズごとの実行対象を型名で登録する ScriptableObject
// ======================================================

using System;
using UnityEngine;

namespace SceneSystem.Data
{
    /// <summary>
    /// フェーズごとの設定内容を保持する ScriptableObject
    /// 型名を元に IUpdatable を解決する
    /// </summary>
    [CreateAssetMenu(fileName = "PhaseData", menuName = "SceneSystem/PhaseData")]
    public sealed class PhaseData : ScriptableObject
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
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// このフェーズで使用する IUpdatable の型名配列を取得する
        /// </summary>
        public string[] GetUpdatableTypeNames()
        {
            if (_updatableTypeNames == null)
            {
                // 空配列を返却
                return Array.Empty<string>();
            }

            string[] names = new string[_updatableTypeNames.Length];

            for (int i = 0; i < _updatableTypeNames.Length; i++)
            {
                names[i] = _updatableTypeNames[i];
            }

            return names;
        }
    }
}