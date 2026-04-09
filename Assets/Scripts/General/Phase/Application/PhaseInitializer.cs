// ======================================================
// PhaseInitializer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-02-02
// 更新日時 : 2026-03-24
// 概要     : フェーズごとの IUpdatable を生成するユーティリティ
// ======================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PhaseSystem.Domain;
using SceneSystem.Domain;

namespace PhaseSystem.Application
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
        private readonly Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

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
            // フェーズごとの Updatable 配列を保持する辞書を作成
            Dictionary<PhaseType, IUpdatable[]> phaseMap = new Dictionary<PhaseType, IUpdatable[]>();

            // フェーズごとに処理
            foreach (PhaseData phaseData in phaseDataList)
            {
                // フェーズに紐づく型名を取得
                string[] typeNames = phaseData.GetUpdatableTypeNames();

                // --------------------------------------------------
                // 型変換
                // string 配列を Type 配列に変換
                // --------------------------------------------------
                Type[] targetTypes = ResolveTypes(typeNames);

                // --------------------------------------------------
                // Updatable 抽出
                // targetTypes に一致する IUpdatable を抽出
                // --------------------------------------------------
                IUpdatable[] phaseUpdatables = FilterUpdatables(allUpdatables, targetTypes);

                // --------------------------------------------------
                // フェーズ辞書登録
                // --------------------------------------------------
                phaseMap.TryAdd(phaseData.Phase, phaseUpdatables);
            }

            return phaseMap;
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 型名配列から解決可能な Type 配列に変換する
        /// </summary>
        /// <param name="typeNames">型名配列</param>
        /// <returns>解決可能な Type 配列</returns>
        private Type[] ResolveTypes(IEnumerable<string> typeNames)
        {
            // null を除外し、型名ごとに ResolveType を実行して配列化
            return typeNames
                .Select(ResolveType)
                .Where(type => type != null)
                .ToArray();
        }

        /// <summary>
        /// 指定された型に一致する IUpdatable 配列を抽出する
        /// </summary>
        /// <param name="allUpdatables">全 IUpdatable 配列</param>
        /// <param name="targetTypes">抽出対象の型配列</param>
        /// <returns>抽出された IUpdatable 配列</returns>
        private IUpdatable[] FilterUpdatables(IUpdatable[] allUpdatables, Type[] targetTypes)
        {
            // null を除外し、targetTypes に対して IsAssignableFrom で型チェック
            return allUpdatables
                .Where(updatable => updatable != null)
                .Where(updatable => targetTypes.Any(type => type.IsAssignableFrom(updatable.GetType())))
                .ToArray();
        }

        /// <summary>
        /// 型名から Type を取得する
        /// </summary>
        /// <param name="typeName">取得対象の型名</param>
        /// <returns>解決された Type、見つからなければ null</returns>
        private Type ResolveType(string typeName)
        {
            // キャッシュ確認
            if (_typeCache.TryGetValue(typeName, out Type cachedType))
            {
                return cachedType;
            }

            Type resolvedType = Type.GetType(typeName);

            // 見つからなければ全アセンブリから検索
            if (resolvedType == null)
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    resolvedType = assembly.GetType(typeName);
                    if (resolvedType != null)
                    {
                        break;
                    }
                }
            }

            // キャッシュ登録
            if (resolvedType != null)
            {
                _typeCache[typeName] = resolvedType;
            }

            return resolvedType;
        }
    }
}