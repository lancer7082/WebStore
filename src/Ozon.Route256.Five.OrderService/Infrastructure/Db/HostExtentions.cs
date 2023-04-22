using FluentMigrator.Runner;
using Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;
using Ozon.Route256.Five.ServiceDiscovery.API.Proto;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db;

public static class HostExtension
{
    public static async Task MigrateAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var sdClient = scope.ServiceProvider.GetRequiredService<SdService.SdServiceClient>();
        var migratorRunner = new ShardMigratorRunner(sdClient);
        await migratorRunner.MigrateAsync();
    }
}