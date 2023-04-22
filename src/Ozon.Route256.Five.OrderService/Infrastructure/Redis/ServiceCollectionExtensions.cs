namespace Ozon.Route256.Five.OrderService.Infrastructure.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>("Redis:ConnectionString");
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
        return services;
    }
}