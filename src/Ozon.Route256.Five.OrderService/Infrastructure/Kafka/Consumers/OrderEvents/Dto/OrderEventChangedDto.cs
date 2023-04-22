namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.OrderEvents.Dto;

public record OrderEventChangedDto(long Id, OrderState NewState, DateTimeOffset UpdateDate);