using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ScriptableObjectExtensions
{
    public static void OverwriteDataWith<T>(this T target, T source) where T : ScriptableObject
    {
        if (source == null || target == null)
        {
            Debug.LogWarning($"Source ({source?.name}) or target ({target?.name}) is null.");
            return;
        }

        try
        {
            var sourceJson = JsonUtility.ToJson(source);
            JsonUtility.FromJsonOverwrite(sourceJson, target);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to overwrite target ({target.name}) with source ({source.name}).\n{ex}");
        }
    }
}