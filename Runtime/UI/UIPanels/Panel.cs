using System;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using GameDevKit.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.UI
{
    public abstract class Panel : AdvancedBehaviour
    {
        [Tooltip("The index of the panel. -1 will default to SiblingIndex.")]
        [SerializeField] protected int _panelIndex = -1;

        [SubclassPicker]
        [SerializeReference] protected IPanelTransition _transition;

        /// <summary> -1 will default to SiblingIndex </summary>
        [ShowInInspector]
        public int PanelIndex => _panelIndex < 0 ? transform.GetSiblingIndex() : _panelIndex;

        public virtual void OnShow() => gameObject.SetActive(true);
        public virtual void OnHide() => gameObject.SetActive(false);

        public virtual UniTask TransitionIn(Panel fromPanel) => _transition?.TransitionIn(fromPanel, this) ?? UniTask.CompletedTask;
        public virtual UniTask TransitionOut(Panel toPanel) => _transition?.TransitionOut(this, toPanel) ?? UniTask.CompletedTask;
    }

    public abstract class Panel<T> : Panel where T : Panel<T>
    {
        private IPanelNavigation<T> _panelNavigation;

        public UnityEvent OnGoNext, OnGoPrev;

        [field: SerializeField] public T PrevPanel { get; protected set; }
        [field: SerializeField] public T NextPanel { get; protected set; }
        [field: SerializeField] public bool AllowBackInput { get; set; } = true;

        public void Init(IPanelNavigation<T> panelNavigation, IPanelTransition defaultTransition)
        {
            _panelNavigation = panelNavigation;
            _transition ??= defaultTransition;
        }

        public void SetNavigation(T prevPanel, T nextPanel) => (PrevPanel, NextPanel) = (prevPanel, nextPanel);

        public void GoNext()
        {
            OnGoNext?.Invoke();
            _panelNavigation.GoTo(NextPanel, true);
        }

        public void GoPrev()
        {
            OnGoPrev?.Invoke();
            _panelNavigation.GoTo(PrevPanel, true);
        }

#if UNITY_EDITOR
        [Button]
        private void Select()
        {
            foreach (var panel in transform.parent.GetComponentsInChildren<Panel<T>>(true))
            {
                panel.gameObject.SetActive(panel == this);
            }
        }
#endif
    }
}