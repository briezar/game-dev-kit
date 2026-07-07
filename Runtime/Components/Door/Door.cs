using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit
{
    public class Door : MonoBehaviour
    {
        public bool CanOpenWhileAlreadyOpened = true;
        public UnityEvent OnOpenDoor, OnCloseDoor;

        public bool IsOpen { get; private set; } = false;

        private readonly Dictionary<Collider, IDoorInteractable> _interactableLookup = new();

        public void Open()
        {
            IsOpen = true;
            OnOpenDoor?.Invoke();
        }

        public void Close()
        {
            IsOpen = false;
            OnCloseDoor?.Invoke();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.TryGetComponent(out IDoorInteractable interactable)) { return; }
            RemoveNullEntries();

            _interactableLookup[other] = interactable;

            if (IsOpen && !CanOpenWhileAlreadyOpened) { return; }

            if (interactable.AutoOpen)
            {
                Open();
            }
            interactable.OnOpenDoor?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_interactableLookup.Remove(other, out var interactable)) { return; }
            RemoveNullEntries();

            if (_interactableLookup.Count > 0) { return; }

            if (interactable.AutoClose)
            {
                Close();
            }
            interactable.OnCloseDoor?.Invoke();
        }

        private void RemoveNullEntries()
        {
            // OnTriggerExit will not be called if the object is destroyed, so we manually remove them
            _interactableLookup.RemoveWhere(c => c.Key == null || c.Value == null);
        }
    }
}