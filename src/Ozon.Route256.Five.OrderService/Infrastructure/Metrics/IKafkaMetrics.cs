namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

public interface IKafkaMetrics
{
    void ResponseTime(long stopwatchElapsedMilliseconds, string handlerName, bool isError);
}