using Dapper;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto.Filters;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;
using Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;
using System.Data;
using System.Text;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;
using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.ShardedDbProvider;

public class ShardedDbOrderRepository : IOrderRepository
{
    #region Orders

    private readonly string _getAllOrdersQuery = @"
SELECT
	o.id AS Id,
	o.quantity AS Quantity,
	o.sum AS Sum,
	o.weight AS Weight,
	o.order_source AS OrderSource,
	o.date AS Date,
	o.region_id AS RegionId,
	o.order_state AS OrderState,
    o.customer_id AS CustomerId,
    o.address_id AS AddressId	
FROM __bucket__.orders AS o
LEFT JOIN __bucket__.regions AS r ON r.id = o.region_id
/**where**/";

    private readonly string _insertOrderQuery = @"
INSERT INTO __bucket__.orders
(
    id, 
    quantity, 
    sum, 
    weight, 
    order_source, 
    date, 
    region_id, 
    order_state, 
    customer_id, 
    address_id
)
VALUES
(
    @Id, 
    @Quantity, 
    @Sum, 
    @Weight, 
    @OrderSource, 
    @Date, 
    @RegionId, 
    @OrderState, 
    @CustomerId, 
    @AddressId
)
RETURNING Id";

    private readonly string _updateOrderStateQuery = @"
UPDATE __bucket__.orders SET 
    order_state = @order_state
WHERE id = @id";

    private readonly string _getOrdersSummaryQuery = @"
SELECT
	o.region_id AS RegionId,
    COUNT(o.id) AS CountOrders,
    SUM(o.sum) AS TotalSum,
    SUM(o.weight) AS TotalWeight,
    COUNT(DISTINCT o.customer_id) AS CountClients
FROM __bucket__.orders AS o
LEFT JOIN __bucket__.regions AS r ON r.id = o.region_id -- Нужно для фильтра по списку регионов
/**where**/
GROUP BY o.region_id";

    #endregion
       
//    private readonly string _indexQuery = @"
//select id from __bucket__.index_customer_last_name
//where last_name = @lastName";

    private readonly IShardConnectionFactory _connectionFactory;
    private readonly IShardingRule<long> _shardingRule;
    private readonly int _bucketsCount;
    private readonly ILogger<DbOrderRepository> _logger;
    private readonly IAddressRepository _addressRepository;

    public ShardedDbOrderRepository(IShardConnectionFactory connectionFactory,
        IShardingRule<long> shardingRule,
        IDbStore dbStore,
        ILogger<DbOrderRepository> logger,
        IAddressRepository addressRepository)
    {
        _connectionFactory = connectionFactory;
        _shardingRule = shardingRule;
        _bucketsCount = dbStore.BucketsCount;
        _logger = logger;
        _addressRepository = addressRepository;
    }

