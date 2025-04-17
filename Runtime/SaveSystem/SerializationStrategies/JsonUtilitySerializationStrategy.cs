using System;
using UnityEngine;

namespace GameDevKit.DataPersistence
{
    public class JsonUtilitySerializationStrategy : SerializationStrategy
    {
        public override string SerializeObject(object value)
        {
            return UnityEngine.JsonUtility.ToJson(value);
        }

        public override T DeserializeObject<T>(string value)
        {
            return UnityEngine.JsonUtility.FromJson<T>(value);
        }
    }
}