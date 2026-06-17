using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameDevKit.UI
{
    [Serializable]
    public class PanelController<T> where T : CommonPanel<T>
    {
        [SerializeField] protected T[] _panels;

        public T CurrentPanel { get; protected set; }

        public IReadOnlyList<T> Panels => _panels;

        public Action<T> OnPanelChanged;

        public virtual void Init(int firstPanelIndex = 0)
        {
            foreach (var panel in _panels)
            {
                panel.gameObject.SetActive(true);
                if (!panel.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"Panel {panel.name} is not active in hierarchy on init!", panel);
                }
                panel.gameObject.SetActive(false);
            }

            GoTo(firstPanelIndex, false);
        }

        public void GoTo<U>(bool animate = true) where U : T
        {
            var panel = Get<U>();
            GoTo(panel, animate);
        }

        protected virtual void AnimatePanelTransition(T panelOut, T panelIn)
        {
            panelOut.TransitionOut();
            panelIn.TransitionIn();
        }

        public void GoTo(int index, bool animate = true) => GoTo(_panels[index], animate);
        public void GoTo(T panel, bool animate = true)
        {
            var isSamePanel = CurrentPanel != null && panel == CurrentPanel;
            if (animate && !isSamePanel)
            {
                AnimatePanelTransition(CurrentPanel, panel);
            }

            panel.OnShow();

            if (isSamePanel) { return; }

            CurrentPanel?.OnHide();
            CurrentPanel = panel;

            OnPanelChanged?.Invoke(CurrentPanel);
        }

        public U Get<U>() where U : T => _panels.Find(match => match is U) as U;
    }
}