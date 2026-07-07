using UnityEngine;
using UnityEngine.Events;

namespace GameDevKit
{
    public interface IDoorInteractable
    {
        bool AutoOpen { get; }
        bool AutoClose { get; }
        UnityEvent OnOpenDoor { get; }
        UnityEvent OnCloseDoor { get; }
    }

    public class DoorInteractable : MonoBehaviour, IDoorInteractable
    {
        [field: SerializeField] public bool AutoOpen { get; set; }
        [field: SerializeField] public bool AutoClose { get; set; }
        [field: SerializeField] public UnityEvent OnOpenDoor { get; private set; }
        [field: SerializeField] public UnityEvent OnCloseDoor { get; private set; }
    }
}