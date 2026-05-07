// ======================================================
// IGameOptionRepository.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-27
// 更新日時 : 2026-04-27
// 概要     : ゲームオプションインターフェース
// ======================================================

using OptionSystem.Domain;

namespace OptionSystem.Infrastructure
{
    /// <summary>
    /// ゲーム設定の保存・読み込みを抽象化するインターフェース
    /// </summary>
    public interface IGameOptionRepository
    {
        /// <summary>
        /// 設定を保存する
        /// </summary>
        void Save(GameRules rules);

        /// <summary>
        /// 設定を読み込む
        /// </summary>
        GameRules Load();

        /// <summary>
        /// 設定が存在するか
        /// </summary>
        bool Exists();

        /// <summary>
        /// 設定を削除する
        /// </summary>
        void Delete();
    }
}