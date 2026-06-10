// ======================================================
// DialogUICollector.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-20
// 更新日時 : 2026-05-20
// 概要     : ダイアログ UI のイベント購読対象を収集するクラス
// ======================================================

using System;
using System.Collections.Generic;
using UISystem.Infrastructure;

namespace UISystem.Application
{
    /// <summary>
    /// ダイアログ UI の各種イベント購読対象を収集する
    /// </summary>
    public sealed class DialogUICollector
    {
        // ======================================================
        // プロパティ
        // ======================================================

        /// <summary>ダイアログボタン</summary>
        public DialogButton[] Buttons { get; private set; } = Array.Empty<DialogButton>();

        /// <summary>パネルイベント</summary>
        public BasePanelEvent[] Panels { get; private set; } = Array.Empty<BasePanelEvent>();

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// ダイアログUIの購読対象を収集する
        /// </summary>
        /// <param name="canvasArray">ダイアログキャンバス定義配列</param>
        /// <returns>購読対象の集合</returns>
        public void Collect(in DialogCanvasDefinition[] canvasArray)
        {
            if (canvasArray == null)
            {
                // 空データを生成
                Buttons = Array.Empty<DialogButton>();
                Panels = Array.Empty<BasePanelEvent>();

                return;
            }

            // --------------------------------------------------
            // 一時格納リスト
            // --------------------------------------------------
            List<DialogButton> buttonList = new List<DialogButton>();
            List<BasePanelEvent> panelList = new List<BasePanelEvent>();

            // --------------------------------------------------
            // 収集処理
            // --------------------------------------------------
            for (int i = 0; i < canvasArray.Length; i++)
            {
                if (canvasArray[i] == null)
                {
                    continue;
                }

                DialogCanvasDefinition canvasDefinition = canvasArray[i];

                // --------------------------------------------------
                // ボタン
                // --------------------------------------------------
                if (canvasDefinition.Buttons != null)
                {
                    for (int j = 0; j < canvasDefinition.Buttons.Length; j++)
                    {
                        if (canvasDefinition.Buttons[j] == null)
                        {
                            continue;
                        }

                        DialogButton dialogButton = canvasDefinition.Buttons[j];

                        // ダイアログ種別を登録
                        dialogButton.Initialize(canvasDefinition.Type);

                        buttonList.Add(dialogButton);
                    }
                }

                // --------------------------------------------------
                // パネル
                // --------------------------------------------------
                BasePanelEvent[] panels =
                    canvasDefinition.Canvas.GetComponentsInChildren<BasePanelEvent>(true);

                if (panels != null)
                {
                    for (int j = 0; j < panels.Length; j++)
                    {
                        if (panels[j] == null)
                        {
                            continue;
                        }

                        panelList.Add(panels[j]);
                    }
                }
            }

            // --------------------------------------------------
            // 収集結果反映
            // --------------------------------------------------
            Buttons = buttonList.ToArray();
            Panels = panelList.ToArray();
        }
    }
}