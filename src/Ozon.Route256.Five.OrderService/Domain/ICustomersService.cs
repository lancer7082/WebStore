using Ozon.Route256.Five.OrderService.Domain.Dto;

namespace Ozon.Route256.Five.OrderService.Domain;

public interface ICustomersService
{
    /// <summary>
    /// Получить список клиентов
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<CustomerDto[]> GetAllAsync(CancellationToken token);

    /// <summary>
    /// Получить клиента по ид.
    /// </summary>
    /// <returns></returns>
    public Task<CustomerDto> GetAsync(long customerId, CancellationToken token);

    /// <summary>
    /// Найти клиента по ид.
    /// </summary>
    /// <returns></returns>
    public Task<CustomerDto?> FindAsync(long customerId, CancellationToken token);
}
