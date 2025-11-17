using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameDevKit;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpecializedExtensions
{
    public static class UnityObjectExtensions
    {
        public static void OverrideSortingLayer(this GameObject gameObject, bool allowClick = false, string layer = "", int sortingOrder = 0)
        {
            bool alreadyActive = gameObject.activeSelf;

            // GameObject must be active to change sorting
            gameObject.SetActive(true);

            var canvas = gameObject.GetOrAddComponent<Canvas>();

            if (gameObject.activeInHierarchy)
            {
                OverrideSorting();
            }
            else
            {
                gameObject.AddComponent<GameObjectLifeCycleDelegate>().Enabled.AddListener(OverrideSorting);
            }

            if (allowClick) { gameObject.GetOrAddComponent<GraphicRaycaster>(); }

            if (!alreadyActive) { gameObject.SetActive(false); }


            void OverrideSorting()
            {
                if (canvas == null) { return; }

                canvas.overrideSorting = true;
                canvas.sortingOrder = sortingOrder;

                if (!layer.IsNullOrEmpty())
                {
                    canvas.sortingLayerName = layer;
                }
            }
        }
        public static void OverrideSortingLayer(this Component component, bool allowClick = false, string layer = "", int sortingOrder = 0) => OverrideSortingLayer(component.gameObject, allowClick, layer, sortingOrder);

        public static void StopOverrideSorting(this GameObject gameObject, bool destroyCanvas = false)
        {
            if (gameObject.TryGetComponent<Canvas>(out var canvas))
            {
                if (!destroyCanvas)
                {
                    canvas.overrideSorting = false;
                    return;
                }
                if (gameObject.TryGetComponent<GraphicRaycaster>(out var raycaster)) { GameObject.Destroy(raycaster); }
                GameObject.Destroy(canvas);
                Canvas.ForceUpdateCanvases();
            }
        }
        public static void StopOverrideSorting(this Component component, bool destroyCanvas = false) => StopOverrideSorting(component.gameObject, destroyCanvas);

        public static void AddEventTrigger(this GameObject gameObject, Action action, EventTriggerType type = EventTriggerType.PointerDown)
        {
            var trigger = gameObject.GetOrAddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.callback.AddListener((eventData) => { action?.Invoke(); });
            entry.eventID = type;
            trigger.triggers.Add(entry);
        }
        public static void AddEventTrigger(this Component component, Action action, EventTriggerType type = EventTriggerType.PointerDown) => AddEventTrigger(component.gameObject, action, type);

        /// <summary> Scales with transform! </summary>
        public static Vector3 GetMiddleOfEdge(this RectTransform rectTransform, RectTransform.Edge edge, Vector2 localDiff = default)
        {
            var height = rectTransform.rect.height;
            var width = rectTransform.rect.width;
            var localPoint = localDiff;

            switch (edge)
            {
                case RectTransform.Edge.Left:
                    localPoint.x -= width / 2;
                    break;
                case RectTransform.Edge.Right:
                    localPoint.x += width / 2;
                    break;
                case RectTransform.Edge.Top:
                    localPoint.y += height / 2;
                    break;
                case RectTransform.Edge.Bottom:
                    localPoint.y -= height / 2;
                    break;
            }

            var worldPoint = rectTransform.TransformPoint(localPoint);
            return worldPoint;
        }
    }
}

public static class UnityObjectExtensions
{
    public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;

    public static bool IsDestroyedOrDisabled(this MonoBehaviour behaviour) => behaviour == null || !behaviour.isActiveAndEnabled;

    public static bool IsPrefab(this Component component) => IsPrefab(component.gameObject);
    public static bool IsPrefab(this GameObject obj)
    {
        return obj.scene == null || obj.scene.name == obj.name || obj.scene.name.IsNullOrEmpty();
    }

    public static bool IsUnityComponent(this Type type) => typeof(Component).IsAssignableFrom(type);

    public static GameObject GetParent(this GameObject gameObject) => gameObject.transform.parent.gameObject;
    public static GameObject GetParent(this Component component) => GetParent(component.gameObject);

    public static void DestroyGameObject(this GameObject gameObject)
    {
        if (gameObject == null) { return; }
        Object.Destroy(gameObject);
    }
    public static void DestroyGameObject(this Component component)
    {
        if (component == null) { return; }
        DestroyGameObject(component.gameObject);
    }

    public static void DestroyGameObjectImmediate(this GameObject gameObject)
    {
        if (gameObject == null) { return; }
        Object.DestroyImmediate(gameObject);
    }
    public static void DestroyGameObjectImmediate(this Component component)
    {
        if (component == null) { return; }
        DestroyGameObjectImmediate(component.gameObject);
    }

