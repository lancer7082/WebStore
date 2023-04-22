using Dapper;
using NpgsqlTypes;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;
using Ozon.Route256.Five.OrderService.Infrastructure.Db;
using Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;
using StackExchange.Redis;
using System.Data;
using System.Drawing;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.ShardedDbProvider;

public class ShardedDbAddressRepository : IAddressRepository
{
    private readonly string _getAllRegionsQuery = @"
SELECT
	r.id AS Id,
	r.name AS Name,
    r.warehouse_address_id AS WarehouseAddressId
FROM __bucket__.regions AS r
/**where**/";

    private readonly string _getAllAddressesQuery = @"
SELECT
	a.id AS Id,
	a.city AS City,
	a.street AS Street,
	a.building AS Building,
	a.apartment AS Apartment,
    coordinates[0] AS Latitude, 
    coordinates[1] AS Longitude,
	a.region_id AS RegionId	
FROM __bucket__.addresses AS a
/**where**/";

    private readonly string _insertAddressQuery = @"
INSERT INTO __bucket__.addresses 
(
    region_id,
    city, 
    street, 
    building, 
    apartment, 
    coordinates
)
VALUES 
(
    @RegionId,
    @City, 
    @Street, 
    @Building, 
    @Apartment, 
    @Coordinates
)
RETURNING Id";

    private readonly IShardConnectionFactory _connectionFactory;
    private readonly IShardingRule<long> _shardingRule;
    private readonly int _bucketsCount;

    public ShardedDbAddressRepository(IShardConnectionFactory connectionFactory,
        IShardingRule<long> shardingRule,
        IDbStore dbStore)
    {
        _connectionFactory = connectionFactory;
        _shardingRule = shardingRule;
        _bucketsCount = dbStore.BucketsCount;
    }

    #region Address

    private SqlBuilder.Template BuildAddressesQuery(long[] ids)
    {
        var builder = new SqlBuilder();

        // Фильтры:

        // по списку ид.
        if (ids != null && ids.Length > 0)
        {
            builder.Where($"a.id = ANY(@{nameof(ids)})", new { ids });
        }

        var queryTemplate = builder.AddTemplate(_getAllAddressesQuery);

        return queryTemplate;
    }

