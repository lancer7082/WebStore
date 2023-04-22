namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

public interface IApiMetrics
{
    void ResponseTime(long stopwatchElapsedMilliseconds, string contextMethod, bool isError);
}