using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace GameDevKit.UI
{
    public class TextButton : Button
    {
        [SerializeField] private TMP_Text _text;

        public TMP_Text textComponent => _text;
        public string text
        {
            get => _text.text;
            set => _text.text = value;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _text = GetComponentInChildren<TMP_Text>(true);
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(TextButton))]
    public class TextButtonEditor : ButtonEditor
    {
        SerializedProperty _textProperty;

        protected override void OnEnable()
        {
            base.OnEnable();
            _textProperty = serializedObject.FindProperty("_text");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_textProperty);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
            // EditorGUILayout.Space();
        }
    }
#endif
}