using System;
using Newtonsoft.Json;

namespace GameDevKit.DataPersistence
{
    public class NewtonsoftSerializationStrategy : SerializationStrategy
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public NewtonsoftSerializationStrategy(JsonSerializerSettings serializerSettings = null)
        {
            _serializerSettings = serializerSettings;
        }

        public override string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, _serializerSettings);
        }

        public override T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, _serializerSettings);
        }
    }
}