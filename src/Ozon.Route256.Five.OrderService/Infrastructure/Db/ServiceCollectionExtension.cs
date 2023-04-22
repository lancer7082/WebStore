using Dapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.Options;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.ShardedDbProvider;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, 
        IConfiguration configuration,
        Action<ShardFactoryOptions>? configureOptions = null)
    {
        services.AddScoped<IAddressRepository, ShardedDbAddressRepository>();
        services.AddScoped<IOrderRepository, ShardedDbOrderRepository>();

        SqlMapper.AddTypeHandler(new ArrayTypeHandler());
        SqlMapper.AddTypeHandler(new PointTypeHandler());

        services.AddScoped<IShardingRule<long>, RoundRobinLongShardingRule>();
        services.AddScoped<IShardingRule<string>, RoundRobinStringShardingRule>();
        services.AddScoped<IShardConnectionFactory, ShardConnectionFactory>();

        var options = configuration.GetSection(nameof(ShardFactoryOptions))
              .Get<ShardFactoryOptions>() ?? new ShardFactoryOptions();
        configureOptions?.Invoke(options);
        services.Configure<ShardFactoryOptions>(
            o =>
            {
                o.BucketsCount = options.BucketsCount;
            });

        return services;
    }
}