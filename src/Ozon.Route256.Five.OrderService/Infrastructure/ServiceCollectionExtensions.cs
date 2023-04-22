using Ozon.Route256.Five.OrderService.Infrastructure.Db;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka;
using Ozon.Route256.Five.OrderService.Infrastructure.Metrics;
using Ozon.Route256.Five.OrderService.Infrastructure.Redis;

namespace Ozon.Route256.Five.OrderService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddRedis(configuration)
            .AddKafka(configuration)
            .AddMetrics(configuration)
            ;        
            
        return services;
    }
    
}