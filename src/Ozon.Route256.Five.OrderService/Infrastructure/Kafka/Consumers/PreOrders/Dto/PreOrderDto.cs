namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.PreOrders.Dto;

public record PreOrderDto(
    long Id,
    OrderSource Source,
    CustomerDto Customer,
    OrderItemDto[] Goods
 );