// ======================================================
// BgmType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-05-26
// 概要     : BGM タイプ定義
// ======================================================

namespace SoundSystem.Domain
{
    /// <summary>
    /// BGM タイプ
    /// </summary>
    public enum BgmType
    {
        /// <summary>未設定</summary>
        None = 0,

        /// <summary>タイトル</summary>
        Title,

        /// <summary>メイン</summary>
        Main,

        /// <summary>リザルト</summary>
        Result
    }
}