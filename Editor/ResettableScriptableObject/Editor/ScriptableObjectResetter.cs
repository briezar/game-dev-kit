using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GameDevKit.Editor
{
    [InitializeOnLoad]
    public static class ScriptableObjectResetter
    {
        private static readonly Dictionary<ScriptableObject, string> _objectStates = new();

        static ScriptableObjectResetter()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    SaveStates();
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    RestoreStates();
                    break;
            }
        }

        private static void SaveStates()
        {
            _objectStates.Clear();
            foreach (var obj in ScriptableObjectResetManager.instance.EnumerateResettableObjects())
            {
                _objectStates[obj] = JsonUtility.ToJson(obj);
            }
        }

        private static void RestoreStates()
        {
            if (_objectStates.Count == 0) { return; }
            foreach (var (obj, state) in _objectStates)
            {
                try
                {
                    JsonUtility.FromJsonOverwrite(state, obj);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to reset {obj?.name}\n{ex}");
                }
            }
            Debug.Log($"Reset {_objectStates.Count} {nameof(ScriptableObject)}(s): {_objectStates.Keys.JoinToString(s => s.name)}");
            _objectStates.Clear();
        }
    }

}