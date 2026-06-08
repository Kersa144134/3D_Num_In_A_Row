// ======================================================
// DialogTextBuilder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-06-08
// 更新日時 : 2026-06-08
// 概要     : ダイアログ用テキスト生成クラス
// ======================================================

using System.Text;

namespace UISystem.Application
{
    /// <summary>
    /// ダイアログ用テキスト生成クラス
    /// </summary>
    public sealed class DialogTextBuilder
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// オプション情報表示文字列を生成
        /// </summary>
        /// <param name="playerCount">プレイヤー人数</param>
        /// <param name="boardSize">盤面サイズ</param>
        /// <param name="connectCount">勝利条件数</param>
        /// <returns>表示文字列</returns>
        public string OptionTextBuild(
            in int playerCount,
            in int boardSize,
            in int connectCount)
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"プレイヤー人数: {playerCount}");
            stringBuilder.AppendLine($"ボードのマスの数: {boardSize} × {boardSize}");
            stringBuilder.Append($"ライン成立に必要な長さ: {connectCount}");

            return stringBuilder.ToString();
        }
    }
}