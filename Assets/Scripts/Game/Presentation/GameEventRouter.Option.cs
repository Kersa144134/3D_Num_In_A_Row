// ======================================================
// GameEventRouter.Option.cs
// 作成者   : 高橋一翔
// 作成日時 : 2025-12-17
// 更新日時 : 2026-04-27
// 概要     : シーン内イベントの仲介を行うクラス
//            オプション関連処理をまとめたファイル
// ======================================================

using OptionSystem.Domain;

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
        /// ゲームオプション更新時の処理を行う
        /// </summary>
        private void HandleGameOptionUpdated(in OptionButtonData data)
        {
            switch (data.Type)
            {
                case OptionType.PlayerCount:
                    _gameOptionManager?.SetPlayerCount(data.IntValue);
                    break;

                case OptionType.LimitTime:
                    _gameOptionManager?.SetLimitTime(data.FloatValue);
                    break;

                case OptionType.BoardSize:
                    _gameOptionManager?.SetBoardSize(data.BoardSizeType);
                    break;

                case OptionType.ConnectCount:
                    _gameOptionManager?.SetConnectCount(data.IntValue);
                    break;

                case OptionType.CameraSpeed:
                    _gameOptionManager?.SetCameraSpeed(data.FloatValue);
                    break;

                case OptionType.PointerSpeed:
                    _gameOptionManager?.SetPointerSpeed(data.FloatValue);
                    break;
            }
        }
    }
}