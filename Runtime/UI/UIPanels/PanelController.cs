using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameDevKit.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit.UI
{
    [Serializable]
    public class PanelController<T> : IPanelNavigation<T> where T : Panel<T>
    {
        [SerializeField] protected T[] _panels;

        [SubclassPicker]
        [SerializeReference] private IPanelTransition _defaultTransition;

        public T CurrentPanel { get; protected set; }

        public IReadOnlyList<T> Panels => _panels;

        public UnityEvent<T> OnPanelChanged;

        private bool _isTransitioning;

        public virtual void Init(int firstPanelIndex = 0)
        {
            foreach (var panel in _panels)
            {
                panel.gameObject.SetActive(true);
                panel.Init(this, _defaultTransition);
                if (!panel.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"Panel {panel.name} is not active in hierarchy on init!", panel);
                }
                panel.gameObject.SetActive(false);
            }

            GoTo(firstPanelIndex, false).Forget();
        }

        public UniTask GoTo<U>(bool animate = true) where U : T => GoTo(Get<U>(), animate);

        public UniTask GoTo(int index, bool animate = true) => GoTo(_panels[index], animate);

        public virtual async UniTask GoTo(T panel, bool animate = true)
        {
            if (_isTransitioning) { return; }
            _isTransitioning = true;
            try
            {
                var isSamePanel = CurrentPanel != null && panel == CurrentPanel;

                panel.OnShow();

                if (!isSamePanel && animate)
                {
                    await AnimatePanelTransition(CurrentPanel, panel);
                }

                if (isSamePanel) { return; }

                CurrentPanel?.OnHide();
                CurrentPanel = panel;

                OnPanelChanged?.Invoke(CurrentPanel);
            }
            finally
            {
                _isTransitioning = false;
            }
        }

        public U Get<U>() where U : T => _panels.Find(match => match is U) as U;

        protected virtual UniTask AnimatePanelTransition(T fromPanel, T toPanel)
        {
            var transitionOutTask = fromPanel != null ? fromPanel.TransitionOut(toPanel) : UniTask.CompletedTask;
            var transitionInTask = toPanel.TransitionIn(fromPanel);

            return UniTask.WhenAll(transitionOutTask, transitionInTask);
        }
    }

    public interface IPanelNavigation<T> where T : Panel<T>
    {
        UniTask GoTo(T panel, bool animate = true);
    }
}