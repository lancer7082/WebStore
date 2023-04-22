using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

public class MetricsInterceptor : Interceptor
{
    private readonly IApiMetrics _apiMetrics;

    public MetricsInterceptor(IApiMetrics grpcMetrics)
    {
        _apiMetrics = grpcMetrics;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var result = await base.UnaryServerHandler(request, context, continuation);
            stopwatch.Stop();
            _apiMetrics.ResponseTime(stopwatch.ElapsedMilliseconds, context.Method, false);

            return result;
        }
        catch
        {
            _apiMetrics.ResponseTime(stopwatch.ElapsedMilliseconds, context.Method, true);

            throw;
        }
    }
}