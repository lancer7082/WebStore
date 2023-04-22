using Ozon.Route256.Five.OrderService.Infrastructure.Repositories;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using System.Globalization;
using System.Security.AccessControl;

namespace Ozon.Route256.Five.OrderService.Domain;

public class RegionsService : IRegionsService
{
    private readonly IAddressRepository _addressRepository;

    public RegionsService(IAddressRepository addressRepository)
    {
        _addressRepository = addressRepository;
    }

    public async Task<string[]> GetAllAsync(CancellationToken token)
    {
        if (token.IsCancellationRequested)
            return Array.Empty<string>();

        return await _addressRepository.GetAllRegionsAsync(token);
    }
}
