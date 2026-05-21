// ======================================================
// UIActionType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : UI 全体で共通利用する入力アクション識別子
// ======================================================

namespace UISystem.Domain
{
    /// <summary>
    /// UI 全体で共通利用する入力アクション種別
    /// </summary>
    public enum UIActionType
    {
        // ======================================================
        // 共通
        // ======================================================

        /// <summary>未指定</summary>
        None,

        /// <summary>ダイアログ YES</summary>
        DialogYes,

        /// <summary>ダイアログ NO</summary>
        DialogNo,

        // ======================================================
        // タイトル
        // ======================================================

        /// <summary>スタートボタン</summary>
        TitleStart,

        /// <summary>オプションボタン</summary>
        TitleOption,

        /// <summary>オプションキャンセルボタン</summary>
        TitleOptionCancel,

        /// <summary>オプション決定ボタン</summary>
        TitleOptionDecide,

        // ======================================================
        // オプション
        // ======================================================

        /// <summary>プレイヤー人数オプション</summary>
        OptionPlayerCount,

        /// <summary>制限時間オプション</summary>
        OptionLimitTime,

        /// <summary>ボードサイズオプション</summary>
        OptionBoardSize,

        /// <summary>連結数オプション</summary>
        OptionConnectCount,

        /// <summary>カメラ回転速度オプション</summary>
        OptionCameraSpeed,

        /// <summary>ポインター速度オプション</summary>
        OptionPointerSpeed
    }
}