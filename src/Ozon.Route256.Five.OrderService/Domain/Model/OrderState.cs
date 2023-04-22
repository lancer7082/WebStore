namespace Ozon.Route256.Five.OrderService.Domain.Model;

public enum OrderState
{
    None,
    Created,
    SentToCustomer,
    Delivered,
    Lost,
    Cancelled
}