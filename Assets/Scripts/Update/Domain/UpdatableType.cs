// ======================================================
// UpdatableType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-22
// 更新日時 : 2026-04-22
// 概要     : フェーズ内で使用されるUpdatable識別子を定義する列挙型
//          : 実体解決のキーとして使用し、依存関係の直接参照を排除する
// ======================================================

namespace UpdateSystem.Domain
{
    /// <summary>
    /// フェーズで利用されるUpdatable識別子
    /// </summary>
    public enum UpdatableType
    {
        BoardPresenter,
        CameraPresenter,
        TitleUIPresenter,
        MainUIPresenter,
        ResultUIPresenter
    }
}