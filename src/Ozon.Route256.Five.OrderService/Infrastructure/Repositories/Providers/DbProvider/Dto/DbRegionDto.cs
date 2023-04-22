namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;

/// <summary>
/// Регион
/// </summary>
/// <param name="Id"></param>
/// <param name="Name">Регион</param>
/// <param name="WarehouseAddress">Адрес склада</param>
public record DbRegionDto(
    int Id,
    string Name,
    int WarehouseAddressId
)
{
    public DbRegionDto() : this(default, "", default) { }
};
