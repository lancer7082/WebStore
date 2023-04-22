using Microsoft.OpenApi.Models;
using Ozon.Route256.Five.OrderService.API.Grpc.Interceptors;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Infrastructure;
using Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;
using Ozon.Route256.Five.OrderService.Infrastructure.DateTimeProvider;
using Ozon.Route256.Five.OrderService.Infrastructure.Metrics;
using Ozon.Route256.Five.OrderService.Infrastructure.Redis;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.CustomerServiceProvider;
using System.Text.Json.Serialization;

public static class RegisterStartupServices
{
    public static WebApplicationBuilder RegisterServices(this WebApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddControllers()
             .AddJsonOptions(options =>
             {
                 options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
             });

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Ordering HTTP API",
                Version = "v1",
                Description = "The Ordering Service HTTP API"
            });
            options.CustomSchemaIds(type => type.ToString());
        });

        builder.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<ExceptionInterceptor>();
            options.Interceptors.Add<TraceInterceptor>();
            options.Interceptors.Add<MetricsInterceptor>();
        });

        builder.Services.AddGrpcReflection();

        builder.Services.AddDateTimeProvider();

        // Grpc клиент для CustomerService
        var customerServiceUrl = configuration.GetValue<string>("CustomerServiceUrl");
        if (string.IsNullOrEmpty(customerServiceUrl))
        {
            throw new ArgumentNullException(nameof(customerServiceUrl));
        }
        builder.Services.AddGrpcClient<Ozon.Route256.Five.CustomerService.API.Proto.Customers.CustomersClient>(
            options =>
            {
                options.Address = new Uri(customerServiceUrl);
            });

        // Grpc клиент для ServiceDiscovery
        var serviceDiscoveryUrl = configuration.GetValue<string>("ServiceDiscoveryUrl");
        if (string.IsNullOrEmpty(serviceDiscoveryUrl))
        {
            throw new ArgumentNullException(nameof(serviceDiscoveryUrl));
        }
        builder.Services.AddGrpcClient<Ozon.Route256.Five.ServiceDiscovery.API.Proto.SdService.SdServiceClient>(
            options =>
            {
                options.Address = new Uri(serviceDiscoveryUrl);
            });

        // Grpc клиент для LogisticsSimulator
        var logisticsSimulatorUrl = configuration.GetValue<string>("LogisticsSimulatorUrl");
        if (string.IsNullOrEmpty(logisticsSimulatorUrl))
        {
            throw new ArgumentNullException(nameof(logisticsSimulatorUrl));
        }
        builder.Services.AddGrpcClient<Ozon.Route256.Five.LogisticsSimulator.API.Proto.LogisticsSimulatorService.LogisticsSimulatorServiceClient>(
            options =>
            {
                options.Address = new Uri(logisticsSimulatorUrl);
            });

        builder.Services.AddSingleton<IDbStore, DbStore>();

        // Customers
        builder.Services.AddScoped<ICustomersCache, RedisCustomersCache>();
        builder.Services.AddScoped<ICustomerRepository, CustomerServiceRepository>();

        // Services
        builder.Services.AddScoped<IRegionsService, RegionsService>();
        builder.Services.AddScoped<ICustomersService, CustomersService>();
        builder.Services.AddScoped<IOrdersService, OrdersService>();

        builder.Services.AddHostedService<SdConsumerHostedService>();

        // Persistence, Redis, Kafka
        builder.Services.AddInfrastructure(configuration);

        return builder;
    }
}