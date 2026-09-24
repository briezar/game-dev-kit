using System;
using Cysharp.Threading.Tasks;
using GameDevKit.Attributes;
using UnityEngine;
using UnityEngine.Pool;

namespace GameDevKit.UI
{
    public interface IPanelTransition
    {
        UniTask TransitionIn(Panel fromPanel, Panel toPanel);
        UniTask TransitionOut(Panel fromPanel, Panel toPanel);
    }

    [CreateAssetMenu(menuName = "UI/Panel Transition")]
    public class PanelTransitionSO : ScriptableObject, IPanelTransition
    {
        [SubclassPicker]
        [SerializeReference] private IPanelTransition _transition;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_transition is not ScriptableObjectPanelTransition soPanelTransition) { return; }

            using var _ = HashSetPool<PanelTransitionSO>.Get(out var visited);
            using var __ = ListPool<PanelTransitionSO>.Get(out var chain);

            visited.Add(this);
            chain.Add(this);

            var current = soPanelTransition.Transition;

            while (current != null)
            {
                chain.Add(current);

                if (!visited.Add(current))
                {
                    Debug.LogError($"Circular panel transition reference detected: {chain.JoinToString(" -> ")}", this);
                    soPanelTransition.Transition = null;
                    return;
                }

                current = (current._transition as ScriptableObjectPanelTransition)?.Transition;
            }
        }
#endif

        public UniTask TransitionIn(Panel fromPanel, Panel toPanel) => _transition.TransitionIn(fromPanel, toPanel);
        public UniTask TransitionOut(Panel fromPanel, Panel toPanel) => _transition.TransitionOut(fromPanel, toPanel);
    }

    [Serializable]
    public class ScriptableObjectPanelTransition : IPanelTransition
    {
        public PanelTransitionSO Transition;

        public UniTask TransitionIn(Panel fromPanel, Panel toPanel) => Transition.TransitionIn(fromPanel, toPanel);
        public UniTask TransitionOut(Panel fromPanel, Panel toPanel) => Transition.TransitionOut(fromPanel, toPanel);
    }
}