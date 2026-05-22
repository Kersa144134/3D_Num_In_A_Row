// ======================================================
// InputIconCollector.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-22
// 更新日時 : 2026-05-22
// 概要     : 入力アイコンを収集・分類するクラス
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UISystem.Infrastructure
{
    /// <summary>
    /// 入力アイコン収集クラス
    /// </summary>
    public sealed class InputIconCollector
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>Gamepadタグ名</summary>
        private const string TAG_GAMEPAD = "Gamepad";

        /// <summary>Virtualpadタグ名</summary>
        private const string TAG_VIRTUALPAD = "Virtualpad";

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 入力アイコンを収集する
        /// </summary>
        /// <param name="intermittentCanvas">断続更新対象キャンバス</param>
        /// <param name="outgameCanvas">アウトゲームキャンバス</param>
        /// <param name="gamepadIcons">Gamepadアイコン配列</param>
        /// <param name="virtualpadIcons">Virtualpadアイコン配列</param>
        public void CollectInputIcons(
            in GameObject intermittentCanvas,
            in GameObject outgameCanvas,
            out Image[] gamepadIcons,
            out Image[] virtualpadIcons)
        {
            // Image 格納用リスト
            List<Image> gamepadList = new List<Image>();
            List<Image> virtualpadList = new List<Image>();

            // キャンバスから Image を収集
            CollectFromCanvas(intermittentCanvas, gamepadList, virtualpadList);
            CollectFromCanvas(outgameCanvas, gamepadList, virtualpadList);

            // List から配列へ変換
            gamepadIcons = gamepadList.ToArray();
            virtualpadIcons = virtualpadList.ToArray();
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 指定キャンバスから Image を収集する
        /// </summary>
        private void CollectFromCanvas(
            in GameObject canvas,
            in List<Image> gamepadList,
            in List<Image> virtualpadList)
        {
            if (canvas == null)
            {
                return;
            }

            // 全 Image コンポーネントを取得
            Image[] images = canvas.GetComponentsInChildren<Image>(true);
            
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];

                if (image == null)
                {
                    continue;
                }

                // タグ取得
                string tag = image.gameObject.tag;

                // Gamepad
                if (tag == TAG_GAMEPAD)
                {
                    gamepadList.Add(image);
                    continue;
                }

                // Virtualpad
                if (tag == TAG_VIRTUALPAD)
                {
                    virtualpadList.Add(image);
                }
            }
        }
    }
}