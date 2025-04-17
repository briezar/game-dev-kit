using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class GraphicsExtension
{
    public static void SetOnClick(this Button button, Action action, bool removeInspectorEvents = false)
    {
        if (button == null)
        {
            Debug.LogError("Button is null. Cannot set onClick action.");
            return;
        }

        if (removeInspectorEvents)
        {
            button.onClick = new();
        }
        else
        {
            button.onClick.RemoveAllListeners();
        }
        button.onClick.AddListener(() => action?.Invoke());
    }

    public static void SetOnValueChanged(this Toggle toggle, Action<bool> action, bool removeInspectorEvents = true)
    {
        if (removeInspectorEvents)
        {
            toggle.onValueChanged = new();
        }
        else
        {
            toggle.onValueChanged.RemoveAllListeners();
        }
        toggle.onValueChanged.AddListener((isOn) => action?.Invoke(isOn));
    }

    public static T SetAlpha<T>(this T graphic, float newAlpha) where T : Graphic
    {
        var color = graphic.color;
        color.a = newAlpha;
        graphic.color = color;
        return graphic;
    }

    public static void SetAlpha(this SpriteRenderer renderer, float newAlpha)
    {
        var color = renderer.color;
        color.a = newAlpha;
        renderer.color = color;
    }

    public static Color GetTransparentColor(this SpriteRenderer renderer, float alpha = 0)
    {
        var color = renderer.color;
        color.a = alpha;
        return color;
    }
}
