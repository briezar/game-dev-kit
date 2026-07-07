using System;
using System.Collections.Generic;
using System.Linq;
using EditorAttributes;
using UnityEngine;

#if UNITY_EDITOR
using GameDevKit.Editor;
#endif

namespace GameDevKit
{
    public class RegisterToGlobalContainerAttribute : Attribute { }

    public class ScriptableObjectContainer : SingletonBehaviour<ScriptableObjectContainer>
    {
        [SerializeField] private List<ScriptableObject> _scriptableObjects = new();

        public static T Find<T>() where T : ScriptableObject
        {
            if (_instance == null)
            {
                Debug.LogError($"No instance of {nameof(ScriptableObjectContainer)} found. Did you forget to add one to the scene?");
                return null;
            }

            var objType = typeof(T);
            var obj = _instance._scriptableObjects.Find(r => r is T) as T;
            if (obj == null)
            {
#if UNITY_EDITOR
                if (objType.HasAttribute<RegisterToGlobalContainerAttribute>())
                {
                    obj = EditorUtils.FindAssets<T>().FirstOrDefault();
                    if (obj == null)
                    {
                        Debug.LogWarning($"Cannot find any instance of {objType.Name}!");
                        return null;
                    }

                    _instance._scriptableObjects.Add(obj);
                    Debug.Log($"Auto-registered {objType.Name}", obj);

                    return obj;
                }
                else
                {
                    Debug.LogWarning($"{objType.Name} does not have {nameof(RegisterToGlobalContainerAttribute)}!");
                }
#endif

                Debug.LogWarning($"ScriptableObject {objType.Name} is not registered!", _instance);
            }
            return obj;
        }

        public static bool AssignIfNull<T>(ref T data) where T : ScriptableObject
        {
            if (data == null)
            {
                data = Find<T>();
                return data != null;
            }
            return false;
        }

#if UNITY_EDITOR
        [Button]
        private void FindAndRegister()
        {
            _scriptableObjects.Clear();
            var scriptableObjects = EditorUtils.FindAssets<ScriptableObject>();
            foreach (var scriptableObject in scriptableObjects)
            {
                if (scriptableObject.HasAttribute<RegisterToGlobalContainerAttribute>())
                {
                    _scriptableObjects.Add(scriptableObject);
                }
            }

            Debug.Log($"Registered {_scriptableObjects.Count} ScriptableObjects to global container", this);
        }
#endif
    }
}