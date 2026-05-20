// ======================================================
// UpdatableBindAttribute.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : IUpdatable と UpdatableType を紐付けるための Attribute
//            Reflection による自動登録処理で利用される
// ======================================================

using System;

namespace UpdateSystem.Domain
{
    /// <summary>
    /// IUpdatable と UpdatableType の紐付けを宣言するための Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class UpdatableBindAttribute : Attribute
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 紐付け対象の Updatable 種別
        /// </summary>
        public UpdatableType Type { get; }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// コンストラクタ
        /// クラスに対して対応する UpdatableType を宣言する
        /// </summary>
        /// <param name="type">紐付け対象の Updatable 種別</param>
        public UpdatableBindAttribute(UpdatableType type)
        {
            Type = type;
        }
    }
}