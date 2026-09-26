using UnityEngine;
using UnityEditor;

namespace GameDevKit.Editor
{
    [CustomPropertyDrawer(typeof(AnimationHash))]
    public class AnimationHashDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var paramNameProp = property.FindPropertyRelative(AnimationHash.EditorProps.ParamName);
            var valueProp = property.FindPropertyRelative(AnimationHash.EditorProps.Value);

            var lineHeight = EditorGUIUtility.singleLineHeight;
            var spacing = 2f;

            var labelRect = new Rect(position.x, position.y, position.width, lineHeight);
            var fieldRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);
            var hashRect = new Rect(position.x, position.y + (lineHeight + spacing) * 2, position.width, lineHeight);

            EditorGUI.LabelField(labelRect, label.text);

            EditorGUI.indentLevel++;

            EditorGUI.BeginChangeCheck();
            var newParamName = EditorGUI.TextField(fieldRect, "Param Name", paramNameProp.stringValue);
            if (EditorGUI.EndChangeCheck())
            {
                paramNameProp.stringValue = newParamName;
                valueProp.intValue = Animator.StringToHash(newParamName);
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.IntField(hashRect, "Hash", valueProp.intValue);
            }

            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + spacing * 2;
        }

        private const float spacing = 2f;
    }
}