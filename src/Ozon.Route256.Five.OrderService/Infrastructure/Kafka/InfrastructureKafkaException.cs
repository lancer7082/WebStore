namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka;

public class InfrastructureKafkaException: Exception
{
    public InfrastructureKafkaException(string error): base(error)
    {
        
    }
}