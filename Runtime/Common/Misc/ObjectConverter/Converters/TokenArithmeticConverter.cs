using System;
using GameDevKit.NewtonsoftJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameDevKit
{
    [Serializable]
    public class TokenArithmeticConverter : IObjectConverter
    {
        public enum Operator
        {
            Add,      // +
            Subtract, // -
            Multiply, // *
            Divide,   // /
            Modulus   // %
        }

        public string tokenPath1;
        public bool useConstantValue;
        public string tokenPath2;
        public float constantValue;

        public Operator operation;

        public object Convert(object data)
        {
            var jObj = JObject.FromObject(data, IObjectConverter.DefaultSerializer());
            var value1 = jObj.SelectToken(tokenPath1).Value<float>();
            var value2 = useConstantValue ? constantValue : jObj.SelectToken(tokenPath2).Value<float>();
            return operation switch
            {
                Operator.Add => value1 + value2,
                Operator.Subtract => value1 - value2,
                Operator.Multiply => value1 * value2,
                Operator.Divide => value1 / value2,
                Operator.Modulus => value1 % value2,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}