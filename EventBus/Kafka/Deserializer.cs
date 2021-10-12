using Confluent.Kafka;
using EventBus.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventBus.Kafka
{
    public class Deserializer<T> : IDeserializer<T>
    {

        T IDeserializer<T>.Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var dataString = Encoding.UTF8.GetString(data.ToArray());
            return JsonConvert.DeserializeObject<T>(dataString);
        }
    }
}
