namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.PreOrders.Dto;

public record OrderItemDto(
    long Id,
    string Name,
    int Quantity,
    decimal Price,
    uint Weight);