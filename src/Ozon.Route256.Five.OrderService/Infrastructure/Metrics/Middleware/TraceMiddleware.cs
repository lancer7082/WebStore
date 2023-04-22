using System.Diagnostics;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

public class TraceMiddleware
{
    private readonly RequestDelegate _next;

    public TraceMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        using var activity = OrderActivitySourceConfig.OrderActivitySource
            .StartActivity(context.Request.Path)!
            .AddTag("Now", DateTime.UtcNow);

        context.Response.Headers.Append("x-o3-trace-id", Activity.Current?.Id!);

        await _next(context);
    }
}
