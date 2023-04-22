namespace Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;

public interface IDbStore
{
    Task UpdateEndpointsAsync(IReadOnlyCollection<DbEndpoint> dbEndpoints);

    Task<DbEndpoint> GetNextEndpointAsync();

    Task<DbEndpoint> GetEndpointByBucketAsync(int bucketId);
    
    int BucketsCount { get; }
}