using System;

namespace GameDevKit.DataPersistence
{
    public abstract class SerializationStrategy
    {
        public abstract string SerializeObject(object value);
        public abstract T DeserializeObject<T>(string value);
    }
}