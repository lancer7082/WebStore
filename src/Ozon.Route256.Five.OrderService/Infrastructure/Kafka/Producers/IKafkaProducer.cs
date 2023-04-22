namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Producers;

public interface IKafkaProducer
{
    Task SendMessage(string key, string value, string topic, CancellationToken token);
}