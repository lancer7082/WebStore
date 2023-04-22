using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.OrderEvents;
public static class MappingExtensions
{
    public static OrderState ToOrderState(this Dto.OrderState orderState) =>
        orderState switch
        {
            Dto.OrderState.Created => OrderState.Created,
            Dto.OrderState.SentToCustomer => OrderState.SentToCustomer,
            Dto.OrderState.Delivered => OrderState.Delivered,
            Dto.OrderState.Lost => OrderState.Lost,
            Dto.OrderState.Cancelled => OrderState.Cancelled,
            _ => OrderState.None
        };
}
