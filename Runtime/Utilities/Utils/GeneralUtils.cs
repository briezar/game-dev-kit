using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class GeneralUtils
{
    public static bool TrySet<T>(ref T field, T value) => TrySet(ref field, value, out _);
    public static bool TrySet<T>(ref T field, T value, out T oldValue)
    {
        oldValue = field;
        if (field.Equals(value)) { return false; }
        field = value;
        return true;
    }

    public static string ToBackingField(string propertyName) => $"<{propertyName}>k__BackingField";

    public static T CloneJsonUtility<T>(T obj)
    {
        var json = JsonUtility.ToJson(obj);
        var clone = JsonUtility.FromJson<T>(json);
        return clone;
    }

    public static T CloneNewtonSoftJson<T>(T obj)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        var clone = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
        return clone;
    }


    public static string GenerateRandomName(string prefix = "Player", string randomCharacters = "0123456789", int randomLength = 5)
    {
        var charArray = randomCharacters.ToCharArray();

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(prefix);

        for (int i = 0; i < randomLength; i++)
        {
            var randomChar = charArray.GetRandom();
            stringBuilder.Append(randomChar);
        }

        return stringBuilder.ToString();
    }

    public static SpriteState CreateIdenticalSpriteState(Sprite sprite)
    {
        var spriteState = new SpriteState
        {
            highlightedSprite = sprite,
            pressedSprite = sprite,
            selectedSprite = sprite,
            disabledSprite = sprite
        };
        return spriteState;
    }

    public static void SimulateClick(GameObject target)
    {
        var pointerEventData = new PointerEventData(EventSystem.current);
        ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.pointerClickHandler);
        ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.submitHandler);
    }

    public static bool ClickedInsideRect(RectTransform rectTransform, Camera cam = null)
    {
        if (Input.GetMouseButtonDown(0))
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, cam ?? Camera.main, out var localPoint);
            return rectTransform.rect.Contains(localPoint);
        }

        return false;
    }

    /// <summary> Selects a random item from a collection based on their weights. </summary>
    public static T GetWeightedRandom<T>(IEnumerable<T> items, Func<T, float> weightSelector)
    {
        var totalWeight = 0f;
        foreach (var item in items)
        {
            totalWeight += weightSelector(item);
        }

        var randomValue = UnityEngine.Random.Range(0, totalWeight);
        foreach (var item in items)
        {
            var weight = weightSelector(item);
            if (randomValue <= weight)
            {
                return item;
            }
            randomValue -= weight;
        }

        Debug.LogError("Failed to select a weighted random item.");
        return items.First();
    }

    /// <summary> Get a random index from an array of weights </summary>
    public static int GetIndexFromLootTable(params float[] weights)
    {
        if (weights.Length == 0)
        {
            Debug.LogError("Loot Table has 0 element!");
            return 0;
        }

        if (weights.Length == 1) { return 0; }

        int tableLength = weights.Length;

        float total = 0;

        for (int i = 0; i < tableLength; i++)
        {
            total += weights[i];
        }

        var randomChance = UnityEngine.Random.Range(0, total);

        for (var i = 0; i < tableLength; i++)
        {
            if (weights[i] <= 0) { continue; }
            if (randomChance <= weights[i])
            {
                return i;
            }
            else
            {
                randomChance -= weights[i];
            }
        }
        Debug.LogWarning("Exit Loot Table");
        return 0;
    }

    public static string GetMethodCallerInfo(int frameSkip = 0)
    {
        var stackTrace = new System.Diagnostics.StackTrace(true);
        var frame = stackTrace.GetFrame(2 + frameSkip); // 0 is this method, 1 is this method's caller, 2 is the upper method

        var methodName = frame.GetMethod().Name.Replace("b__0", "").Replace("<", "").Replace(">", "");
        var fileName = System.IO.Path.GetFileName(frame.GetFileName());
        var lineNumber = frame.GetFileLineNumber().ToString();
        var output = $"{methodName.Colorize(Color.yellow)} in {fileName.Colorize(Color.yellow)} at line {lineNumber.Colorize(Color.yellow)}";

        return output;
    }

    public static void ClampInputFieldValue(TMP_InputField inputField, float min, float max)
    {
        var value = inputField.text.ToFloat().Clamp(min, max);
        inputField.text = value.ToString();
    }

}