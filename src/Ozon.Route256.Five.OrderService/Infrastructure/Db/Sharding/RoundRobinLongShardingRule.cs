using Murmur;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;

public class RoundRobinLongShardingRule: IShardingRule<long>
{
    public int GetBucketId(
        long key,
        int bucketsCount)
    {
        var keyHashCode = GetHashCodeForKey(key);
        return Math.Abs(keyHashCode % bucketsCount);
    }

    private int GetHashCodeForKey(
        long key)
    {
        var murmur = MurmurHash.Create32();
        var bytes = BitConverter.GetBytes(key);
        var hash = murmur.ComputeHash(bytes);
        var result = BitConverter.ToInt32(hash);
        return result;
    }
}