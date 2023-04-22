using Ozon.Route256.Five.OrderService.Infrastructure.Db;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("ROUTE256_");
var host = builder
    .RegisterServices(builder.Configuration)
    .Build();

var doMigrate = bool.TryParse(Environment.GetEnvironmentVariable("MIGRATE"), out var migrate) && migrate;

if (doMigrate)
{
    await host.MigrateAsync();
}
else
{
    host
        .SetupMiddleware()
        .Run();
}
