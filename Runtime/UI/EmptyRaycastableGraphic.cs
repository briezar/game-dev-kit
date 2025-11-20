using UnityEngine;
using UnityEngine.UI;
using UnityEditor.UI;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevKit.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class EmptyRaycastableGraphic : Graphic
    {
        protected override void UpdateGeometry() { }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EmptyRaycastableGraphic))]
    internal class EmptyRaycastableGraphicDrawer : GraphicEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Script);
            // AppearanceControlsGUI();
            RaycastControlsGUI();
            // MaskableControlsGUI();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}