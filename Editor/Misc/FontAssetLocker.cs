using System;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEngine;

// https://discussions.unity.com/t/tmpro-dynamic-font-asset-constantly-changes-in-source-control/868941/35

namespace GameDevKit.Editor
{
    [CustomEditor(typeof(TMP_FontAsset))]
    public class FontAssetSaveDisabler : TMP_FontAssetEditor
    {
        private static bool FontAssetsLocked
        {
            get => EditorPrefs.GetBool("TMPFontsLocked", false);
            set => EditorPrefs.SetBool("TMPFontsLocked", value);
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var label = FontAssetsLocked ? "Locked by" : "Unlocked by";
                EditorGUILayout.ObjectField(label, MonoScript.FromScriptableObject(this), GetType(), false);
            }

            if (GUILayout.Button(FontAssetsLocked ? "Unlock Font Assets" : "Lock Font Assets"))
            {
                FontAssetsLocked = !FontAssetsLocked;
                GUIUtility.ExitGUI();
            }

            if (!FontAssetsLocked)
            {
                EditorGUILayout.Space();
                base.OnInspectorGUI();
            }
        }

        private class SaveHandler : AssetModificationProcessor
        {
            private static string[] OnWillSaveAssets(string[] paths)
            {
                if (FontAssetsLocked)
                {
                    int index = 0;
                    foreach (string path in paths)
                    {
                        if (!typeof(TMP_FontAsset).IsAssignableFrom(AssetDatabase.GetMainAssetTypeAtPath(path)))
                            paths[index++] = path;
                    }

                    Array.Resize(ref paths, index);
                }

                return paths;
            }
        }
    }
}