using Google.Protobuf.WellKnownTypes;
using Ozon.Route256.Five.OrderService.API.Proto;
using System.Reflection.Metadata.Ecma335;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Grpc.Core;

namespace Ozon.Route256.Five.OrderService.API.Grpc.Extensions;

public static class MappingExtensions
{
    public static Timestamp ToProtoTimestamp(this DateTime dateTime) =>
        Timestamp.FromDateTime(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));

    public static Proto.OrderSource ToProtoOrderSource(this Domain.Model.OrderSource orderSourceDto) =>
        orderSourceDto switch
        {
            Domain.Model.OrderSource.WebSite => Proto.OrderSource.WebSite,
            Domain.Model.OrderSource.Mobile => Proto.OrderSource.Mobile,
            Domain.Model.OrderSource.Api => Proto.OrderSource.Api,
            _ => Proto.OrderSource.None
        };

    public static Proto.OrderState ToProtoOrderState(this Domain.Model.OrderState orderStateDto) =>
        orderStateDto switch
        {
            Domain.Model.OrderState.Created => Proto.OrderState.Created,
            Domain.Model.OrderState.SentToCustomer => Proto.OrderState.SentToCustomer,
            Domain.Model.OrderState.Delivered => Proto.OrderState.Delivered,
            Domain.Model.OrderState.Lost => Proto.OrderState.Lost,
            Domain.Model.OrderState.Cancelled => Proto.OrderState.Cancelled,
            _ => Proto.OrderState.None
        };

    public static Address ToProtoAddress(this AddressDto addressDto) =>
        new()
        {
            Region = addressDto.Region,
            City = addressDto.City,
            Street = addressDto.Street,
            Building = addressDto.Building,
            Apartment = addressDto.Apartment,
            Longitude = addressDto.Longitude,
            Latitude = addressDto.Latitude
        };

    public static Customer ToProtoCustomer(this CustomerDto customerDto) =>
        new()
        {
            Id = customerDto.Id,
            FirstName = customerDto.FirstName,
            LastName = customerDto.LastName,
            MobileNumber = customerDto.MobileNumber,
            Email = customerDto.Email
        };

    public static Order ToProtoOrder(this OrderDto orderDto, Customer customer, Address address) =>
        new()
        {
            OrderId = orderDto.Id,
            Quantity = orderDto.Quantity,
            Date = orderDto.Date.ToProtoTimestamp(),
            Weight = orderDto.Weight,
            Sum = orderDto.Sum,
            OrderSource = orderDto.OrderSource.ToProtoOrderSource(),
            OrderState = orderDto.OrderState.ToProtoOrderState(),
            Region = orderDto.Region,
            Customer = customer,
            Address = address
        };

    #region API CustomerService

    public static CustomerDto ToCustomerDto(this CustomerService.API.Proto.Customer customerProto) =>
        new(customerProto.Id,
            customerProto.FirstName,
            customerProto.LastName,
            customerProto.MobileNumber,
            customerProto.Email
        );

    /// <summary>
    /// Конвертация списка клиентов из API сервиса CustomerService в DTO
    /// </summary>
    /// <param name="customers"></param>
    /// <returns></returns>
    public static IEnumerable<CustomerDto> ConvertCustomersToDto(IEnumerable<CustomerService.API.Proto.Customer> customers)
    {
        foreach (var customer in customers)
        {
            yield return customer.ToCustomerDto();
        }
    }

    #endregion
}