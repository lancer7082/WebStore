using System.Diagnostics;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

public static class OrderActivitySourceConfig
{
    public const string SourceName = "OrderService";
    
    public static ActivitySource OrderActivitySource = new(SourceName);
}