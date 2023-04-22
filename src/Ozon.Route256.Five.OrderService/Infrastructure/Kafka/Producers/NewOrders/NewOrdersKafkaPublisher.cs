using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Producers.NewOrders;

public class NewOrdersKafkaPublisher : INewOrdersKafkaPublisher
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IKafkaProducer _kafkaProducer;
    private readonly NewOrdersSettings _newOrdersSettings;

    public NewOrdersKafkaPublisher(IKafkaProducer kafkaProducer, IOptionsSnapshot<NewOrdersSettings> optionsSnapshot)
    {
        _kafkaProducer = kafkaProducer;
        _newOrdersSettings = optionsSnapshot.Value;
    }

    public Task PublishToKafka(NewOrderDto dto, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(_newOrdersSettings.Topic))
            throw new InfrastructureKafkaException($"Topic for {nameof(NewOrdersKafkaPublisher)} is empty");

        var value = JsonSerializer.Serialize(dto, JsonSerializerOptions);
        return _kafkaProducer.SendMessage(dto.OrderId.ToString(), value, _newOrdersSettings.Topic, token);
    }
}