using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

public class TraceInterceptor : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        using var activity = OrderActivitySourceConfig.OrderActivitySource
            .StartActivity(context.Method)!
            .AddTag("Now", DateTime.UtcNow);

        await context.WriteResponseHeadersAsync(
            new Metadata
            {
                {
                    "x-o3-trace-id", Activity.Current?.Id!
                }
            });
        return await base.UnaryServerHandler(request, context, continuation);
    }
}