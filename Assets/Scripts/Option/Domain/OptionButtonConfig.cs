// ======================================================
// OptionButtonConfig.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 概要     : インスペクタ編集用オプション設定データ
// ======================================================

using System;
using UnityEngine;
using UnityEngine.UI;

namespace OptionSystem.Domain
{
    /// <summary>
    /// インスペクタから直接設定するためのデータ
    /// </summary>
    [Serializable]
    public sealed class OptionButtonConfig
    {
        // ======================================================
        // インスペクタ設定
        // ======================================================

        [SerializeField]
        private OptionType _type;

        [SerializeField]
        private int _intValue;

        [SerializeField]
        private float _floatValue;

        [SerializeField]
        private GameRules.BoardSizeType _boardSizeType;

        // ======================================================
        // 変換処理
        // ======================================================

        /// <summary>
        /// 実行時データへ変換する
        /// </summary>
        public OptionButtonData ToRuntimeData(Button button)
        {
            switch (_type)
            {
                case OptionType.PlayerCount:
                    return new OptionButtonData(button, OptionType.PlayerCount, _intValue);

                case OptionType.LimitTime:
                    return new OptionButtonData(button, OptionType.LimitTime, _floatValue);

                case OptionType.BoardSize:
                    return new OptionButtonData(button, _boardSizeType);

                case OptionType.ConnectCount:
                    return new OptionButtonData(button, OptionType.ConnectCount, _intValue);

                case OptionType.CameraRotationSpeed:
                    return new OptionButtonData(button, OptionType.CameraRotationSpeed, _floatValue);

                case OptionType.PointerSpeed:
                    return new OptionButtonData(button, OptionType.PointerSpeed, _floatValue);

                default:
                    return null;
            }
        }
    }
}