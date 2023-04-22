namespace Ozon.Route256.Five.OrderService.Infrastructure.DateTimeProvider;

public interface IDateTimeProvider
{
    DateTimeOffset CurrentDateTimeOffsetUtc { get; }
}