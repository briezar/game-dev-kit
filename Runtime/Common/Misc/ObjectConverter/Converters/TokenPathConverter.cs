using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace GameDevKit
{
    [Serializable]
    public class TokenPathConverter : IObjectConverter
    {
        public string tokenPath;

        public object Convert(object data)
        {
            JObject jObj = data switch
            {
                UnityEngine.Object => JObject.Parse(JsonUtility.ToJson(data)),
                string str => JObject.Parse(str),
                _ => JObject.FromObject(data, IObjectConverter.DefaultSerializer()),
            };
            return jObj.SelectToken(tokenPath);
        }
    }
}