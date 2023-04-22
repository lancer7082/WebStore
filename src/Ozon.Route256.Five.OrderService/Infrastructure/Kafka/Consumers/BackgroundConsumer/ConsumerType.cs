namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.BackgroundConsumer;

public class ConsumerType
{
    public static readonly ConsumerType PreOrder = new("PreOrderConsumer");
    public static readonly ConsumerType OrderEvent = new("OrderEventConsumer");

    private ConsumerType(string name)
    {
        Name = name;
    }

    public string Name { get; }
}