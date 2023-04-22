namespace Ozon.Route256.Five.OrderService.Infrastructure.DateTimeProvider;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddDateTimeProvider(this IServiceCollection collection)
    {
        collection.AddSingleton<IDateTimeProvider, LocalDateTimeProvider>();
        return collection;
    }
}