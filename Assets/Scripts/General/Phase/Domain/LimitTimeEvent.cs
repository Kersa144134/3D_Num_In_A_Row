// ======================================================
// LimitTimeEvent.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-02
// 更新日時 : 2026-04-02
// 概要     : 制限時間関連イベントデータ
// ======================================================

namespace PhaseSystem.Domain
{
    /// <summary>
    /// 制限時間イベントデータ
    /// </summary>
    public readonly struct LimitTimeEvent
    {
        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>経過時間（秒）</summary>
        private readonly float _elapsedTime;

        /// <summary>制限時間（秒）</summary>
        private readonly float _limitTime;

        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>
        /// 残り時間（秒）
        /// </summary>
        public float RemainingTime
        {
            get
            {
                // 制限時間から経過時間を減算して残り時間を算出
                float remaining = _limitTime - _elapsedTime;

                // 残り時間が負数にならないように 0 でクランプ
                return remaining > 0.0f
                    ? remaining
                    : 0.0f;
            }
        }

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// LimitTimeEvent の生成
        /// </summary>
        /// <param name="elapsedTime">経過時間（秒）</param>
        /// <param name="limitTime">制限時間（秒）</param>
        public LimitTimeEvent(
            in float elapsedTime,
            in float limitTime)
        {
            _elapsedTime = elapsedTime;
            _limitTime = limitTime;
        }
    }
}