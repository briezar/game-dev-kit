using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class DropdownUtils
{
    public static void PopulateDropdownFromEnum<T>(Dropdown dropdown) where T : Enum
    {
        dropdown.ClearOptions();

        var enumOptions = EnumUtils.GetEnums<T>();
        var options = Array.ConvertAll(enumOptions, (input) => input.ToString());

        dropdown.AddOptions(new List<string>(options));
    }

    public static void SetDropdownValueByString(Dropdown dropdown, string stringValue, int defaultValue = 0)
    {
        if (dropdown == null) { return; }
        var index = dropdown.options.FindIndex(match => match.text == stringValue);
        if (index == -1) { index = defaultValue; }
        dropdown.value = index;
    }
}
