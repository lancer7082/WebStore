using FluentAssertions;
using Ozon.Route256.Five.OrderService.Infrastructure.ClientBalancing;

namespace Ozon.Route256.Five.OrderService.Tests;
public class DbStoreTests
{
    private readonly DbStore _dbStore;

    public DbStoreTests()
    {
        _dbStore = new DbStore();

        _dbStore.UpdateEndpointsAsync(
            new[]
            {
            new DbEndpoint("testHost1", 5000, DbReplicaType.Master, new int[] {0,1,2,3}),
            new DbEndpoint("testHost2", 5001, DbReplicaType.Master, new int[] {4,5,6,7}),
            });
    }

    [Fact]
    public async Task GetNextEndpointAsync_Successful()
    {
        var result = await _dbStore.GetNextEndpointAsync();
        result.Host.Should().Be("testHost1");

        result = await _dbStore.GetNextEndpointAsync();
        result.Host.Should().Be("testHost2");

        result = await _dbStore.GetNextEndpointAsync();
        result.Host.Should().Be("testHost1");
    }
}