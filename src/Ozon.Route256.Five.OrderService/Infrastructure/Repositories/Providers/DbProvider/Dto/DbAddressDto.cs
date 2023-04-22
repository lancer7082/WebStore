namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;

/// <summary>
/// Адрес доставки
/// </summary>
public record DbAddressDto(
    long Id,
    string City,        // Город
    string Street,      // Улица
    string Building,    // Дом
    string Apartment,   // Квартира
    double Latitude,    // Широта
    double Longitude,   // Долгота
    int RegionId       // Регион
)
{
    public DbAddressDto() : this(default, "", "", "", "", default, default, default) { }
}
