using Ozon.Route256.Five.OrderService.Domain.Dto;

namespace Ozon.Route256.Five.OrderService.Domain;

public interface ICustomerRepository
{
    Task<CustomerDto[]> GetAllAsync(CancellationToken token);

    Task<CustomerDto?> FindAsync(long id, CancellationToken token);

    Task<CustomerDto> GetAsync(long id, CancellationToken token);

    Task<CustomerDto[]> FindManyAsync(long[] id, CancellationToken token);
}