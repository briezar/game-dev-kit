using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ScrollExtension
{
    /// <summary> Get the normalized position where the element would be focused in the viewport </summary>
    /// <param name="focusValue">Ranges from 0 to 1 with 0 being leftmost, 1 being rightmost</param>
    public static float GetHorizontalNormalizedPos(this ScrollRect scrollRect, int elementIndex, float focusValue = 0.5f)
    {
        var contentWidth = scrollRect.content.rect.width;
        var viewportWidth = scrollRect.viewport.rect.width;

        // Nothing to scroll
        if (contentWidth <= viewportWidth) { return 0; }

        var activeChildCount = scrollRect.content.GetChildCount(child => child.gameObject.activeInHierarchy);
        var spacingPerChild = contentWidth / activeChildCount;

        var visibleChildInViewport = viewportWidth / spacingPerChild;

        // Found this value through testing
        var scrollCoefficient = (contentWidth / viewportWidth) - 1;

        var scrollDistancePerChild = (1 / visibleChildInViewport) / scrollCoefficient;

        // The element is put at the leftmost, so we scroll to the left (-1) according to the focusValue (visibleChild - 1 because it's the element at elementIndex) 
        var extraDistance = -1 * (visibleChildInViewport - 1) * scrollDistancePerChild * focusValue;

        var finalPos = scrollDistancePerChild * elementIndex + extraDistance;

        return finalPos.Clamp(0, 1);
    }

    /// <inheritdoc cref="GetHorizontalNormalizedPos(ScrollRect, int, float)"/>
    public static float GetHorizontalNormalizedPos(this ScrollRect scrollRect, Transform child, float focusValue = 0.5f)
    {
        if (!child.IsChildOf(scrollRect.content))
        {
            Debug.LogWarning($"{child.name} is not child of {scrollRect.name}");
            return 0;
        }

        return GetHorizontalNormalizedPos(scrollRect, child.GetSiblingIndex(), focusValue);
    }

    public static float GetNormalizedPos(this ScrollRect scrollRect, int elementIndex, RectTransform.Axis axis, float focusValue = 0.5f)
    {
        var contentSize = axis == RectTransform.Axis.Horizontal ? scrollRect.content.rect.width : scrollRect.content.rect.height;
        var viewportSize = axis == RectTransform.Axis.Horizontal ? scrollRect.viewport.rect.width : scrollRect.viewport.rect.width;

        // Nothing to scroll
        if (contentSize <= viewportSize) { return 0; }

        var activeChildCount = scrollRect.content.GetChildCount(child => child.gameObject.activeInHierarchy);
        var spacingPerChild = contentSize / activeChildCount;

        var visibleChildInViewport = viewportSize / spacingPerChild;

        // Found this value through testing
        var scrollCoefficient = (contentSize / viewportSize) - 1;

        var scrollDistancePerChild = (1 / visibleChildInViewport) / scrollCoefficient;

        // The element is put at the leftmost, so we scroll to the left (-1) according to the focusValue (visibleChild - 1 because it's the element at elementIndex) 
        var extraDistance = -1 * (visibleChildInViewport - 1) * scrollDistancePerChild * focusValue;

        var finalPos = scrollDistancePerChild * elementIndex + extraDistance;

        if (axis == RectTransform.Axis.Vertical) { finalPos = 1 - finalPos; }

        return finalPos.Clamp(0, 1);
    }
}
