using EditorAttributes;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameDevKit.Editor
{
    public class StaticBatchingHelper : MonoBehaviour
    {
#if UNITY_EDITOR

        public bool Static = true;

        public ShadowCastingMode ShadowCastingMode = ShadowCastingMode.Off;
        public bool StaticShadowCaster = false;
        public bool ContributeGI = false;

        [Button]
        public void SetupChildren()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.gameObject.isStatic = Static;
                if (child.TryGetComponent<Renderer>(out var renderer))
                {
                    renderer.staticShadowCaster = StaticShadowCaster;
                    renderer.shadowCastingMode = ShadowCastingMode;
                }

                var flags = GameObjectUtility.GetStaticEditorFlags(child.gameObject);
                if (ContributeGI)
                {
                    flags |= StaticEditorFlags.ContributeGI;
                }
                else
                {
                    flags &= ~StaticEditorFlags.ContributeGI;
                }

                GameObjectUtility.SetStaticEditorFlags(child.gameObject, flags);

            }

            Debug.Log($"Setup {transform.childCount} children");
        }

#endif

    }
}