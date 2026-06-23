// ======================================================
// IUpdatableRunnerModifier.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : UpdatableRunner の状態変更を行うインターフェース
// ======================================================

#nullable enable

namespace UpdateSystem.Domain
{
    /// <summary>
    /// UpdatableRunner の登録内容を変更する
    /// </summary>
    public interface IUpdatableRunnerModifier
    {
        /// <summary>
        /// 登録されている Updatable を丸ごと置換する
        /// </summary>
        /// <param name="updatables">新しい Updatable 配列</param>
        void Replace(in IUpdatable[] updatables);
    }
}