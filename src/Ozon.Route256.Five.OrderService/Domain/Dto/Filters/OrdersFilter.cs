using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Domain.Dto.Filters;

/// <summary>
/// Параметры отбора заказов
/// </summary>
/// <param name="DateBegin"></param>
/// <param name="DateEnd"></param>
/// <param name="Regions"></param>
/// <param name="CustomerId"></param>
/// <param name="OrderSource"></param>
public class OrdersFilter
{
    public DateTime? DateBegin { get; set; }
    public DateTime? DateEnd { get; set; }
    public string[]? Regions { get; set; }
    public long? CustomerId { get; set; }
    public OrderSource? OrderSource { get; set; }
}
