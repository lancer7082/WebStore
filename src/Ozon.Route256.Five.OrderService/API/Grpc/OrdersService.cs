using Faker;
using Grpc.Core;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.API.Proto;
using Ozon.Route256.Five.OrderService.API.Grpc.Extensions;

namespace Ozon.Route256.Five.OrderService.API.Grpc;

public class OrdersService : Proto.OrderService.OrderServiceBase
{
    private readonly IOrdersService _ordersService;
    private readonly ICustomersService _customerService;

    public OrdersService(IOrdersService ordersService, ICustomersService customersService)
    {
        _ordersService = ordersService;
        _customerService = customersService;
    }

    public async override Task<GetOrderResponse> GetOrder(GetOrderRequest request, ServerCallContext context)
    {
        var order = await _ordersService.GetOrderAsync(request.OrderId, context.CancellationToken);

        if (order.Customer.Id == 0)
        {
            throw new InvalidArgumentException("Incorrect customer id");
        }

        var customer = await _customerService.FindAsync(order.Customer.Id, context.CancellationToken);
        if (customer == null)
        {
            throw new NotFoundException($"Customer {order.Customer.Id} not found");
        }

        var customerProto = customer.ToProtoCustomer();

        var orderAddressProto = order.Address.ToProtoAddress();

        var orderProto = order.ToProtoOrder(customerProto, orderAddressProto);

        return new GetOrderResponse { Order = orderProto };
    }
}

