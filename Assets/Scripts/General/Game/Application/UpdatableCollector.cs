// ======================================================
// UpdatableCollector.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-08
// 更新日時 : 2025-12-08
// 概要     : 指定ルートから IUpdatable コンポーネントを取得するクラス
// ======================================================

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SceneSystem.Domain;

namespace SceneSystem.Application
{
    /// <summary>
    /// IUpdatable を実装しているコンポーネントをシーンルートから収集する
    /// </summary>
    public class UpdatableCollector
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定されたルート GameObject 配列から IUpdatable コンポーネントを収集する
        /// </summary>
        /// <param name="roots">探索対象となる GameObject 配列</param>
        /// <returns>収集した IUpdatable 配列</returns>
        public IUpdatable[] Collect(in GameObject[] roots)
        {
            HashSet<IUpdatable> updatables = new HashSet<IUpdatable>();

            foreach (GameObject root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                // root および子オブジェクトに存在する全 IUpdatable を取得
                IUpdatable[] allUpdatables = root.GetComponentsInChildren<IUpdatable>(true);

                foreach (IUpdatable updatable in allUpdatables)
                {
                    updatables.Add(updatable);
                }
            }

            // HashSet から配列に変換して返却
            return updatables.ToArray();
        }
    }
}