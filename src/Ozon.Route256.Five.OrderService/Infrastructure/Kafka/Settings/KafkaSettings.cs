namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Settings;

public class KafkaSettings
{
    public string? BootstrapServers { get; set; }
    public string? GroupId { get; set; }
    public int TimeoutForRetryInSeconds { get; set; } = 2;
}