// ======================================================
// IBoardWriter.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-22
// 更新日時 : 2026-06-22
// 概要     : 盤面書き込みインターフェース
// ======================================================

namespace BoardSystem.Domain
{
    /// <summary>
    /// 盤面書き込みインターフェース
    /// </summary>
    public interface IBoardWriter
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 指定座標へ値設定
        /// </summary>
        /// <param name="index">設定対象座標</param>
        /// <param name="value">設定する値</param>
        void Set(in BoardIndex index, in int value);

        /// <summary>
        /// 指定座標の値をクリア
        /// </summary>
        /// <param name="index">クリア対象座標</param>
        void Clear(in BoardIndex index);

        /// <summary>
        /// 盤面データを一括反映
        /// </summary>
        /// <param name="boardData">反映する盤面データ</param>
        void ApplyBoard(in int[,,] boardData);
    }
}