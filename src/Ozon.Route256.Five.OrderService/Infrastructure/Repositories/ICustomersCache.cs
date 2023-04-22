using Ozon.Route256.Five.OrderService.Domain.Dto;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories;

public interface ICustomersCache
{
    Task<CustomerDto?> FindAsync(long id, CancellationToken token);

    Task<CustomerDto[]> FindManyAsync(long[] ids, CancellationToken token);

    Task<CustomerDto> GetAsync(long id, CancellationToken token);

    Task Insert(CustomerDto customer, CancellationToken token);

    Task Update(CustomerDto customer, CancellationToken token);

    Task<bool> IsExists(long customerId, CancellationToken token);
}
