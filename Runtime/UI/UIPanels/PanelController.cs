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
            CurrentPanel = _panels[firstPanelIndex];
        }

        public void GoTo<U>(bool animate = true) where U : T
        {
            var panel = Get<U>();
            GoTo(panel, animate);
        }

        protected virtual void AnimatePanelTransition(T panel)
        {
            var movementType = CurrentPanel.PanelIndex < panel.PanelIndex ? TransitionDirection.RightToLeft : TransitionDirection.LeftToRight;
            CurrentPanel.TransitionOut(movementType);
            panel.TransitionIn(movementType);
        }

        public void GoTo(int index, bool animate = true) => GoTo(_panels[index], animate);
        public void GoTo(T panel, bool animate = true)
        {
            var isSamePanel = CurrentPanel != null && panel == CurrentPanel;
            if (animate && !isSamePanel)
            {
                AnimatePanelTransition(panel);
            }

            panel.OnShow();

            if (isSamePanel) { return; }

            CurrentPanel?.OnHide();
            CurrentPanel = panel;

            OnPanelChanged?.Invoke(CurrentPanel);
        }

        public U Get<U>() where U : T
        {
            var panel = _panels.Find(match => match is U);
            return panel as U;
        }
    }
}