namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.PreOrders.Dto;

public record AddressDto(
    string Region,
    string City,
    string Street,
    string Building,
    string Apartment,
    double Latitude,
    double Longitude);