    public async Task<IEnumerable<DbOrderDto>> QueryOrdersAsync(IDbConnection connection, string query, object param)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var orders = await connection.QueryAsync<DbOrderDto>(query, param);
        return orders;
    }

    private async Task<DbOrderDto?> FindByIdAsync(long id, CancellationToken token)
    {
        var queryTemplate = BuildQueryById(id);
        await using var connection = await _connectionFactory.GetConnectionByKeyAsync(id, token);
        var orders = QueryOrdersAsync(connection, queryTemplate.RawSql, queryTemplate.Parameters).Result;
        var result = orders.FirstOrDefault();
        return result;
    }

    public async Task<DbOrderDto?> FindAsync(long id, CancellationToken token)
    {
        var result = await FindByIdAsync(id, token);
        return result;
    }

    public async Task<DbOrderDto> GetAsync(long id, CancellationToken token)
    {
        var result = await FindByIdAsync(id, token);
        if (result == null)
        {
            throw new NotFoundException($"Order {id} not found");
        }
        return result;
    }

    private SqlBuilder.Template BuildQueryById(long orderId)
    {
        var builder = new SqlBuilder();

        builder.Where($"o.id = @{nameof(orderId)}", new { orderId });

        return builder.AddTemplate(_getAllOrdersQuery);
    }

    private SqlBuilder.Template BuildQueryByFilter(string queryTemplate, OrdersFilter filter)
    {
        var builder = new SqlBuilder();

        // Фильтры:

        // по дате
        if (filter.DateBegin.HasValue)
        {
            builder.Where($"o.date >= @{nameof(filter.DateBegin)}", new { filter.DateBegin });
        }

        if (filter.DateEnd.HasValue)
        {
            builder.Where($"o.date <= @{nameof(filter.DateEnd)}", new { filter.DateBegin });
        }

        // по клиенту
        if (filter.CustomerId.HasValue)
        {
            builder.Where($"o.customer_id = @{nameof(filter.CustomerId)}", new { filter.CustomerId });
        }

        // по типу заказа
        if (filter.OrderSource.HasValue)
        {
            builder.Where($"o.order_source = @{nameof(filter.OrderSource)}", new { OrderSource = (byte)filter.OrderSource });
        }

        // по списку регионов
        if (filter.Regions != null && filter.Regions.Length > 0)
        {
            builder.Where($"r.name = ANY(@{nameof(filter.Regions)})", new { filter.Regions });
        }

        // TODO: проверить, что в запросе есть хотя бы один фильтр или paging

        return builder.AddTemplate(queryTemplate);
    }


    public async Task<DbOrderDto[]> GetOrdersAsync(OrdersFilter filter, PagingParams? paging, SortingParams? sorting, CancellationToken token)
    {
        var queryTemplate = BuildQueryByFilter(_getAllOrdersQuery, filter);

        var orders = new List<DbOrderDto>();

        // Запрос по всем бакетам        
        for (var i = 0; i < _bucketsCount; i++)
        {
            await using (var connection = await _connectionFactory.GetConnectionByBucketAsync(i, token))
            {
                var ordersBucket = QueryOrdersAsync(connection, queryTemplate.RawSql, queryTemplate.Parameters).Result;
                orders.AddRange(ordersBucket);
            }
        }

        var ordersQuery = orders.AsQueryable();

        // Сортировка
        if (sorting != null)
        {
            if (sorting.SortField.ToLower() == "region")
            {
                var regionsIds = orders.Select(x => x.RegionId).Distinct().ToArray();
                var regions = await _addressRepository.FindRegionsByIdsAsync(regionsIds, token);

                if (sorting.Ascending)
                {
                    ordersQuery = ordersQuery
                        .SelectMany(
                            x => regions.Where(r => r.Id == x.RegionId).DefaultIfEmpty(),
                            (x, r) => new {Order = x, Region = r}
                        )
                        .OrderBy(x => x.Region != null ? x.Region.Name : "")
                        .Select(x => x.Order)
                        ;
                }
                else
                {
                    ordersQuery = ordersQuery
                        .SelectMany(
                            x => regions.Where(r => r.Id == x.RegionId).DefaultIfEmpty(),
                            (x, r) => new { Order = x, Region = r }
                        )
                        .OrderByDescending(x => x.Region != null ? x.Region.Name : "")
                        .Select(x => x.Order)
                        ;
                }
            }
        }

        // Пейджинг
        if (paging != null && paging.PageSize > 0)
        {
            var pager = new Pager(paging.PageNumber, paging.PageSize);

            ordersQuery
                .Skip(pager.Offset)
                .Take(pager.PageSize)
                ;
        }

        return ordersQuery.ToArray();
    }

    public async Task<RegionSummaryDto[]> GetSummaryAsync(OrdersFilter filter, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Array.Empty<RegionSummaryDto>();

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        var regionsSummary = new List<DbRegionSummaryDto>();
        var queryTemplate = BuildQueryByFilter(_getOrdersSummaryQuery, filter);

        // Запрос по всем бакетам        
        for (var i = 0; i < _bucketsCount; i++)
        {
            await using (var connection = await _connectionFactory.GetConnectionByBucketAsync(i, token))
            {
                var regionsSummaryBucket = await connection.QueryAsync<DbRegionSummaryDto>(queryTemplate.RawSql, queryTemplate.Parameters);
                regionsSummary.AddRange(regionsSummaryBucket);
            }
        }

        // Получение списка регионов
        var regionsIds = regionsSummary.Select(x => x.RegionId).Distinct().ToArray();
        var regions = await _addressRepository.FindRegionsByIdsAsync(regionsIds, token);
       
        var result = regionsSummary
            .GroupBy(x => x.RegionId) // суммирование данных по бакетам
            .Select(x => new DbRegionSummaryDto(
                    x.Key, 
                    x.Sum(x => x.CountOrders),
                    x.Sum(x => x.TotalSum),
                    x.Sum(x => x.TotalSum),
                    x.Sum(x => x.CountClients)
                ))
            .SelectMany(
                x => regions.Where(r => r.Id == x.RegionId).DefaultIfEmpty(),
                (x, r) => new RegionSummaryDto(
                    r?.Name ?? throw new NotFoundException($"Region {x.RegionId} not found"),
                    x.CountOrders,
                    x.TotalSum,
                    x.TotalWeight,
                    x.CountClients
                )
            );

        return result.ToArray();
    }

    public async Task<long> InsertAsync(OrderDto order, CancellationToken token)
    {
        long orderId = 0;

        await using var connection = await _connectionFactory.GetConnectionByKeyAsync(order.Id, token);

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            using (var tran = connection.BeginTransaction())
            {
                // Поиск региона
                var region = await _addressRepository.GetRegionByNameAsync(connection, order.Region, token);

                // Добавление адреса
                var addressDb = order.Address.ToDbAddressDto(0, region.Id);
                var addressId = await _addressRepository.InsertAsync(connection, addressDb, token);
                if (addressId == 0)
                {
                    throw new InvalidArgumentException("Error while inserting address");
                }

                // Добавление заказа
                await connection.ExecuteAsync(_insertOrderQuery, new { 
                    order.Id,
                    order.Quantity,
                    order.Sum,
                    order.Weight,
                    order.OrderSource,
                    order.Date,
                    RegionId = region.Id,
                    order.OrderState,
                    CustomerId = order.Customer.Id,
                    AddressId = addressId
                });

                tran.Commit();

                orderId = order.Id;
            }
            connection.Close();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while inserting order");
            connection.Close();
            throw;
        }

        return orderId;
    }

    public async Task<bool> UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken token)
    {
        await using var connection = await _connectionFactory.GetConnectionByKeyAsync(orderId, token);

        try
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }

            var paramOrder = new DynamicParameters();

            paramOrder.Add("@id", orderId);
            paramOrder.Add("@order_state", (byte)newState);

            await connection.ExecuteAsync(_updateOrderStateQuery, paramOrder);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while updating order");
            connection.Close();
            throw;
        }

        return true;
    }

}