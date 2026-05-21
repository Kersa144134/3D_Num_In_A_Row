// ======================================================
// OptionType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-08
// 概要     : オプション種別情報
// ======================================================

namespace OptionSystem.Domain
{
    // ======================================================
    // 列挙型
    // ======================================================

    /// <summary>
    /// オプション種別
    /// </summary>
    public enum OptionType
    {
        /// <summary>プレイヤー人数</summary>
        PlayerCount,

        /// <summary>制限時間</summary>
        LimitTime,

        /// <summary>ボードサイズ</summary>
        BoardSize,

        /// <summary>ライン成立条件</summary>
        ConnectCount,

        /// <summary>カメラ速度</summary>
        CameraSpeed,

        /// <summary>ポインター速度</summary>
        PointerSpeed
    }
}