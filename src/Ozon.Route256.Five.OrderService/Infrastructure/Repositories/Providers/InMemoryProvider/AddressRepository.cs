using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.InMemoryProvider;

public class AddressRepository : IAddressRepository
{
    private readonly InMemoryProvider _inMemoryStorage;

    public AddressRepository(InMemoryProvider inMemoryStorage)
    {
        _inMemoryStorage = inMemoryStorage;
    }

    #region Address

    public Task<DbAddressDto?> FindAsync(long id, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<DbAddressDto?>(token);

        return _inMemoryStorage.Addresses.TryGetValue(id, out var address)
            ? Task.FromResult<DbAddressDto?>(address)
            : Task.FromResult<DbAddressDto?>(null);
    }

    public Task<DbAddressDto[]> FindManyAsync(long[] ids, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<DbAddressDto[]>(token);

        var addresses = FindDto(ids, token).ToArray();
        return Task.FromResult(addresses);
    }

    public Task<DbAddressDto[]> GetAllAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<DbAddressDto[]>(token);
        return Task.FromResult(_inMemoryStorage.Addresses.Values.ToArray());
    }

    private IEnumerable<DbAddressDto> FindDto(IEnumerable<long> ids, CancellationToken token)
    {
        foreach (var id in ids)
        {
            token.ThrowIfCancellationRequested();
            if (_inMemoryStorage.Addresses.TryGetValue(id, out var address))
                yield return address;
        }
    }

    Task<long> IAddressRepository.InsertAsync(System.Data.IDbConnection connection, DbAddressDto addressDto, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    Task<DbAddressDto?> IAddressRepository.FindAsync(long id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    Task<DbAddressDto> IAddressRepository.GetAsync(long id, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Region

    public Task<string[]> GetAllRegionsAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<string[]>(token);

        var regions = _inMemoryStorage.Regions.Values
            .Select(x => x.Name)
            .ToArray();

        return Task.FromResult(regions);
    }

    public Task<DbRegionDto[]> FindRegionsByNamesAsync(string[] regions, CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Task.FromCanceled<DbRegionDto[]>(token);

        var regionsDto = _inMemoryStorage.Regions.Values
            .Where(x => regions.Contains(x.Name))
            .ToArray();

        return Task.FromResult(regionsDto);
    }

    public Task<DbRegionDto> GetRegionByNameAsync(string region, CancellationToken token)
    {
        var regionDto = _inMemoryStorage.Regions.Values.FirstOrDefault(x => x.Name.Equals(region));
        if (regionDto == null)
        {
            throw new NotFoundException($"Region {region} not found");
        }
        return Task.FromResult(regionDto);
    }

    Task<DbRegionDto> IAddressRepository.GetRegionByNameAsync(System.Data.IDbConnection connection, string region, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    Task<DbRegionDto> IAddressRepository.GetRegionByIdAsync(int regionId, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    Task<DbRegionDto[]> IAddressRepository.FindRegionsByIdsAsync(int[] ids, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    Task<DbAddressDto> IAddressRepository.GetAsync(long id, long shardKey, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    Task<DbAddressDto[]> IAddressRepository.FindManyAsync(Dictionary<long, long[]> idsByShardKey, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    #endregion
}