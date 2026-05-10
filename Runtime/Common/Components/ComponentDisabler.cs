using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit
{
    public class ComponentDisabler : MonoBehaviour
    {
        public enum DisableMode { DisableGameObject, DisableBehaviour }

        [TypeFilter(typeof(Component))]
        public SerializableType TypeToDisable;

        [Tooltip("DisableMode.DisableBehaviour only works if TypeToDisable derives from UnityEngine.Behaviour")]
        public DisableMode Mode = DisableMode.DisableGameObject;

        private readonly Dictionary<Component, bool> _originalStatesMap = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Mode is DisableMode.DisableBehaviour && !TypeToDisable.Type.Implements<Behaviour>())
            {
                Mode = DisableMode.DisableGameObject;
                Debug.Log($"{nameof(Mode)} has been changed to {DisableMode.DisableGameObject} because {TypeToDisable.Type.Name} does not implement {typeof(Behaviour)}");
            }
        }
#endif

        [Button]
        public void DisableComponents()
        {
            if (Mode is DisableMode.DisableBehaviour && !TypeToDisable.Type.Implements<Behaviour>())
            {
                Debug.Log($"{DisableMode.DisableBehaviour} is not applicable for type {TypeToDisable.Type.Name} because it does not implement {typeof(Behaviour)}");
                return;
            }

            var components = GetComponentsInChildren(TypeToDisable);
            foreach (var component in components)
            {
                if (_originalStatesMap.ContainsKey(component)) { continue; }

                var state = false;
                switch (Mode)
                {
                    case DisableMode.DisableGameObject:
                        state = component.gameObject.activeSelf;
                        component.gameObject.SetActive(false);
                        break;
                    case DisableMode.DisableBehaviour:
                        if (component is Behaviour behaviour)
                        {
                            state = behaviour.enabled;
                            behaviour.enabled = false;
                        }
                        break;
                }

                _originalStatesMap[component] = state;
            }
        }

        [Button]
        public void Revert()
        {
            foreach (var (component, originalState) in _originalStatesMap)
            {
                if (component == null) { continue; }
                switch (Mode)
                {
                    case DisableMode.DisableGameObject:
                        component.gameObject.SetActive(originalState);
                        break;
                    case DisableMode.DisableBehaviour:
                        if (component is Behaviour behaviour)
                        {
                            behaviour.enabled = originalState;
                        }
                        break;
                }
            }

            _originalStatesMap.Clear();
        }
    }
}