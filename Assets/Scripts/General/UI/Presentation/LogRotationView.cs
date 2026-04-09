// ======================================================
// LogRotationView.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-04-09
// 更新日時 : 2026-04-09
// 概要     : ログ UI の描画・補間移動・Text 反映を行うビュー
// ======================================================

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UISystem.Domain;

namespace UISystem.Presentation
{
    /// <summary>
    /// ログ描画ビュー
    /// </summary>
    public sealed class LogRotationView
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>移動速度</summary>
        private const float MOVE_SPEED = 2000.0f;

        // ======================================================
        // フィールド
        // ======================================================

        /// <summary>使用する Text 配列</summary>
        private readonly TextMeshProUGUI[] _texts;

        /// <summary>RectTransform キャッシュ</summary>
        private readonly RectTransform[] _rects;

        /// <summary>循環インデックス</summary>
        private int _currentIndex;

        // ======================================================
        // コンストラクタ
        // ======================================================

        /// <summary>
        /// 初期化
        /// </summary>
        public LogRotationView(in TextMeshProUGUI[] texts)
        {
            _texts = texts;

            _rects = new RectTransform[texts.Length];

            // RectTransform をキャッシュ
            for (int i = 0; i < texts.Length; i++)
            {
                _rects[i] = texts[i].rectTransform;
            }

            _currentIndex = 0;
        }

        // ======================================================
        // パブリックメソッド
        // ======================================================

        /// <summary>
        /// 描画反映
        /// </summary>
        public void Apply(
            in List<LogViewData> viewDataList,
            in float deltaTime)
        {
            float moveDelta = MOVE_SPEED * deltaTime;

            for (int i = 0; i < viewDataList.Count; i++)
            {
                LogViewData data = viewDataList[i];

                // 使用する Text を取得
                TextMeshProUGUI text = GetNextText();

                RectTransform rect = _rects[_currentIndex];

                // テキスト反映
                text.text = data.Message;

                // 補間移動
                Move(rect, data.TargetPosition, moveDelta);
            }
        }

        // ======================================================
        // プライベートメソッド
        // ======================================================

        /// <summary>
        /// 次に使用する Text を取得
        /// </summary>
        private TextMeshProUGUI GetNextText()
        {
            TextMeshProUGUI text = _texts[_currentIndex];

            _currentIndex++;

            if (_currentIndex >= _texts.Length)
            {
                _currentIndex = 0;
            }

            return text;
        }

        /// <summary>
        /// 補間移動
        /// </summary>
        private void Move(
            in RectTransform rect,
            in Vector2 target,
            in float moveDelta)
        {
            Vector2 current = rect.anchoredPosition;

            float nextX =
                Mathf.MoveTowards(current.x, target.x, moveDelta);

            float nextY =
                Mathf.MoveTowards(current.y, target.y, moveDelta);

            rect.anchoredPosition =
                new Vector2(nextX, nextY);
        }
    }
}