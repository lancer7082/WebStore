using System.Data.Common;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;

public interface IShardConnectionFactory
{   
    Task<DbConnection> GetConnectionByKeyAsync(
        long shardKey,
        CancellationToken token);
    Task<DbConnection> GetConnectionByKeyAsync(
        string shardKey,
        CancellationToken token);
    
    Task<DbConnection> GetConnectionByBucketAsync(
        int bucketId,
        CancellationToken token);
}