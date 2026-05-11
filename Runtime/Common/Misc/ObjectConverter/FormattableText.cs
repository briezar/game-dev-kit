using System;
using System.Linq;
using GameDevKit.Attributes;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public class FormattableText
    {
        [TextArea]
        public string BaseText;

        [SerializeReference, SubclassPicker]
        public IObjectConverter[] StringFormatEntries;

        public string GetFormattedText(object data)
        {
            if (StringFormatEntries.IsNullOrEmpty()) { return BaseText; }
            var args = StringFormatEntries.Select(converter =>
            {
                try
                {
                    return converter.Convert(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error converting data with converter index {StringFormatEntries.IndexOf(converter)}\n{ex}");
                    return BaseText;
                }
            }).ToArray();

            return string.Format(BaseText, args);
        }
    }
}