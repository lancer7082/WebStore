using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Model;
using System.ComponentModel.DataAnnotations;

namespace Ozon.Route256.Five.OrderService.Domain;

public class OrderValidationAttribute : ValidationAttribute
{
    /// <summary>
    ///  Проверка заказа:
    ///     Проверяем расстояние между адресом в заказе и складом региона
    ///     Если расстояние более 5000, то заказ не валидиный 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value == null)
        {
            return new ValidationResult("Empty value");
        }

        var order = (OrderDto)value;

        if (order.Id == 0)
        {
            return new ValidationResult("Incorrect order id");
        }

        if (order.Address == null)
        {
            return new ValidationResult($"Address for order {order.Id} is empty");
        }

        if (string.IsNullOrEmpty(order.Region))
        {
            return new ValidationResult($"Region for order {order.Id} is empty");
        }

        var addressRepository = context.GetRequiredService<IAddressRepository>();

        // Получение склада по региону
        var regionDb = addressRepository.GetRegionByNameAsync(order.Region, default).Result;
        var warehouseAddressDb = addressRepository.GetAsync(regionDb.WarehouseAddressId, order.Id, default).Result;

        // Координаты из заказа        
        var orderCoord = new Coordinates(order.Address.Latitude, order.Address.Longitude);

        // Координаты склада
        var warehousCoord = new Coordinates(warehouseAddressDb.Latitude, warehouseAddressDb.Longitude);

        // Определение расстояния 
        var dist = orderCoord.DistanceToKm(warehousCoord);

        if (dist > 5000)
        {
            return new ValidationResult($"Incorrect region. Warehouse is too far (distance = {dist} km)");
        }

        return ValidationResult.Success;
    }
}

