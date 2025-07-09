using System;

namespace GameDevKit.DataPersistence
{
    public abstract class SerializationStrategy
    {
        public abstract string Serialize(object value);
        public abstract T Deserialize<T>(string value);
    }
}