// ======================================================
// IContextInjectable.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-03-06
// 更新日時 : 2026-03-06
// 概要     : UpdatableContext を外部から注入するためのインターフェース
// ======================================================

namespace SceneSystem.Domain
{
    /// <summary>
    /// UpdatableContext を受け取り、
    /// コンポーネントに依存サービスを注入するためのインターフェース
    /// </summary>
    public interface IContextInjectable
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// UpdatableContext を受け取り依存関係を注入する
        /// </summary>
        /// <param name="context">シーン内で共有されるサービス参照を保持するコンテキスト</param>
        void InjectContext(UpdatableContext context);
    }
}