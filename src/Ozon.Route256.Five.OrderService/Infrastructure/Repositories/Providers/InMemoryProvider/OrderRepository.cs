using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.Domain.Dto.Filters;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Model;
using System.Runtime.CompilerServices;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;
using System.Linq;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.InMemoryProvider;
public class OrderRepository : IOrderRepository
{
    private readonly InMemoryProvider _inMemoryStorage;

    public OrderRepository(InMemoryProvider inMemoryStorage)
    {
        _inMemoryStorage = inMemoryStorage;
    }

    public Task<DbOrderDto?> FindAsync(long id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<DbOrderDto?>(token);

        return _inMemoryStorage.Orders.TryGetValue(id, out var order)
            ? Task.FromResult<DbOrderDto?>(order)
            : Task.FromResult<DbOrderDto?>(null);
    }

    public Task<DbOrderDto> GetAsync(long id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<DbOrderDto>(token);

        var order = _inMemoryStorage.Orders[id];
        if (order == null)
        {
            throw new NotFoundException($"Order {id} not found");
        }

        return Task.FromResult(order);
    }

    private IQueryable<DbOrderDto> GetQueryByFilter(OrdersFilter filter, PagingParams? paging = null, SortingParams? sorting = null)
    {
        var ordersQuery = _inMemoryStorage.Orders.Values.AsQueryable();

        // Фильтры:

        // по дате
        if (filter.DateBegin != null)
        {
            ordersQuery = ordersQuery.Where(x => x.Date >= filter.DateBegin);
        }

        if (filter.DateEnd != null)
        {
            ordersQuery = ordersQuery.Where(x => x.Date <= filter.DateEnd);
        }

        // по клиенту
        if (filter.CustomerId != null)
        {
            ordersQuery = ordersQuery.Where(x => x.CustomerId == filter.CustomerId);
        }

        // по типу заказа
        if (filter.OrderSource != null)
        {
            ordersQuery = ordersQuery.Where(x => x.OrderSource == (byte)filter.OrderSource);
        }

        // TODO:

        //// по списку регионов
        //if (filter.Regions != null && filter.Regions.Length > 0)
        //{
        //    ordersQuery = ordersQuery.Where(x => filter.Regions.Contains(x.Region));
        //}

        //// Сортировка
        //if (sorting != null)
        //{
        //    if (sorting.SortField.ToLower() == "region")
        //    {
        //        if (sorting.Ascending)
        //        {
        //            ordersQuery = ordersQuery.OrderBy(s => s.Region);
        //        }
        //        else
        //        {
        //            ordersQuery = ordersQuery.OrderByDescending(s => s.Region);
        //        }
        //    }
        //}

        // Пейджинг
        if (paging != null && paging.PageSize > 0)
        {
            ordersQuery = ordersQuery
                .Skip(paging.PageSize * paging.PageNumber)
                .Take(paging.PageSize);
        }

        return ordersQuery;
    }

    public Task<DbOrderDto[]> GetOrdersAsync(OrdersFilter filter, PagingParams? paging, SortingParams? sorting, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<DbOrderDto[]>(token);  //Array.Empty<OrderDto>();

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var ordersQuery = GetQueryByFilter(filter, paging, sorting);

        var orders = ordersQuery.ToArray();

        return Task.FromResult(orders);
    }

    public Task<RegionSummaryDto[]> GetSummaryAsync(OrdersFilter filter, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<RegionSummaryDto[]>(token);

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var ordersQuery = GetQueryByFilter(filter);

        var summary = ordersQuery
            .GroupBy(x => x.RegionId)
            .Select(x => new DbRegionSummaryDto(
                x.Key,
                x.Count(),
                x.Sum(y => y.Sum),
                x.Sum(y => y.Weight),
                x.Select(y => y.CustomerId).Distinct().Count()
            ))
            .ToList();

        // Получение списка регионов по ид.
        var regionsIds = summary.Select(x => x.RegionId);        
        var regions = _inMemoryStorage.Regions
            .Where(x => regionsIds.Contains((int)x.Key))
            .Select(x => x.Value);

        var result = summary
            .SelectMany(
                x => regions.Where(r => r.Id == x.RegionId).DefaultIfEmpty(),
                (x, r) => new RegionSummaryDto(
                    r?.Name ?? throw new NotFoundException($"Region {x.RegionId} not found"),
                    x.CountOrders,
                    x.TotalSum,
                    x.TotalWeight,
                    x.CountClients
                )
            )
            .ToArray();

        return Task.FromResult(result);
    }

    public async Task<bool> UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken token)
    {
        var order = await GetAsync(orderId, token);

        _inMemoryStorage.Orders[orderId] = order with { OrderState = (byte)newState };

        return true;
    }

    public Task<bool> IsExistsAsync(long orderId, CancellationToken token)
    {
        if (_inMemoryStorage.Orders.TryGetValue(orderId, out var order))
        {
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<long> InsertAsync(OrderDto orderDto, CancellationToken token)
    {
        var regionId = _inMemoryStorage.Regions
            .Where(x => x.Value.Name == orderDto.Region)
            .Select(x => x.Value.Id)
            .FirstOrDefault();

        var orderDb = orderDto.ToDbOrderDto(regionId);
        if (_inMemoryStorage.Orders.TryAdd(orderDto.Id, orderDb))
        {
            return Task.FromResult(orderDto.Id);
        }
        return Task.FromResult((long)0);
    }
}
