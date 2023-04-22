namespace Ozon.Route256.Five.OrderService.Domain.Dto;

/// <summary>
/// Регион
/// </summary>
/// <param name="Name">Регион</param>
/// <param name="WarehouseAddress">Адрес склада</param>
public record RegionDto(
    string Name,
    AddressDto WarehouseAddress
);
