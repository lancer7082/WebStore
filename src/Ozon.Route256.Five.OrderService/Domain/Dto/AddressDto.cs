namespace Ozon.Route256.Five.OrderService.Domain.Dto;

/// <summary>
/// Адрес доставки
/// </summary>
public record AddressDto(
    string City,        // Город
    string Street,      // Улица
    string Building,    // Дом
    string Apartment,   // Квартира
    double Latitude,    // Широта
    double Longitude,   // Долгота
    string Region       // Регион
);