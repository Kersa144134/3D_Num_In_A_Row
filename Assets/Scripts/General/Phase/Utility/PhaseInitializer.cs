// ======================================================
// PhaseInitializer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-02
// 更新日時 : 2026-02-02
// 概要     : フェーズごとの IUpdatable を整理し、PhaseController への登録を補助する
// ======================================================

using System;
using System.Collections.Generic;
using System.Linq;
using PhaseSystem.Data;
using SceneSystem.Data;

namespace SceneSystem.Utility
{
    /// <summary>
    /// フェーズごとの IUpdatable を保持し、PhaseController に登録する補助クラス
    /// </summary>
    public sealed class PhaseInitializer
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>フェーズごとに紐づく IUpdatable 配列を保持する辞書</summary>
        private readonly Dictionary<PhaseType, IUpdatable[]> _phaseUpdatablesMap = new();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズに紐づく Updatable を初期化・登録する
        /// </summary>
        /// <param name="allUpdatables">シーン上の IUpdatable 配列</param>
        /// <param name="phaseDataList">全フェーズデータ</param>
        public void InitializePhases(
            in IUpdatable[] allUpdatables,
            in PhaseData[] phaseDataList
        )
        {
            foreach (PhaseData phaseData in phaseDataList)
            {
                string[] typeNames = phaseData.GetUpdatableTypeNames();

                // 型名から Type 配列に変換
                Type[] targetTypes = typeNames
                    .Select(t =>
                    {
                        Type type = Type.GetType(t);
                        if (type != null) return type;

                        // 全アセンブリから検索
                        return AppDomain.CurrentDomain.GetAssemblies()
                            .Select(a => a.GetType(t))
                            .FirstOrDefault(tt => tt != null);
                    })
                    .Where(tt => tt != null)
                    .ToArray();

                // フェーズに紐づくコンポーネントのみ抽出
                IUpdatable[] phaseUpdatables = Array.FindAll(allUpdatables, u =>
                    u != null && Array.Exists(targetTypes, tt => tt.IsAssignableFrom(u.GetType()))
                );

                // 辞書に登録
                _phaseUpdatablesMap[phaseData.Phase] = phaseUpdatables;
            }
        }

        /// <summary>
        /// 指定フェーズに紐づく Updatable を取得する
        /// </summary>
        /// <param name="phase">取得対象のフェーズ</param>
        /// <param name="updatables">取得結果の配列</param>
        /// <returns>指定フェーズが存在すれば true、存在しなければ false</returns>
        public bool TryGetUpdatablesForPhase(in PhaseType phase, out IUpdatable[] updatables)
        {
            return _phaseUpdatablesMap.TryGetValue(phase, out updatables);
        }
    }
}