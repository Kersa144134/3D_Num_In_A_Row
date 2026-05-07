// ======================================================
// OptionButtonConfigDrawer.cs
// 作成者   : 高橋一翔
// 作成日時 : 2026-05-07
// 更新日時 : 2026-05-07
// 概要     : OptionButtonConfig のインスペクタ描画制御
// ======================================================

using UnityEditor;
using UnityEngine;
using OptionSystem.Domain;

namespace EditorSystem.PropertyDrawers
{
    /// <summary>
    /// OptionButtonConfig のカスタムPropertyDrawer
    /// 種別に応じて入力フィールドを切り替えつつ
    /// 固定レイアウトでインスペクタ崩れを防止する
    /// </summary>
    [CustomPropertyDrawer(typeof(OptionButtonConfig))]
    public sealed class OptionButtonConfigDrawer : PropertyDrawer
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>行数：合計</summary>
        private const int LINE_COUNT_TOTAL = 4;

        /// <summary>行数：ヘッダー</summary>
        private const int LINE_COUNT_HEADER = 2;

        /// <summary>行数：値領域</summary>
        private const int LINE_COUNT_VALUE = 2;

        /// <summary>行間スペース</summary>
        private const float LINE_SPACING = 2f;

        /// <summary>ヘッダー表示文字：種別</summary>
        private const string HEADER_TYPE = "種別";

        /// <summary>ヘッダー表示文字：値</summary>
        private const string HEADER_VALUE = "値";

        /// <summary>GUIスタイル（ヘッダー太字）</summary>
        private static readonly GUIStyle HEADER_STYLE =
            new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold
            };

        // ======================================================
        // GUI描画
        // ======================================================

        /// <summary>
        /// インスペクタ描画処理
        /// </summary>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty typeProp =
                property.FindPropertyRelative("_type");

            SerializedProperty intValueProp =
                property.FindPropertyRelative("_intValue");

            SerializedProperty floatValueProp =
                property.FindPropertyRelative("_floatValue");

            SerializedProperty boardProp =
                property.FindPropertyRelative("_boardSizeType");

            float lineHeight =
                EditorGUIUtility.singleLineHeight;

            float y =
                position.y;

            // ==================================================
            // 1行目：Header（種別）
            // ==================================================
            DrawHeader(position, ref y, HEADER_TYPE, lineHeight);

            // ==================================================
            // 2行目：種別Enum
            // ==================================================
            EditorGUI.PropertyField(
                GetLineRect(position, y, lineHeight),
                typeProp);

            y += lineHeight + LINE_SPACING;

            OptionButtonConfig.OptionType type =
                (OptionButtonConfig.OptionType)typeProp.enumValueIndex;

            // ==================================================
            // 3行目：Header（値）
            // ==================================================
            DrawHeader(position, ref y, HEADER_VALUE, lineHeight);

            // ==================================================
            // 4行目：値フィールド
            // ==================================================
            Rect valueRect =
                GetLineRect(position, y, lineHeight);

            switch (type)
            {
                case OptionButtonConfig.OptionType.PlayerCount:
                case OptionButtonConfig.OptionType.ConnectCount:
                    {
                        EditorGUI.PropertyField(valueRect, intValueProp);
                        break;
                    }

                case OptionButtonConfig.OptionType.LimitTime:
                case OptionButtonConfig.OptionType.CameraRotationSpeed:
                case OptionButtonConfig.OptionType.PointerSpeed:
                    {
                        EditorGUI.PropertyField(valueRect, floatValueProp);
                        break;
                    }

                case OptionButtonConfig.OptionType.BoardSize:
                    {
                        EditorGUI.PropertyField(valueRect, boardProp);
                        break;
                    }
            }

            EditorGUI.EndProperty();
        }

        // ======================================================
        // 高さ計算
        // ======================================================

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight =
                EditorGUIUtility.singleLineHeight;

            return (lineHeight * LINE_COUNT_TOTAL)
                   + (LINE_SPACING * (LINE_COUNT_TOTAL - 1));
        }

        // ======================================================
        // 共通描画処理
        // ======================================================

        /// <summary>
        /// ヘッダー描画
        /// </summary>
        private void DrawHeader(Rect position, ref float y, string text, float lineHeight)
        {
            EditorGUI.LabelField(
                GetLineRect(position, y, lineHeight),
                text,
                HEADER_STYLE);

            y += lineHeight + LINE_SPACING;
        }

        /// <summary>
        /// 行Rect生成
        /// </summary>
        private Rect GetLineRect(Rect position, float y, float lineHeight)
        {
            return new Rect(
                position.x,
                y,
                position.width,
                lineHeight);
        }
    }
}