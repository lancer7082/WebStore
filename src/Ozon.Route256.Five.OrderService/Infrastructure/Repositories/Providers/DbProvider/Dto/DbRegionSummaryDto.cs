namespace Ozon.Route256.Five.OrderService.Infrastructure.Repositories.Providers.DbProvider.Dto;

/// <summary>
/// Статистика по региону
/// </summary>
public record DbRegionSummaryDto(
    int RegionId,      // Регион
    long CountOrders,   // Количество заказов в регионе 
    double TotalSum,    // Общая сумма заказов
    double TotalWeight, // Суммарный вес
    long CountClients   // Количество клиентов, сделавших заказ в этом регионе
)
{
    public DbRegionSummaryDto() : this(default, default, default, default, default) { }
};
