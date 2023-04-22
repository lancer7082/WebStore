using Prometheus;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

internal class ApiMetrics : IApiMetrics
{
    private readonly Histogram _responseTimeHistogram =
       Prometheus.Metrics.CreateHistogram("ozon_api_response_time_ms", string.Empty, "methodName", "isError");
    
    public void ResponseTime(long duration, string methodName, bool isError)
    {
        _responseTimeHistogram
            .WithLabels(methodName, isError ? "1" : "0")
            .Observe(duration);
    }
}