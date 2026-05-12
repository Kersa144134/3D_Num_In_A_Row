// ======================================================
// ButtonDictionaryBuilder.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-12
// 更新日時 : 2026-05-12
// 概要     : UI のボタン辞書およびバインダー構築を行うクラス
// ======================================================

using System.Collections.Generic;
using UnityEngine.UI;
using OptionSystem.Domain;
using UISystem.Domain;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// UIの辞書・バインダー・イベント登録を構築するクラス
    /// </summary>
    public sealed class ButtonDictionaryBuilder
    {
        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// NormalButton 辞書の構築およびイベント登録を行う
        /// </summary>
        /// <param name="normalButtons">対象の NormalButton リスト</param>
        public Dictionary<UIActionType, NormalButtonEvent> BuildNormalButtons(
            in List<NormalButton> normalButtons)
        {
            // --------------------------------------------------
            // 辞書生成
            // --------------------------------------------------
            Dictionary<UIActionType, NormalButtonEvent> table = new Dictionary<UIActionType, NormalButtonEvent>();

            // --------------------------------------------------
            // NormalButtonEvent 取得と辞書登録
            // --------------------------------------------------
            foreach (NormalButton normalButton in normalButtons)
            {
                if (normalButton == null || normalButton.Button == null)
                {
                    continue;
                }

                table[normalButton.Type] = normalButton.Button.GetComponent<NormalButtonEvent>();
            }

            return table;
        }

        /// <summary>
        /// OptionButtonBinder辞書の構築およびイベント登録を行う
        /// </summary>
        /// <param name="factory">Binder生成ファクトリ</param>
        /// <param name="playerCountButtons">プレイヤー数UI</param>
        /// <param name="limitTimeButtons">制限時間UI</param>
        /// <param name="boardSizeButtons">ボードサイズUI</param>
        /// <param name="connectCountButtons">連結数UI</param>
        /// <param name="cameraRotationSpeedButtons">カメラ速度UI</param>
        /// <param name="pointerSpeedButtons">ポインター速度UI</param>
        public Dictionary<OptionType, OptionButtonBinder> BuildOptionButtons(
            in OptionButtonBinderFactory factory,
            in GridLayoutGroup playerCountButtons,
            in GridLayoutGroup limitTimeButtons,
            in GridLayoutGroup boardSizeButtons,
            in GridLayoutGroup connectCountButtons,
            in GridLayoutGroup cameraRotationSpeedButtons,
            in GridLayoutGroup pointerSpeedButtons)
        {
            // --------------------------------------------------
            // 辞書生成
            // --------------------------------------------------
            Dictionary<OptionType, OptionButtonBinder> binders
                = new Dictionary<OptionType, OptionButtonBinder>();

            // --------------------------------------------------
            // OptionButtonBinder 生成と辞書登録
            // --------------------------------------------------
            // プレイヤー人数オプション
            binders[OptionType.PlayerCount] =factory.Create(
                OptionType.PlayerCount, playerCountButtons);

            // 制限時間オプション
            binders[OptionType.LimitTime] =factory.Create(
                OptionType.LimitTime, limitTimeButtons);

            // ボードサイズオプション
            binders[OptionType.BoardSize] = factory.Create(
                OptionType.BoardSize, boardSizeButtons);

            // 連結数オプション
            binders[OptionType.ConnectCount] = factory.Create(
                OptionType.ConnectCount, connectCountButtons);

            // カメラ回転速度オプション
            binders[OptionType.CameraRotationSpeed] = factory.Create(
                OptionType.CameraRotationSpeed, cameraRotationSpeedButtons);

            // ポインター速度オプション
            binders[OptionType.PointerSpeed] = factory.Create(
                OptionType.PointerSpeed, pointerSpeedButtons);

            return binders;
        }
    }
}