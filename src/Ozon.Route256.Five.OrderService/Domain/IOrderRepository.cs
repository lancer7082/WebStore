using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Dto.Filters;
using Ozon.Route256.Five.OrderService.Domain.Model;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;

namespace Ozon.Route256.Five.OrderService.Domain;

public interface IOrderRepository
{
    Task<DbOrderDto> GetAsync(long id, CancellationToken token);

    Task<DbOrderDto?> FindAsync(long id, CancellationToken token);

    /// <summary>
    /// Получить список заказов
    /// </summary>
    /// <returns></returns>
    Task<DbOrderDto[]> GetOrdersAsync(OrdersFilter filter, PagingParams? paging = null, SortingParams? sorting = null, CancellationToken token = default);

    /// <summary>
    /// Получение суммарной информации по заказам
    /// </summary>
    /// <returns></returns>
    Task<RegionSummaryDto[]> GetSummaryAsync(OrdersFilter filter, CancellationToken token);

    /// <summary>
    /// Изменение статуса заказа
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="newState"></param>
    /// <returns></returns>
    Task<bool> UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken token);

    /// <summary>
    /// Добавление заказа в репозиторий
    /// </summary>
    /// <param name="orderDto"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<long> InsertAsync(OrderDto orderDto, CancellationToken token);
}