    public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component)
    {
        component = gameObject.GetComponentInParent<T>();
        return component != null;
    }
    public static bool TryGetComponentInParent<T>(this Component thisComponent, out T component) => TryGetComponentInParent(thisComponent.gameObject, out component);

    public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component)
    {
        component = gameObject.GetComponentInChildren<T>();
        return component != null;
    }
    public static bool TryGetComponentInChildren<T>(this Component thisComponent, out T component) => TryGetComponentInChildren(thisComponent.gameObject, out component);

    public static void GetComponentsInChildrenExceptSelf<T>(this Component component, bool includeInactive, List<T> result)
    {
        component.gameObject.GetComponentsInChildren(includeInactive, result);
        result.RemoveAll(c => c.Equals(component));
    }
    public static List<T> GetComponentsInChildrenExceptSelf<T>(this Component component, bool includeInactive = false)
    {
        var list = new List<T>();
        GetComponentsInChildrenExceptSelf(component, includeInactive, list);
        return list;
    }

    public static T AddComponent<T>(this Component component) where T : Component => component.gameObject.AddComponent<T>();

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (!gameObject.TryGetComponent<T>(out var result))
        {
            result = gameObject.AddComponent<T>();
        }
        return result;
    }
    public static T GetOrAddComponent<T>(this Component component) where T : Component => GetOrAddComponent<T>(component.gameObject);

    public static T LazyGet<T>(this Component component, ref T backingField, Func<T> selector)
    {
        if (backingField == null || backingField.Equals(null))
        {
            backingField = selector();
        }
        return backingField;
    }

    public static T LazyGetOrAdd<T>(this Component component, ref T backingField) where T : Component
    {
        return LazyGet(component, ref backingField, () => component.gameObject.GetOrAddComponent<T>());
    }

    public static RectTransform GetRectTransform(this Component component) => component.transform as RectTransform;
    public static RectTransform GetRectTransform(this GameObject gameObject) => gameObject.transform as RectTransform;

    public static void DestroyAllChildren(this Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            child.DestroyGameObject();
        }
    }

    public static IEnumerable<Transform> EnumerateChildren(this Transform parent, Func<Transform, bool> condition = null)
    {
        var count = parent.childCount;
        for (int i = 0; i < count; i++)
        {
            var child = parent.GetChild(i);
            if (condition == null || condition(child)) { yield return child; }
        }
    }

    public static IEnumerable<Transform> ReverseEnumerateChildren(this Transform parent, Func<Transform, bool> condition = null)
    {
        var count = parent.childCount;
        for (int i = count - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            if (condition == null || condition(child)) { yield return child; }
        }
    }

    public static int GetChildCount(this Transform parent, Func<Transform, bool> condition)
    {
        var count = 0;
        var childCount = parent.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = parent.GetChild(i);
            if (condition == null || condition(child)) { count++; }
        }
        return count;
    }

    public static void ClampInScreen(this RectTransform rectTransform, float offset = 0, Camera camera = null)
    {
        camera ??= Camera.main;

        var safeArea = Screen.safeArea;
        var minPos = camera.ScreenToWorldPoint(safeArea.min);
        var maxPos = camera.ScreenToWorldPoint(safeArea.max);

        var corners = ArrayPool<Vector3>.Shared.Rent(4);
        rectTransform.GetWorldCorners(corners);

        var minCorner = corners[0];
        var maxCorner = corners[2];

        ArrayPool<Vector3>.Shared.Return(corners);

        float xOffset = 0;
        float yOffset = 0;

        if (minCorner.x < minPos.x)
        {
            xOffset = minPos.x - minCorner.x;
        }
        else if (maxCorner.x > maxPos.x)
        {
            xOffset = maxPos.x - maxCorner.x;
        }

        xOffset += offset * Mathf.Sign(xOffset);

        if (minCorner.y < minPos.y)
        {
            yOffset = minPos.y - minCorner.y;
        }
        else if (maxCorner.y > maxPos.y)
        {
            yOffset = maxPos.y - maxCorner.y;
        }

        yOffset += offset * Mathf.Sign(yOffset);

        rectTransform.position += new Vector3(xOffset, yOffset);
    }

    public static void Stop(this Coroutine coroutine, MonoBehaviour runner)
    {
        if (coroutine == null) { return; }
        runner.StopCoroutine(coroutine);
    }

    public static Coroutine WaitAndDo(this MonoBehaviour monoBehaviour, float delay, Action action)
    {
        return monoBehaviour.StartCoroutine(WaitAndDoRoutine());
        IEnumerator WaitAndDoRoutine()
        {
            yield return YieldCollection.WaitForSeconds(delay);
            action?.Invoke();
        }
    }

    public static Coroutine WaitForEndOfFrameAndDo(this MonoBehaviour monoBehaviour, Action action)
    {
        return monoBehaviour.StartCoroutine(WaitAndDoRoutine());
        IEnumerator WaitAndDoRoutine()
        {
            yield return YieldCollection.WaitForEndOfFrame();
            action?.Invoke();
        }
    }

    public static void RebuildImmediate(this LayoutGroup layout)
    {
        layout.CalculateLayoutInputVertical();
        layout.CalculateLayoutInputHorizontal();
        layout.SetLayoutVertical();
        layout.SetLayoutHorizontal();
    }

    /// <summary> Sets the parent of a transform without changing its world position </summary>
    public static void SetParentConvertPosition(this Transform child, Transform parent)
    {
        var position = child.position;
        child.SetParent(parent, worldPositionStays: false);
        child.position = position;
    }
}
