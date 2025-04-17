using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GameDevKit.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class HyperlinkText : MonoBehaviour, IPointerClickHandler
    {
        [Serializable]
        public class LinkMap
        {
            public string id, url;
            public UnityEvent onClick = new();
            public bool openUrlOnClick = true;
        }

        [SerializeField] private LinkMap[] _links;

        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _text.raycastTarget = true;
        }

        public LinkMap GetLink(string id)
        {
            var link = _links.Find(link => link.id == id);
            return link;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            var linkIndex = TMP_TextUtilities.FindIntersectingLink(_text, eventData.position, Camera.main);
            if (linkIndex < 0 || linkIndex > _links.Length - 1) { return; }

            var linkInfo = _text.textInfo.linkInfo[linkIndex];

            var link = _links.Find(link => link.id == linkInfo.GetLinkID());
            if (link == null) { return; }

            if (link.openUrlOnClick && !link.url.IsNullOrEmpty())
            {
                Application.OpenURL(link.url);
            }

            link.onClick?.Invoke();
        }
    }
}