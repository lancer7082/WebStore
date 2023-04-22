using Prometheus;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

internal class KafkaMetrics : IKafkaMetrics
{
    private readonly Histogram _responseTimeHistogram =
       Prometheus.Metrics.CreateHistogram("ozon_kafka_response_time_ms", string.Empty, "methodName", "isError");
    
    public void ResponseTime(long duration, string handlerName, bool isError)
    {
        _responseTimeHistogram
            .WithLabels(handlerName, isError ? "1" : "0")
            .Observe(duration);
    }
}