using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Consumers.PreOrders;

public static class MappingExtensions
{
    public static OrderSource ToOrderSource(this Dto.OrderSource orderSource) =>
        orderSource switch
        {
            Dto.OrderSource.WebSite => OrderSource.WebSite,
            Dto.OrderSource.Mobile => OrderSource.Mobile,
            Dto.OrderSource.Api => OrderSource.Api,
            _ => OrderSource.None
        };

    public static AddressDto ToAddressDto(this Dto.AddressDto address) =>
        new(
            address.City,
            address.Street,
            address.Building,
            address.Apartment,
            address.Latitude,
            address.Longitude,
            address.Region
        );

    public static OrderDto ConvertToOrderDto(this Dto.PreOrderDto preOrderDto, CustomerDto customer, DateTime date) =>
        new(preOrderDto.Id,
            preOrderDto.Goods.Sum(x => x.Quantity),
            preOrderDto.Goods.Sum(x => x.Quantity * (double)x.Price),
            preOrderDto.Goods.Sum(x => x.Weight),
            preOrderDto.Source.ToOrderSource(),
            date,
            preOrderDto.Customer.Address.Region,
            OrderState.Created,
            customer,
            preOrderDto.Customer.Address.ToAddressDto()
        );
}
