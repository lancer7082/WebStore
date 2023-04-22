using Google.Protobuf.WellKnownTypes;
using Ozon.Route256.Five.OrderService.API.Proto;
using System.Reflection.Metadata.Ecma335;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Grpc.Core;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;
using StackExchange.Redis;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider;

public static class MappingExtensions
{
    public static RegionDto ToRegionDto(this DbRegionDto region, AddressDto warehouseAddress) =>
        new(
            region.Name,
            warehouseAddress
    );

    #region Order

    public static Domain.Model.OrderState ToModelOrderState(this byte orderState) => (Domain.Model.OrderState)orderState;

    public static OrderDto ToOrderDto(this DbOrderDto order, CustomerDto customer, AddressDto address, string region) =>
        new(
            order.Id,
            order.Quantity,
            order.Sum,
            order.Weight,
            (Domain.Model.OrderSource)order.OrderSource,
            order.Date,
            region,
            (Domain.Model.OrderState)order.OrderState,
            customer,
            address
        );

    public static DbOrderDto ToDbOrderDto(this OrderDto order, int regionId) =>
        new(
            order.Id,
            order.Quantity,
            order.Sum,
            order.Weight,
            (byte)order.OrderSource,
            order.Date,
            regionId,
            (byte)order.OrderState,
            order.Customer.Id,
            0
        );

    #endregion

    #region Address

    public static AddressDto ToAddressDto(this DbAddressDto address, string region) =>
        new(
            address.City,
            address.Street,
            address.Building,
            address.Apartment,
            address.Latitude,
            address.Longitude,
            region            
        );

    public static DbAddressDto ToDbAddressDto(this AddressDto address, int addressId, int regionId) =>
        new(
            addressId,
            address.City,
            address.Street,
            address.Building,
            address.Apartment,
            address.Latitude,
            address.Longitude,
            regionId
            );

    #endregion
}