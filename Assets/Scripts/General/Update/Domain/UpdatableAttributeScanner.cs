// ======================================================
// UpdatableAttributeScanner.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : UpdatableBindAttribute を走査し
//            UpdatableContexts へ自動登録を行うクラス
// ======================================================

using System;
using System.Diagnostics;
using System.Reflection;

namespace UpdateSystem.Domain
{
    /// <summary>
    /// UpdatableBindAttribute を解析し、自動登録を行うクラス
    /// </summary>
    public sealed class UpdatableAttributeScanner
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// インスタンス配列から Attribute を解析し登録する
        /// </summary>
        /// <param name="writer">登録先</param>
        /// <param name="instances">対象インスタンス</param>
        public void RegisterFromAssembly(
            IUpdatableWriter writer,
            IUpdatable[] instances)
        {
            if (writer == null || instances == null)
            {
                return;
            }

            // インスタンス単位で処理
            for (int i = 0; i < instances.Length; i++)
            {
                IUpdatable instance = instances[i];

                if (instance == null)
                {
                    continue;
                }

                // --------------------------------------------------
                // 型取得
                // --------------------------------------------------
                Type type = instance.GetType();

                // --------------------------------------------------
                // Attribute 取得
                // --------------------------------------------------
                UpdatableBindAttribute attribute = type.GetCustomAttribute<UpdatableBindAttribute>();

                // --------------------------------------------------
                // Attribute 未定義
                // --------------------------------------------------
                if (attribute == null)
                {
                    throw new InvalidOperationException(
                        $"[Updatable登録エラー] UpdatableBindAttribute が定義されていません。" +
                        $"対象クラス: {type.FullName}");
                }

                // --------------------------------------------------
                // 登録
                // --------------------------------------------------
                writer.Register(attribute.Type, instance);
            }
        }
    }
}