// ======================================================
// GameEventRouter.Phase.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            フェーズ関連処理をまとめたファイル
// ======================================================

using PhaseSystem.Domain;

namespace GameSystem.Presentation
{
    /// <summary>
    /// シーン内イベントを仲介するクラス
    /// </summary>
    public sealed partial class GameEventRouter
    {
        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// フェーズ変更を通知する
        /// </summary>
        private void NotifyPhaseChanged(in PhaseType nextPhase)
        {
            _onPhaseChanged.OnNext(
                new PhaseChangeEvent(
                    _currentPhase.Value,
                    nextPhase
                )
            );
        }

        /// <summary>
        /// ポーズ用のフェーズトグル処理を行う
        /// </summary>
        private void TogglePausePhase(in PhaseType phase)
        {
            // フェーズ遷移先を決定する
            PhaseType nextPhase;

            if (phase == PhaseType.Play)
            {
                nextPhase = PhaseType.Pause;

                // 現在のアクティブ状態フェーズをキャッシュ
                _cachedActivePhase = phase;
            }
            else if (phase == PhaseType.Pause)
            {
                // キャッシュしていたアクティブ状態フェーズへ復帰
                nextPhase = _cachedActivePhase;
            }
            else
            {
                return;
            }

            NotifyPhaseChanged(nextPhase);
        }
    }
}