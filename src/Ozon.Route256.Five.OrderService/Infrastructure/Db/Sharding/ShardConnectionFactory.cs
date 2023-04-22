using System.Data.Common;
using Microsoft.Extensions.Options;
using Npgsql;
using Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;

public class ShardConnectionFactory: IShardConnectionFactory
{
    private readonly IDbStore _dbStore;
    private readonly IShardingRule<long> _longShardingRule;
    private readonly IShardingRule<string> _stringShardingRule;
    private readonly int _bucketsCount;

    public ShardConnectionFactory(IDbStore dbStore,
        IShardingRule<long> longShardingRule,
        IShardingRule<string> stringShardingRule,
        IOptions<ShardFactoryOptions> options)
    {
        _dbStore            = dbStore;
        _longShardingRule   = longShardingRule;
        _stringShardingRule = stringShardingRule;
        _bucketsCount       = options.Value.BucketsCount;
    }

    public async Task<string> GetConnectionStringAsync(int bucketId)
    {
        var result = await _dbStore.GetEndpointByBucketAsync(bucketId);
        return $"Server={result.Host}:{result.Port};Database=orders_db;User Id=admin;Password=admin;";
    }

    public async Task<DbConnection> GetConnectionByKeyAsync(
        long shardKey,
        CancellationToken token)
    {
        var bucketId = _longShardingRule.GetBucketId(shardKey, _bucketsCount);
        return await GetConnectionByBucketAsync(bucketId, token);
    }

    public async Task<DbConnection> GetConnectionByKeyAsync(
        string shardKey,
        CancellationToken token)
    {
        var bucketId = _stringShardingRule.GetBucketId(shardKey, _bucketsCount);
        return await GetConnectionByBucketAsync(bucketId, token);
    }

    public async Task<DbConnection> GetConnectionByBucketAsync(
        int bucketId,
        CancellationToken token)
    {
        var connectionString = await GetConnectionStringAsync(bucketId);
        return new ShardNpgsqlConnection(new NpgsqlConnection(connectionString), bucketId);
    }
}