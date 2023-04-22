using Confluent.Kafka;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Validations;
using Newtonsoft.Json.Linq;
using Ozon.Route256.Five.LogisticsSimulator.API.Proto;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Dto.Filters;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.Domain.Model;
using Ozon.Route256.Five.OrderService.Infrastructure.Kafka.Producers.NewOrders;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net.NetworkInformation;

namespace Ozon.Route256.Five.OrderService.Domain;

public class OrdersService : IOrdersService
{
    private readonly ILogger<OrdersService> _logger;
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly LogisticsSimulatorService.LogisticsSimulatorServiceClient _logisticsSumulatorClient;
    private readonly INewOrdersKafkaPublisher _newOrdersKafkaPublisher;
    private readonly IServiceProvider _serviceProvider;

    public OrdersService(IOrderRepository orderRepository, 
        ICustomerRepository customerRepository,
        IAddressRepository addressRepository,
        LogisticsSimulatorService.LogisticsSimulatorServiceClient logisticsSumulatorClient,
        ILogger<OrdersService> logger,
        INewOrdersKafkaPublisher newOrdersKafkaPublisher,
        IServiceProvider serviceProvider)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _addressRepository = addressRepository;
        _logisticsSumulatorClient = logisticsSumulatorClient;
        _logger = logger;
        _newOrdersKafkaPublisher = newOrdersKafkaPublisher;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Отмена заказа
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="token"></param>
    public async Task CancelOrderAsync(long orderId, CancellationToken token)
    {
        var orderDb = await _orderRepository.FindAsync(orderId, token);
        if (orderDb == null)
        {
            throw new NotFoundException($"Order {orderId} not found");
        }

        var orderState = orderDb.OrderState.ToModelOrderState();
        
        if (orderState == OrderState.Cancelled)
        {
            throw new InvalidArgumentException("Order already cancelled");
        }

        if (orderState == OrderState.SentToCustomer)
        {
            throw new InvalidArgumentException("Can't cancel sent order");
        }

        // Отмена заказа в LogisticsSimulator через GRPC

        try
        {
            _logisticsSumulatorClient.OrderCancel(new LogisticsSimulator.API.Proto.Order { Id = orderId }, cancellationToken: token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while cancelling order {OrderId} in LogisticsSimulator", orderId);
            throw new InvalidOperationException($"Error while cancelling order {orderId} in LogisticsSimulator");
        }

        // Отмена заказа
        await _orderRepository.UpdateOrderStateAsync(orderId, OrderState.Cancelled, token);

        return;
    }

    private async Task<OrderDto?> FindOrderByIdAsync(long orderId, CancellationToken token)
    {
        var orderDb = await _orderRepository.FindAsync(orderId, token);
        if (orderDb == null)
        {
            return null;
        }

        // Регион в заказе
        var regionOrderDb = await _addressRepository.GetRegionByIdAsync(orderDb.RegionId, token);

        // Регион в адресе
        var addressDb = await _addressRepository.GetAsync(orderDb.AddressId, orderDb.Id, token);
        var regionAddressDb = await _addressRepository.GetRegionByIdAsync(addressDb.RegionId, token);
        var address = addressDb.ToAddressDto(regionAddressDb.Name);
        
        var customer = await _customerRepository.GetAsync(orderDb.CustomerId, token);

        var order = orderDb.ToOrderDto(customer, address, regionOrderDb.Name);
        return order;
    }

    public async Task<OrderDto?> FindOrderAsync(long orderId, CancellationToken token)
    {
        var result = await FindOrderByIdAsync(orderId, token);
        return result;
    }

    public async Task<OrderDto> GetOrderAsync(long orderId, CancellationToken token)
    {
        var result = await FindOrderByIdAsync(orderId, token);
        if (result == null)
        {
            throw new NotFoundException($"Order {orderId} not found");
        }
        return result;
    }

    public async Task<OrderState> GetOrderStateAsync(long orderId, CancellationToken token)
    {
        var orderDb = await _orderRepository.FindAsync(orderId, token);
        if (orderDb == null)
        {
            throw new NotFoundException($"Order {orderId} not found");
        }
        return orderDb.OrderState.ToModelOrderState();
    }

    /// <summary>
    /// Получить список заказов по клиенту
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="dateBegin"></param>
    /// <param name="paging"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="InvalidArgumentException"></exception>
    /// <exception cref="NotFoundException"></exception>
    public async Task<OrderDto[]> GetOrdersByCustomerAsync(long customerId, DateTime dateBegin, PagingParams? paging, CancellationToken token)
    {
        if (dateBegin == DateTime.MinValue)
        {
            throw new InvalidArgumentException("Incorrect date");
        }

        if (customerId == 0)
        {
            throw new ArgumentNullException(nameof(customerId));
        }

        var customer = await _customerRepository.FindAsync(customerId, token);

        if (customer == null)
        {
            throw new NotFoundException($"Customer {customerId} not found");
        }

        var filter = new OrdersFilter { DateBegin = dateBegin, CustomerId = customerId};
        var orders = await GetOrdersAsync(filter, paging, token: token);

        return orders;
    }

    /// <summary>
    /// Получить статистику по заказам по регионам
    /// </summary>
    /// <param name="regions"></param>
    /// <param name="dateBegin"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="InvalidArgumentException"></exception>
    public async Task<RegionSummaryDto[]> GetSummaryByRegionsAsync(string[] regions, DateTime dateBegin, CancellationToken token)
    {
        if (dateBegin == DateTime.MinValue)
        {
            throw new InvalidArgumentException("Incorrect date");
        }

        // Проверка наличия регионов
        if (regions != null && regions.Length > 0)
        {
            var regionsDto = await _addressRepository.FindRegionsByNamesAsync(regions, token);
            if (regionsDto == null || regionsDto.Length < regions.Length)
            {
                throw new InvalidArgumentException("Region(s) not found");
            }
        }

        var filter = new OrdersFilter { DateBegin = dateBegin, Regions = regions };
        var summary = await _orderRepository.GetSummaryAsync(filter, token: token);

        return summary;
    }

    /// <summary>
    /// Получить список заказов по списку регионов
    /// </summary>
    /// <param name="regions"></param>
    /// <param name="paging"></param>
    /// <param name="sorting"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidArgumentException"></exception>
    public async Task<OrderDto[]> GetOrdersByRegionsAsync(string[]? regions, OrderSource? orderSource, PagingParams? paging, SortingParams? sorting, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Array.Empty<OrderDto>();

        if (regions == null || regions.Length == 0)
        {
            throw new ArgumentNullException(nameof(regions));
        }

        if (regions.Length == 0)
        {
            throw new InvalidArgumentException(nameof(regions));
        }

        // Проверка наличия регионов
        var regionsDto = await _addressRepository.FindRegionsByNamesAsync(regions, token);
        if (regionsDto == null || regionsDto.Length < regions.Length)
        {
            throw new InvalidArgumentException("Region(s) not found");
        }
  
        var filter = new OrdersFilter { Regions = regions, OrderSource = orderSource };
        
        var orders = await GetOrdersAsync(filter, paging, sorting, token);
        
        return orders;
    }

    /// <summary>
    /// Получить список заказов
    /// </summary>
    /// <returns></returns>
    private async Task<OrderDto[]> GetOrdersAsync(OrdersFilter filter, PagingParams? paging = null, SortingParams? sorting = null,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
            return Array.Empty<OrderDto>();

        var ordersDb = await _orderRepository.GetOrdersAsync(filter, paging, sorting, token);

        // Получение адресов по списку ид.
        var idsByShardKey = ordersDb.ToDictionary(x => x.Id, x => new[] { x.AddressId });
        var addresses = await _addressRepository.FindManyAsync(idsByShardKey, token);

        // Получение регионов по списку ид.
        var regionsIdsOrders = ordersDb.Select(x => x.RegionId).Distinct();
        var regionsIdsAddresses = addresses.Select(x => x.RegionId).Distinct();
        var regionsIds = regionsIdsOrders.Union(regionsIdsAddresses).Distinct().ToArray();
        var regions = await _addressRepository.FindRegionsByIdsAsync(regionsIds, token);

        // Получение клиентов по списку ид.
        var customersIds = ordersDb.Select(x => x.CustomerId).Distinct().ToArray();
        var customers = await _customerRepository.FindManyAsync(customersIds, token);

        var orders = ordersDb
            // Адреса
            .SelectMany(
                x => addresses.Where(a => a.Id == x.AddressId).DefaultIfEmpty(),
                (x, a) => new { Order = x, Address = a }
            )
            // Клиенты
            .SelectMany(
                x => customers.Where(c => c.Id == x.Order.CustomerId).DefaultIfEmpty(),
                (x, c) => new { x.Order, x.Address, Customer = c }
            )
            // Регионы из заказов
            .SelectMany(
                x => regions.Where(r => r.Id == x.Order.RegionId).DefaultIfEmpty(),
                (x, r) => new { x.Order, x.Address, x.Customer, RegionOrder = r }
            )
            // Регионы из адресов
            .SelectMany(
                x => regions.Where(r => x.Address != null && r.Id == x.Address.RegionId).DefaultIfEmpty(),
                (x, r) => new { x.Order, x.Address, x.Customer, x.RegionOrder, RegionAddress = r }
            )
            .Select(x => {
                if (x.Customer == null)
                {
                    throw new NotFoundException($"Customer {x.Order.CustomerId} not found");
                }
                if (x.Address == null)
                {
                    throw new NotFoundException($"Address {x.Order.AddressId} not found");
                }
                if (x.RegionAddress == null)
                {
                    throw new NotFoundException($"Region {x.Address.RegionId} not found");
                }
                if (x.RegionOrder == null)
                {
                    throw new NotFoundException($"Region {x.Order.RegionId} not found");
                }
                return x.Order.ToOrderDto(x.Customer, x.Address.ToAddressDto(x.RegionAddress.Name), x.RegionOrder.Name);
            })
            ;

        return orders.ToArray();
    }

    /// <summary>
    /// Отправка заказа в топик new_orders
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private async Task<bool> PublishNewOrderAsync(long orderId, CancellationToken token)
    {
        await _newOrdersKafkaPublisher.PublishToKafka(new NewOrderDto(orderId), token);

        return true;
    }

    public async Task<bool> CheckAndPublishNewOrderAsync(long orderId, CancellationToken token)
    {
        var order = await _orderRepository.GetAsync(orderId, token);

        // валидация заказа
        var results = new List<ValidationResult>();
        var context = new ValidationContext(order, _serviceProvider, null);        
        if (Validator.TryValidateObject(order, context, results, true))
        {
            // Отправка сообщения в кафку о новом заказе
            var result = await PublishNewOrderAsync(orderId, token);

            return result;
        }

        return false;
    }

    public async Task<bool> SaveIfNotExists(OrderDto order, CancellationToken token)
    {
        if ((await _orderRepository.FindAsync(order.Id, token)) == null)
        {
            await _orderRepository.InsertAsync(order, token);
            return true;
        }
        return false;
    }

    public Task<bool> UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken token)
    {
        return _orderRepository.UpdateOrderStateAsync(orderId, newState, token);
    }
}
