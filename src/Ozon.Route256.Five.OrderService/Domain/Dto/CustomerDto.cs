namespace Ozon.Route256.Five.OrderService.Domain.Dto;

/// <summary>
/// Клиент
/// </summary>
public record CustomerDto(
    long Id,                // Ид. клиента
    string FirstName,       // Имя
    string LastName,        // Фамилия
    string MobileNumber,    // Телефон
    string Email            // Email
);
