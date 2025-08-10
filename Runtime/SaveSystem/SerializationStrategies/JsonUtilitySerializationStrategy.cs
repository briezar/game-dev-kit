using System;
using UnityEngine;

namespace GameDevKit.DataPersistence
{
    public class JsonUtilitySerializationStrategy : SerializationStrategy
    {
        public override string Serialize(object value) => JsonUtility.ToJson(value);
        public override T Deserialize<T>(string value) => JsonUtility.FromJson<T>(value);
    }
}