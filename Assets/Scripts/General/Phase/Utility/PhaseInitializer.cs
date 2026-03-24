// ======================================================
// PhaseInitializer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-02
// 更新日時 : 2026-03-24
// 概要     : フェーズごとの IUpdatable を生成するユーティリティ（型キャッシュ対応）
// ======================================================

using System;
using System.Collections.Generic;
using System.Linq;
using PhaseSystem.Data;
using SceneSystem.Data;

namespace PhaseSystem.Utility
{
    /// <summary>
    /// フェーズごとの IUpdatable を生成するクラス
    /// </summary>
    public sealed class PhaseInitializer
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>型名と Type のキャッシュ</summary>
        private readonly Dictionary<string, Type> _typeCache =
            new Dictionary<string, Type>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// フェーズごとの Updatable マップを生成する
        /// </summary>
        /// <param name="allUpdatables">シーン上の IUpdatable 配列</param>
        /// <param name="phaseDataList">全フェーズデータ</param>
        /// <returns>フェーズと Updatable 配列の対応辞書</returns>
        public Dictionary<PhaseType, IUpdatable[]> CreatePhaseMap(
            in IUpdatable[] allUpdatables,
            in PhaseData[] phaseDataList
        )
        {
            // --------------------------------------------------
            // 結果格納用辞書
            // --------------------------------------------------

            Dictionary<PhaseType, IUpdatable[]> phaseMap =
                new Dictionary<PhaseType, IUpdatable[]>();

            // --------------------------------------------------
            // フェーズごとに処理
            // --------------------------------------------------

            foreach (PhaseData phaseData in phaseDataList)
            {
                // --------------------------------------------------
                // 型名取得
                // --------------------------------------------------

                string[] typeNames =
                    phaseData.GetUpdatableTypeNames();

                // --------------------------------------------------
                // 型変換（キャッシュ利用）
                // --------------------------------------------------

                Type[] targetTypes =
                    typeNames
                        .Select(typeName => ResolveType(typeName))
                        .Where(type => type != null)
                        .ToArray();

                // --------------------------------------------------
                // Updatable抽出
                // --------------------------------------------------

                IUpdatable[] phaseUpdatables =
                    Array.FindAll(
                        allUpdatables,
                        updatable =>
                            // nullチェックと型一致判定を同時に行う
                            updatable != null &&
                            Array.Exists(
                                targetTypes,
                                type =>
                                    // 継承関係も含めて判定する
                                    type.IsAssignableFrom(updatable.GetType())
                            )
                    );

                // --------------------------------------------------
                // 辞書に登録
                // --------------------------------------------------

                phaseMap[phaseData.Phase] =
                    phaseUpdatables;
            }

            // --------------------------------------------------
            // 結果返却
            // --------------------------------------------------

            return phaseMap;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 型名から Type を取得する（キャッシュ対応）
        /// </summary>
        /// <param name="typeName">取得対象の型名（フルネーム推奨）</param>
        /// <returns>解決された Type、見つからなければ null</returns>
        private Type ResolveType(string typeName)
        {
            // --------------------------------------------------
            // キャッシュ確認
            // --------------------------------------------------

            if (_typeCache.TryGetValue(typeName, out Type cachedType))
            {
                // 既に解決済みのためそのまま返す
                return cachedType;
            }

            // --------------------------------------------------
            // 直接取得（高速パス）
            // --------------------------------------------------

            Type resolvedType =
                Type.GetType(typeName);

            // --------------------------------------------------
            // 見つからない場合は全アセンブリ検索
            // --------------------------------------------------

            if (resolvedType == null)
            {
                resolvedType =
                    AppDomain.CurrentDomain
                        .GetAssemblies()
                        .Select(assembly => assembly.GetType(typeName))
                        .FirstOrDefault(type => type != null);
            }

            // --------------------------------------------------
            // キャッシュ登録
            // --------------------------------------------------

            if (resolvedType != null)
            {
                // 次回以降の探索コスト削減のため保存
                _typeCache[typeName] = resolvedType;
            }

            // --------------------------------------------------
            // 結果返却
            // --------------------------------------------------

            return resolvedType;
        }
    }
}