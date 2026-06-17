using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using EditorAttributes;
using UnityEngine;

namespace GameDevKit.UI
{
    public enum TransitionDirection { None, LeftToRight, RightToLeft }

    public abstract class CommonPanel<T> : AdvancedBehaviour where T : CommonPanel<T>
    {
        private int _index = -1;

        public Action OnClickNext, OnClickBack;
        public T PrevPanel { get; protected set; }
        public T NextPanel { get; protected set; }
        public bool CanBack { get; set; } = true;

        public int PanelIndex => _index < 0 ? transform.GetSiblingIndex() : _index;

        /// <summary> -1 will default to SiblingIndex </summary>
        public void SetIndex(int index)
        {
            _index = index;
        }

        public virtual void OnShow()
        {
            gameObject.SetActive(true);
        }
        public virtual void OnHide()
        {
            gameObject.SetActive(false);
        }


        public void SetNavigation(T prevPanel, T nextPanel)
        {
            PrevPanel = prevPanel;
            NextPanel = nextPanel;
        }

        public virtual async UniTask TransitionIn() { }
        public virtual async UniTask TransitionOut() { }

#if UNITY_EDITOR
        [Button]
        private void Select()
        {
            foreach (var panel in transform.parent.GetComponentsInChildren<CommonPanel<T>>(true))
            {
                panel.gameObject.SetActive(panel == this);
            }
        }
#endif
    }
}