// ======================================================
// IUpdatableReader.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : Updatable の取得専用インターフェース
// ======================================================

#nullable enable

namespace SceneSystem.Domain
{
    /// <summary>
    /// Updatable の取得専用インターフェース
    /// </summary>
    public interface IUpdatableReader
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定した型に一致するインスタンス配列を取得する
        /// </summary>
        /// <typeparam name="T">取得対象の型</typeparam>
        /// <returns>一致するインスタンス配列（存在しない場合は空配列）</returns>
        T[] GetAll<T>();

        /// <summary>
        /// 指定した型に一致するインスタンスを1件取得する
        /// </summary>
        /// <typeparam name="T">取得対象の型</typeparam>
        /// <returns>最初のインスタンス、存在しない場合は null</returns>
        T? Get<T>() where T : class;
    }
}