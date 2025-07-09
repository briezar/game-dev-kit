using System;
using UnityEngine;

namespace GameDevKit.DataPersistence
{
    public class JsonUtilitySerializationStrategy : SerializationStrategy
    {
        public override string Serialize(object value)
        {
            return UnityEngine.JsonUtility.ToJson(value);
        }

        public override T Deserialize<T>(string value)
        {
            return UnityEngine.JsonUtility.FromJson<T>(value);
        }
    }
}