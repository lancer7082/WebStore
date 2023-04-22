using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Metrics;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMetrics(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<IApiMetrics, ApiMetrics>();
        services.TryAddSingleton<IKafkaMetrics, KafkaMetrics>();

        var jaegerHost = "";
        var jaegerPort = 0;
        var connectionString = configuration.GetValue<string>("Jaeger:ConnectionString");
        if (connectionString != null && !string.IsNullOrEmpty(connectionString))
        {
            jaegerHost = connectionString.Split(':')[0];
            if (!int.TryParse(connectionString.Split(':')[1], out jaegerPort))
            {
                jaegerPort = 0;
            }
        }

        services.AddOpenTelemetry()
            .WithTracing(
                builder =>
                {
                    builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("OrderService"))
                        .AddSource(OrderActivitySourceConfig.SourceName)
                        .AddAspNetCoreInstrumentation()
                        .AddNpgsql();

                    if (!string.IsNullOrWhiteSpace(jaegerHost) && jaegerPort > 0)
                    {
                        builder
                            .AddJaegerExporter(
                                options =>
                                {
                                    options.AgentHost = jaegerHost;
                                    options.AgentPort = jaegerPort;
                                    options.Protocol = JaegerExportProtocol.UdpCompactThrift;
                                    options.ExportProcessorType = ExportProcessorType.Simple;
                                });
                    }
                });
        return services;
    }
}