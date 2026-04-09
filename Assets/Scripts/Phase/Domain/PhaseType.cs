// ======================================================
// PhaseType.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-05
// 更新日時 : 2026-01-23
// 概要     : ゲーム内フェーズを識別する列挙体
// ======================================================

namespace PhaseSystem.Domain
{
    /// <summary>
    /// ゲーム全体の進行状態を示すフェーズ列挙
    /// </summary>
    public enum PhaseType
    {
        /// <summary>初期値用</summary>
        None,

        /// <summary>タイトルフェーズ</summary>
        Title,

        /// <summary>ゲーム開始前フェーズ</summary>
        Ready,

        /// <summary>ゲーム内の 1P 通常フェーズ</summary>
        Play_1,

        /// <summary>ゲーム内の 1P 通常フェーズ</summary>
        Play_2,

        /// <summary>イベント中フェーズ</summary>
        Event,

        /// <summary>一時停止中フェーズ</summary>
        Pause,

        /// <summary>ゲーム終了後フェーズ</summary>
        Finish,

        /// <summary>リザルトフェーズ</summary>
        Result,
    }
}