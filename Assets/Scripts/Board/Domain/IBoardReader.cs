// ======================================================
// IBoardReader.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-22
// 更新日時 : 2026-06-22
// 概要     : 盤面読み取りインターフェース
// ======================================================

namespace BoardSystem.Domain
{
    /// <summary>
    /// 盤面読み取りインターフェース
    /// </summary>
    public interface IBoardReader
    {
        // ======================================================
        // ゲッター
        // ======================================================

        /// <summary>
        /// 指定座標の値取得
        /// </summary>
        /// <param name="index">取得対象座標</param>
        /// <returns>格納されている値</returns>
        int Get(in BoardIndex index);

        /// <summary>
        /// 盤面サイズ取得
        /// </summary>
        /// <returns>盤面サイズ</returns>
        int GetSize();
    }
}