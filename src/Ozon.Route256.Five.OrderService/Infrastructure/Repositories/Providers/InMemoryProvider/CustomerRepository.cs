using System.Collections.Concurrent;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.InMemoryProvider;

public class CustomerRepository : ICustomerRepository
{
    private readonly InMemoryProvider _inMemoryStorage;

    public CustomerRepository(InMemoryProvider inMemoryStorage)
    {
        _inMemoryStorage = inMemoryStorage;
    }

    public Task<CustomerDto[]> GetAllAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<CustomerDto[]>(token);

        return Task.FromResult(_inMemoryStorage.Customers.Values.ToArray());
    }

    public Task<CustomerDto?> FindAsync(long id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<CustomerDto?>(token);

        return _inMemoryStorage.Customers.TryGetValue(id, out var customer)
            ? Task.FromResult<CustomerDto?>(customer)
            : Task.FromResult<CustomerDto?>(null);
    }

    public Task<CustomerDto> GetAsync(long id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<CustomerDto>(token);

        return Task.FromResult(_inMemoryStorage.Customers[id]);
    }

    Task<CustomerDto[]> ICustomerRepository.FindManyAsync(long[] id, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}