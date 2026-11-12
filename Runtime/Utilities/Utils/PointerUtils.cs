using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class PointerUtils
{
    private static readonly List<RaycastResult> _raycastResults = new();
    private static readonly Dictionary<EventSystem, PointerEventData> _pointerEventDataLookup = new();

    public static bool IsPointerOverUI()
    {
        var eventSystem = EventSystem.current;
        var pointer = Pointer.current;
        if (eventSystem == null || pointer == null) { return false; }
        if (!_pointerEventDataLookup.TryGetValue(eventSystem, out var pointerEventData))
        {
            pointerEventData = new(eventSystem);
            _pointerEventDataLookup[eventSystem] = pointerEventData;
            _pointerEventDataLookup.RemoveWhere(kvp => kvp.Key == null);
        }
        pointerEventData.position = pointer.position.ReadValue();

        eventSystem.RaycastAll(pointerEventData, _raycastResults);
        return _raycastResults.Exists(r => r.module is GraphicRaycaster);
    }
}