    private async Task<IEnumerable<DbAddressDto>> QueryAddressesAsync(IDbConnection connection, string query, object? param)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var result = await connection.QueryAsync<DbAddressDto>(query, param);
        return result;
    }

    private async Task<IEnumerable<DbAddressDto>> FindManyAddressesAsync(IDbConnection connection, long[] ids)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var queryAddressesTemplate = BuildAddressesQuery(ids);
        var result = await QueryAddressesAsync(connection, queryAddressesTemplate.RawSql, queryAddressesTemplate.Parameters);
        return result;
    }

    private async Task<DbAddressDto?> FindAddressAsync(IDbConnection connection, long id)
    {
        var addresses = await FindManyAddressesAsync(connection, new[] { id });
        var result = addresses.FirstOrDefault();
        return result;
    }

    public Task<DbAddressDto[]> GetAllAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    Task<DbAddressDto?> IAddressRepository.FindAsync(long id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<long> InsertAsync(IDbConnection connection, DbAddressDto address, CancellationToken token)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        // Добавление адреса
        var addressId = await connection.ExecuteScalarAsync(_insertAddressQuery, new { 
            address.RegionId,
            address.City,
            address.Street,
            address.Building,
            address.Apartment,
            Coordinates = new NpgsqlPoint(address.Latitude, address.Longitude)
        });

        return (long)addressId;
    }

    Task<DbAddressDto> IAddressRepository.GetAsync(long id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    Task<DbAddressDto[]> IAddressRepository.FindManyAsync(long[] ids, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Address (with sharding)

    public async Task<DbAddressDto> GetAsync(long id, long shardKey, CancellationToken token)
    {
        await using var connection = await _connectionFactory.GetConnectionByKeyAsync(shardKey, token);
        var result = await FindAddressAsync(connection, id);
        if (result == null)
        {
            throw new NotFoundException($"Address {id} not found");
        }
        return result;
    }

    public async Task<DbAddressDto[]> FindManyAsync(Dictionary<long, long[]> idsByShardKey, CancellationToken token)
    {
        // Словарь для получения ключей шардирования по бакету
        var bucketToShardKeysMap = idsByShardKey.Keys?
                 .Select(k => (BucketId: _shardingRule.GetBucketId(k, _bucketsCount), ShardKey: k))
                 .GroupBy(x => x.BucketId)
                 .ToDictionary(g => g.Key, g => g.Select(x => x.ShardKey).ToArray())
             ?? new Dictionary<int, long[]>();

        var result = new List<DbAddressDto>();
        foreach (var (bucketId, shardKeys) in bucketToShardKeysMap)
        {
            var ids = idsByShardKey
                .Where(x => shardKeys.Contains(x.Key))
                .SelectMany(x => x.Value)
                .ToArray();
            await using var connection = await _connectionFactory.GetConnectionByBucketAsync(bucketId, token);
            var addressesBucket = await FindManyAddressesAsync(connection, ids);

            result.AddRange(addressesBucket);
        }

        return result.ToArray();
    }

    #endregion

    #region Region

    private SqlBuilder.Template BuildRegionsQuery(int[]? ids = null, string[]? regions = null)
    {
        var builder = new SqlBuilder();

        // Фильтры:
        
        // по списку ид.
        if (ids != null && ids.Length > 0)
        {
            builder.Where($"r.id = ANY(@{nameof(ids)})", new { ids });
        }

        // по списку регионов
        if (regions != null && regions.Length > 0)
        {
            builder.Where($"r.name = ANY(@{nameof(regions)})", new { regions });
        }

        var queryTemplate = builder.AddTemplate(_getAllRegionsQuery);

        return queryTemplate;
    }

    private async Task<IEnumerable<DbRegionDto>> QueryRegionsAsync(IDbConnection connection, string query, object? param)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var result = await connection.QueryAsync<DbRegionDto>(query, param);
        return result;
    }

    private async Task<IEnumerable<DbRegionDto>> FindManyRegionsAsync(IDbConnection connection, string[] names)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var queryRegionsTemplate = BuildRegionsQuery(regions: names);
        var result = await QueryRegionsAsync(connection, queryRegionsTemplate.RawSql, queryRegionsTemplate.Parameters);
        return result;
    }

    private async Task<IEnumerable<DbRegionDto>> FindManyRegionsAsync(IDbConnection connection, int[] ids)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var queryRegionsTemplate = BuildRegionsQuery(ids: ids);
        var result = await QueryRegionsAsync(connection, queryRegionsTemplate.RawSql, queryRegionsTemplate.Parameters);
        return result;
    }

    private async Task<DbRegionDto?> FindRegionByIdAsync(IDbConnection connection, int id, CancellationToken token)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var queryRegionsTemplate = BuildRegionsQuery(ids: new[] { id });
        var regions = await QueryRegionsAsync(connection, queryRegionsTemplate.RawSql, queryRegionsTemplate.Parameters);
        var result = regions.FirstOrDefault();
        return result;
    }

    private async Task<DbRegionDto?> FindRegionAsync(IDbConnection connection, string name)
    {
        var regions = await FindManyRegionsAsync(connection, new[] { name });
        var result = regions.FirstOrDefault();
        return result;
    }

    public async Task<DbRegionDto> GetRegionByNameAsync(IDbConnection connection, string region, CancellationToken token)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var result = await FindRegionAsync(connection, region);
        if (result == null)
        {
            throw new NotFoundException($"Region {region} not found");
        }

        return result;
    }

    #endregion

    #region Region (with sharding)

    public async Task<string[]> GetAllRegionsAsync(CancellationToken token)
    {
        var bucketId = 0; // Копии таблицы регионов есть во всех бакетах, поэтому берем 1-й бакет.
        await using var connection = await _connectionFactory.GetConnectionByBucketAsync(bucketId, token);
        var regions = await QueryRegionsAsync(connection, _getAllRegionsQuery, null);
        var result = regions.Select(x => x.Name).ToArray();
        return result;
    }

    public async Task<DbRegionDto> GetRegionByNameAsync(string region, CancellationToken token)
    {
        var bucketId = 0; // Полная копия таблицы регионов есть в каждом бакете. Поэтому берем 1-ый бакет.
        await using var connection = await _connectionFactory.GetConnectionByBucketAsync(bucketId, token);
        var result = await GetRegionByNameAsync(connection, region, token);
        return result;
    }

    public async Task<DbRegionDto> GetRegionByIdAsync(int regionId, CancellationToken token)
    {
        var bucketId = 0; // Полная копия таблицы регионов есть в каждом бакете. Поэтому берем 1-ый бакет.
        await using var connection = await _connectionFactory.GetConnectionByBucketAsync(bucketId, token);
        var result = await FindRegionByIdAsync(connection, regionId, token);
        if (result == null)
        {
            throw new NotFoundException($"Region {regionId} not found");
        }
        return result;
    }

    public async Task<DbRegionDto[]> FindRegionsByNamesAsync(string[] names, CancellationToken token)
    {
        var bucketId = 0; // Полная копия таблицы регионов есть в каждом бакете. Поэтому берем 1-ый бакет.
        await using var connection = await _connectionFactory.GetConnectionByBucketAsync(bucketId, token);
        var result = await FindManyRegionsAsync(connection, names);
        return result.ToArray();
    }

    public async Task<DbRegionDto[]> FindRegionsByIdsAsync(int[] ids, CancellationToken token)
    {
        var bucketId = 0; // Полная копия таблицы регионов есть в каждом бакете. Поэтому берем 1-ый бакет.
        await using var connection = await _connectionFactory.GetConnectionByBucketAsync(bucketId, token);
        var result = await FindManyRegionsAsync(connection, ids);
        return result.ToArray();
    }

    #endregion
}
