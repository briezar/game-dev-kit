using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public class ScriptableObjectConverter : IObjectConverter
    {
        public ObjectConverterSO converter;

        public object Convert(object data) => converter.Convert(data);
    }
}