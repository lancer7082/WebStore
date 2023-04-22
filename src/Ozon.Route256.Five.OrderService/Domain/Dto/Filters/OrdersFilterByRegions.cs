using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Domain.Dto.Filters;

/// <summary>
/// Параметры фильтрации заказов
/// </summary>
/// <param name="region"></param>
public class OrdersFilterByRegions
{
    public string[]? Regions { get; set; }
    public OrderSource? OrderSource { get; set; }
};