using System.Text;
using Murmur;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Db.Sharding;

public class RoundRobinStringShardingRule: IShardingRule<string>
{
    public int GetBucketId(
        string key,
        int bucketsCount)
    {
        var keyHashCode = GetHashCodeForKey(key);
        return Math.Abs(keyHashCode % bucketsCount);
    }

    private int GetHashCodeForKey(
        string key)
    {
        var murmur = MurmurHash.Create32();
        var bytes = Encoding.UTF8.GetBytes(key);
        var hash = murmur.ComputeHash(bytes);
        var result = BitConverter.ToInt32(hash);
        return result;
    }
}