namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.OrderEvents.Dto;

public enum OrderState
{
    Created,
    SentToCustomer,
    Delivered,
    Lost,
    Cancelled
}