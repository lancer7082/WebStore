namespace Ozon.Route256.Five.OrderService.Infrastructure.DateTimeProvider;

public class LocalDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset CurrentDateTimeOffsetUtc => DateTimeOffset.UtcNow;
}