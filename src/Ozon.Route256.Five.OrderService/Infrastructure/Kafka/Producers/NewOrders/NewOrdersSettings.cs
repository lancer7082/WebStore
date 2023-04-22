namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Producers.NewOrders;

public class NewOrdersSettings
{
    public const string Sections = "Kafka:NewOrderProducer";
    public string? Topic { get; set; }
}