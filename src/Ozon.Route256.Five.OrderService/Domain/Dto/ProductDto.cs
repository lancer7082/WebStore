namespace Ozon.Route256.Five.OrderService.Domain.Dto;

public record ProductDto(
    long Id,        // Ид. продукта
    string Name,    // Наименование
    int Quantity,   // Кол-во
    double Price,   // Цена
    double Weight   // Вес
);
