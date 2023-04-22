using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.BackgroundConsumer;

public class KafkaJsonSerializer<TValue> : IDeserializer<TValue>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public TValue Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context) => 
        JsonSerializer.Deserialize<TValue>(data, JsonSerializerOptions)!;
}