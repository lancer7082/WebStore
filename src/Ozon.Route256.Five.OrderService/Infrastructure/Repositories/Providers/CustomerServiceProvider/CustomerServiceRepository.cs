using Ozon.Route256.Five.CustomerService.API.Proto;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.API.Grpc.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Ozon.Route256.Five.OrderService.API.Proto;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.CustomerServiceProvider;

/// <summary>
/// Репозиторий для получения данных из Grpc API сервиса CustomerService
/// </summary>
public class CustomerServiceRepository : ICustomerRepository
{
    private readonly ILogger<CustomerServiceRepository> _logger;
    private readonly Customers.CustomersClient _customerServiceClient;
    private readonly ICustomersCache _customersCache;

    public CustomerServiceRepository(Customers.CustomersClient customerServiceClient,
        ILogger<CustomerServiceRepository> logger,
        ICustomersCache customersCache
        )
    {
        _customerServiceClient = customerServiceClient;
        _logger = logger;
        _customersCache = customersCache;
    }

    public async Task<CustomerDto?> FindAsync(long id, CancellationToken token)
    {
        // Получение клиента из кеша
        var customer = await _customersCache.FindAsync(id, token);
        if (customer != null)
        {
            return customer;
        }

        // Получение клиента из сервиса CustomerService
        var customerProto = await _customerServiceClient.GetCustomerAsync(new GetCustomerByIdRequest { Id = (int)id }, cancellationToken: token);
        if (customerProto == null) 
        {
            return null;
        }

        var customerDto = customerProto.ToCustomerDto();

        // Сохранение данных клиента в кеше
        await _customersCache.Insert(customerDto, token);

        return customerDto;
    }

    public async Task<CustomerDto[]> GetAllAsync(CancellationToken token)
    {
        var customersProto = await _customerServiceClient.GetCustomersAsync(new Google.Protobuf.WellKnownTypes.Empty(), cancellationToken: token);
        
        var customersDto = MappingExtensions.ConvertCustomersToDto(customersProto.Customers.ToArray());

        return customersDto.ToArray();
    }

    public async Task<CustomerDto> GetAsync(long id, CancellationToken token)
    {
        // Получение клиента из кеша
        var customer = await _customersCache.FindAsync(id, token);
        if (customer != null)
        {
            return customer;
        }

        var customerProto = await _customerServiceClient.GetCustomerAsync(new GetCustomerByIdRequest { Id = (int)id }, cancellationToken: token);
        if (customerProto == null)
        {
            throw new NotFoundException($"Customer {id} not found");
        }

        var customerDto = customerProto.ToCustomerDto();
        
        // Сохранение данных клиента в кеше
        await _customersCache.Insert(customerDto, token);
        
        return customerDto;
    }

    public async Task<CustomerDto[]> FindManyAsync(long[] ids, CancellationToken token)
    {
        var customers = new List<CustomerDto>();

        // Получение клиентов из кеша
        var customersCache = await _customersCache.FindManyAsync(ids, token);

        var idsCache = customersCache.Select(x => x.Id);
        var idsNotFound = ids.Except(idsCache).ToArray();
        if (idsNotFound.Length > 0)
        {
            // Получение клиентов из сервиса CustomerService
            foreach(var id in idsNotFound)
            {
                var customerProto = await _customerServiceClient.GetCustomerAsync(new GetCustomerByIdRequest { Id = (int)id }, cancellationToken: token);
                if (customerProto != null)
                {
                    var customerDto = customerProto.ToCustomerDto();
                    customers.Add(customerDto);

                    // Сохранение данных клиента в кеше
                    await _customersCache.Insert(customerDto, token);
                }
            }
        }
        customers.AddRange(customersCache);
        return customers.ToArray();
    }
}
