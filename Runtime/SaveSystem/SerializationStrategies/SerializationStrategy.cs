using System;

namespace GameDevKit.DataPersistence
{
    public abstract class SerializationStrategy
    {
        public abstract string Serialize(object value);

        /// <summary> Returns default if value is null or empty </summary>
        public abstract T Deserialize<T>(string value);
    }
}