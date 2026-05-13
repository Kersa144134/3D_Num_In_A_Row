// ======================================================
// IOptionSelectionRepository.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-13
// 更新日時 : 2026-05-13
// 概要     : オプション選択状態の保存インターフェース
// ======================================================

using OptionSystem.Domain;

namespace OptionSystem.Infrastructure
{
    /// <summary>
    /// オプション選択状態保存インターフェース
    /// </summary>
    public interface IOptionSelectionRepository
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 選択インデックスを保存する
        /// </summary>
        /// <param name="type">オプション種別</param>
        /// <param name="index">選択インデックス</param>
        void Save(in OptionType type, int index);

        /// <summary>
        /// 選択インデックスを取得する
        /// </summary>
        /// <param name="type">オプション種別</param>
        /// <returns>保存済みインデックス</returns>
        int Load(in OptionType type);

        /// <summary>
        /// 設定が存在するか
        /// </summary>
        bool HasSavedData();

        /// <summary>
        /// 設定を削除する
        /// </summary>
        void Delete();
    }
}