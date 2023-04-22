using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;

/// <summary>
/// Заказ
/// </summary>
public record DbOrderDto(
    long Id,               // Id заказа 
    int Quantity,               // Количество товаров в заказе
    double Sum,                 // Общая сумма заказа
    double Weight,              // Общий вес заказа
    byte OrderSource,           // Тип заказа
    DateTime Date,              // Дата заказа
    int RegionId,               // Откуда сделан заказ (регион)
    byte OrderState,            // Статус заказа
    long CustomerId,            // Клиент
    long AddressId              // Адрес доставки
)
{
    public DbOrderDto() : this(default, default, default, default, default, default, default, default, default, default) { }
};
