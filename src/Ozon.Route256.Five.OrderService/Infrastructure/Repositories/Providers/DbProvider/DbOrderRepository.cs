using Dapper;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Dto.Filters;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.Domain.Model;
using Ozon.Route256.Five.OrderService.Infrastructure.Db;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;
using System.Data;
using System.Text;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider;

public class DbOrderRepository : IOrderRepository
{
    private readonly string _getAllOrdersQuery = @"
SELECT
	o.id AS OrderId,
	o.quantity AS Quantity,
	o.sum AS Sum,
	o.weight AS Weight,
	o.order_source AS OrderSource,
	o.date AS Date,
	o.region_id AS RegionId,
	o.order_state AS OrderState,
    o.customer_id AS CustomerId,
    o.address_id AS AddressId	
FROM orders AS o
LEFT JOIN regions AS r ON r.Id = o.region_id
/**where**/";

    private readonly string _insertOrderQuery = @"
INSERT INTO orders
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
    @id, 
    @quantity, 
    @sum, 
    @weight, 
    @order_source, 
    @date, 
    @region_id, 
    @order_state, 
    @customer_id, 
    @address_id
)
RETURNING Id";

    private readonly string _updateOrderStateQuery = @"
UPDATE orders SET 
    order_state = @order_state
WHERE id = @id";

    private readonly string _getOrdersSummaryQuery = @"
SELECT
	o.region_id AS RegionId,
    COUNT(o.id) AS CountOrders,
    SUM(o.sum) AS TotalSum,
    SUM(o.weight) AS TotalWeight,
    COUNT(DISTINCT o.customer_id) AS CountClients
FROM orders AS o
LEFT JOIN regions AS r ON r.Id = o.region_id
/**where**/
GROUP BY o.region_id";

    private readonly IDbConnectionFactory _connectionFactory;
    private readonly ILogger<DbOrderRepository> _logger;
    private readonly IAddressRepository _addressRepository;

    public DbOrderRepository(IDbConnectionFactory connectionFactory, 
        ILogger<DbOrderRepository> logger,
        IAddressRepository addressRepository)
    {
        _connectionFactory = connectionFactory;
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
        await using var connection = await _connectionFactory.GetConnectionAsync();
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

    private SqlBuilder.Template BuildQueryByFilter(string queryTemplate,
        OrdersFilter filter, 
        PagingParams? paging = null, 
        SortingParams? sorting = null)
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

        // Сортировка
        if (sorting != null)
        {
            if (sorting.SortField.ToLower() == "region")
            {
                builder.OrderBy("r.name", sorting.Ascending ? "asc" : "desc");
            }
        }
 
        // Пейджинг
        if (paging != null && paging.PageSize > 0)
        {
            var pager = new Pager(paging.PageNumber, paging.PageSize);

            var queryStringBuilder = new StringBuilder(queryTemplate);
            queryStringBuilder
                .Append("\nLIMIT ")
                .Append(pager.PageSize)
                .Append("\nOFFSET ")
                .Append(pager.Offset);

            return builder.AddTemplate(queryStringBuilder.ToString());
        }

        // TODO: проверить, что в запросе есть хотя бы один фильтр или paging

        return builder.AddTemplate(queryTemplate);
    }


    public async Task<DbOrderDto[]> GetOrdersAsync(OrdersFilter filter, PagingParams? paging, SortingParams? sorting, CancellationToken token)
    {
        var queryTemplate = BuildQueryByFilter(_getAllOrdersQuery, filter, paging, sorting);

        await using var connection = await _connectionFactory.GetConnectionAsync();
        var orders = QueryOrdersAsync(connection, queryTemplate.RawSql, queryTemplate.Parameters).Result;

        return orders.ToArray();
    }

    public async Task<RegionSummaryDto[]> GetSummaryAsync(OrdersFilter filter, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Array.Empty<RegionSummaryDto>();

        if (filter == null)
        {
            throw new ArgumentNullException(nameof(filter));
        }

        await using var connection = await _connectionFactory.GetConnectionAsync();

        var queryTemplate = BuildQueryByFilter(_getOrdersSummaryQuery, filter);

        var regionsSummaryDb = await connection.QueryAsync<DbRegionSummaryDto>(queryTemplate.RawSql, queryTemplate.Parameters);

        // Получение списка регионов
        var regionsIds = regionsSummaryDb.Select(x => x.RegionId).ToArray();
        var regions = await _addressRepository.FindRegionsByIdsAsync(regionsIds, token);

        var result = regionsSummaryDb
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

        await using var connection = await _connectionFactory.GetConnectionAsync();

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
                var paramOrder = new DynamicParameters();

                paramOrder.Add("@id", order.Id);
                paramOrder.Add("@quantity", order.Quantity);
                paramOrder.Add("@sum", order.Sum);
                paramOrder.Add("@weight", order.Weight);
                paramOrder.Add("@order_source", order.OrderSource);
                paramOrder.Add("@date", order.Date);
                paramOrder.Add("@region_id", region.Id);
                paramOrder.Add("@order_state", order.OrderState);
                paramOrder.Add("@customer_id", order.Customer.Id);
                paramOrder.Add("@address_id", addressId);

                await connection.ExecuteAsync(_insertOrderQuery, paramOrder);

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
        await using var connection = await _connectionFactory.GetConnectionAsync();

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