using Ozon.Route256.Five.OrderService.API.Infrastructure;
using Ozon.Route256.Five.OrderService.API.Grpc;
using Ozon.Route256.Five.OrderService.Infrastructure.Metrics;
using Prometheus;

public static class RegisterStartupMiddlewares
{
    public static WebApplication SetupMiddleware(this WebApplication app)
    {
        app.UseMiddleware<CustomExceptionHandlerMiddleware>();
        app.UseMiddleware<TraceMiddleware>();
        app.UseMiddleware<MetricsMiddleware>();
        app.UseSwagger()
            .UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", "Ordering.API V1");
            });

        app.UseRouting();
        app.UseEndpoints(x =>
        {
            x.MapControllers();
            GrpcEndpointRouteBuilderExtensions.MapGrpcService<OrdersService>(x);
            x.MapGrpcReflectionService();
            x.MapMetrics();
        });

        return app;
    }
}