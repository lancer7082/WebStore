using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using System;
using System.Diagnostics;
using System.Net;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

public class MetricsMiddleware
{
    private readonly RequestDelegate _next;

    private readonly IApiMetrics _apiMetrics;

    public MetricsMiddleware(RequestDelegate next,
        IApiMetrics apiMetrics)
    {
        _next = next;
        _apiMetrics = apiMetrics;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
            
            stopwatch.Stop();
            
            _apiMetrics.ResponseTime(stopwatch.ElapsedMilliseconds, context.Request.Path, false);

        }
        catch
        {
            _apiMetrics.ResponseTime(stopwatch.ElapsedMilliseconds, context.Request.Path, true);

            throw;
        }
    }
}
