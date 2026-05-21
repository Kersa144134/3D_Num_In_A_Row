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
    /// OptionButtonConfig の PropertyDrawer
    /// </summary>
    [CustomPropertyDrawer(typeof(OptionButtonConfig))]
    public sealed class OptionButtonConfigDrawer : PropertyDrawer
    {
        // ======================================================
        // 定数
        // ======================================================

        /// <summary>行数の合計</summary>
        private const int LINE_COUNT_TOTAL = 4;

        /// <summary>行間スペース</summary>
        private const float LINE_SPACING = 2f;

        /// <summary>種別ヘッダー表示文字</summary>
        private const string HEADER_TYPE = "種別";

        /// <summary>値ヘッダー表示文字</summary>
        private const string HEADER_VALUE = "値";

        /// <summary>
        /// GUI スタイル
        /// ヘッダー太字
        /// </summary>
        private static readonly GUIStyle HEADER_STYLE =
            new GUIStyle(EditorStyles.label)
            {
                fontStyle = FontStyle.Bold
            };

        // ======================================================
        // GUI描画
        // ======================================================

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            SerializedProperty typeProp = property.FindPropertyRelative("_type");
            SerializedProperty intValueProp = property.FindPropertyRelative("_intValue");
            SerializedProperty floatValueProp = property.FindPropertyRelative("_floatValue");
            SerializedProperty boardProp = property.FindPropertyRelative("_boardSizeType");

            float lineHeight = EditorGUIUtility.singleLineHeight;

            float y = position.y;

            // --------------------------------------------------
            // 1行目
            // Header（種別）
            // --------------------------------------------------
            DrawHeader(position, ref y, HEADER_TYPE, lineHeight);

            // --------------------------------------------------
            // 2行目
            // 種別 Enum
            // --------------------------------------------------
            EditorGUI.PropertyField(
                GetLineRect(position, y, lineHeight),
                typeProp);

            y += lineHeight + LINE_SPACING;

            OptionType type = (OptionType)typeProp.enumValueIndex;

            // --------------------------------------------------
            // 3行目
            // Header（値）
            // --------------------------------------------------
            DrawHeader(position, ref y, HEADER_VALUE, lineHeight);

            // --------------------------------------------------
            // 4行目
            // 値フィールド
            // --------------------------------------------------
            Rect valueRect = GetLineRect(position, y, lineHeight);

            switch (type)
            {
                case OptionType.PlayerCount:
                case OptionType.ConnectCount:
                    {
                        EditorGUI.PropertyField(valueRect, intValueProp);
                        break;
                    }

                case OptionType.LimitTime:
                case OptionType.CameraSpeed:
                case OptionType.PointerSpeed:
                    {
                        EditorGUI.PropertyField(valueRect, floatValueProp);
                        break;
                    }

                case OptionType.BoardSize:
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
            float lineHeight = EditorGUIUtility.singleLineHeight;

            return (lineHeight * LINE_COUNT_TOTAL) + (LINE_SPACING * (LINE_COUNT_TOTAL - 1));
        }

        // ======================================================
        // プライベートメソッド
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
        /// 行 Rect 生成
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