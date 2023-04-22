namespace Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;

public interface IShardingRule<TShardKey>
{
    int GetBucketId(
        TShardKey key,
        int bucketsCount);
}