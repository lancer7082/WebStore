using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;
using System.Data;

namespace Ozon.Route256.Five.OrderService.Domain;

public interface IAddressRepository
{
    #region Address

    public Task<DbAddressDto?> FindAsync(long id, CancellationToken token);
    
    public Task<DbAddressDto> GetAsync(long id, CancellationToken token);

    public Task<DbAddressDto[]> FindManyAsync(long[] ids, CancellationToken token);

    public Task<DbAddressDto[]> GetAllAsync(CancellationToken token);

    public Task<string[]> GetAllRegionsAsync(CancellationToken token);

    Task<long> InsertAsync(IDbConnection connection, DbAddressDto addressDto, CancellationToken token);

    #endregion

    #region Address (with sharding)

    Task<DbAddressDto> GetAsync(long id, long shardKey, CancellationToken token);

    Task<DbAddressDto[]> FindManyAsync(Dictionary<long, long[]> idsByShardKey, CancellationToken token);

    #endregion

    #region Region

    Task<DbRegionDto[]> FindRegionsByIdsAsync(int[] ids, CancellationToken token);

    Task<DbRegionDto[]> FindRegionsByNamesAsync(string[] regions, CancellationToken token);

    Task<DbRegionDto> GetRegionByNameAsync(string region, CancellationToken token);

    Task<DbRegionDto> GetRegionByNameAsync(IDbConnection connection, string region, CancellationToken token);

    Task<DbRegionDto> GetRegionByIdAsync(int regionId, CancellationToken token);

    #endregion
}