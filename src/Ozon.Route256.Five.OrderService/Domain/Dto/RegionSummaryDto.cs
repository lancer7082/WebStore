namespace Ozon.Route256.Five.OrderService.Domain.Dto;

/// <summary>
/// Статистика по региону
/// </summary>
public record RegionSummaryDto(
    string Region,      // Регион
    long CountOrders,   // Количество заказов в регионе 
    double TotalSum,    // Общая сумма заказов
    double TotalWeight, // Суммарный вес
    long CountClients   // Количество клиентов, сделавших заказ в этом регионе
)
{
    public RegionSummaryDto() : this("", default, default, default, default) { }
};
