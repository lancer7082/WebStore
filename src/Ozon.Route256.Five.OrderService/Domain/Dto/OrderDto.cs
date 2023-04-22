using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Domain.Dto;

/// <summary>
/// Заказ
/// </summary>
[OrderValidation]
public record OrderDto(
    long Id,                    // Id заказа 
    int Quantity,               // Количество товаров в заказе
    double Sum,                 // Общая сумма заказа
    double Weight,              // Общий вес заказа
    OrderSource OrderSource,    // Тип заказа
    DateTime Date,              // Дата заказа
    string Region,              // Откуда сделан заказ (регион)
    OrderState OrderState,      // Статус заказа
    CustomerDto Customer,       // Клиент
    AddressDto Address          // Адрес доставки
);