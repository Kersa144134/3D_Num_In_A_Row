// ======================================================
// SeType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-26
// 更新日時 : 2026-06-09
// 概要     : SE タイプ定義
// ======================================================

namespace SoundSystem.Domain
{
    /// <summary>
    /// SE タイプ
    /// </summary>
    public enum SeType
    {
        /// <summary>未設定</summary>
        None = 0,

        // --------------------------------------------------
        // スコア
        // --------------------------------------------------
        /// <summary>コンボ × 1</summary>
        Combo_1 = 1,

        /// <summary>コンボ × 2</summary>
        Combo_2 = 2,

        /// <summary>コンボ × 3</summary>
        Combo_3 = 3,

        /// <summary>スコア 加算</summary>
        Score_Add = 6,

        /// <summary>スコア 減算</summary>
        Score_Subtract = 7,

        // --------------------------------------------------
        // UI
        // --------------------------------------------------
        /// <summary>UI 決定</summary>
        UI_Decide = 10,

        /// <summary>UI キャンセル</summary>
        UI_Cancel = 11,

        /// <summary>UI クリック</summary>
        UI_Click = 12,

        /// <summary>UI フォーカス</summary>
        UI_Focus = 13,

        /// <summary>UI ダイアログ表示</summary>
        UI_ShowDialog = 14,

        /// <summary>UI ダイアログ非表示</summary>
        UI_HideDialog = 15,

        /// <summary>UI ポーズ表示</summary>
        UI_ShowPause = 16,

        /// <summary>UI ダイアログ非表示</summary>
        UI_HidePause = 17,

        /// <summary>UI スキップ</summary>
        UI_Skip = 18,

        // --------------------------------------------------
        // カメラ
        // --------------------------------------------------
        /// <summary>カメラ 回転</summary>
        Camera_Rotation = 20,

        /// <summary>カメラ ズームアウト</summary>
        Camera_Zoom = 21,

        /// <summary>カメラ 投影切り替え</summary>
        Camera_SwitchProjection = 22,

        // --------------------------------------------------
        // ボード
        // --------------------------------------------------
        /// <summary>ボード 列選択</summary>
        Board_ColumnSelect = 30,

        /// <summary>ボード 回転</summary>
        Board_Rotate = 31,

        /// <summary>ピース 落下</summary>
        Piece_Drop = 32,

        /// <summary>ピース 発光</summary>
        Piece_Emission = 33,

        /// <summary>ピース 削除</summary>
        Piece_Delete = 34,

        // --------------------------------------------------
        // エフェクト（共通）
        // --------------------------------------------------
        /// <summary>共通 インパクト 小</summary>
        Effect_Impact_Small = 40,

        /// <summary>共通 インパクト 中</summary>
        Effect_Impact_Medium = 41,

        /// <summary>共通 インパクト 大</summary>
        Effect_Impact_Large = 42,
        
        /// <summary>共通 上昇</summary>
        Effect_Rise = 43,

        /// <summary>共通 下降</summary>
        Effect_Fall = 44,

        // --------------------------------------------------
        // エフェクト（タイトル）
        // --------------------------------------------------
        /// <summary>タイトル プレイヤーカットイン</summary>
        Effect_Title_PlayerCutIn = 50,

        // --------------------------------------------------
        // エフェクト（メイン）
        // --------------------------------------------------
        /// <summary>エフェクト プレイヤー切り替え</summary>
        Effect_Main_ChangePlayer = 60,

        // --------------------------------------------------
        // エフェクト（リザルト）
        // --------------------------------------------------
        /// <summary>エフェクト リザルト 1 位</summary>
        Effect_Result_1st = 70,

        /// <summary>エフェクト リザルト 2 位</summary>
        Effect_Result_2nd = 71,

        /// <summary>エフェクト リザルト 3 位</summary>
        Effect_Result_3rd = 72,

        /// <summary>エフェクト リザルト 4 位</summary>
        Effect_Result_4th = 73,
    }
}