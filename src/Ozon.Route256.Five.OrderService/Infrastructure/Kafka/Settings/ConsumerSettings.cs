namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Settings;

public class ConsumerSettings
{
    public string? Topic { get; set; }
    public bool Enabled { get; set; } = true;
    public bool AutoCommit { get; set; } = false;
}