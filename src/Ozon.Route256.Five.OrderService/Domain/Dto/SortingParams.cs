namespace Ozon.Route256.Five.OrderService.Domain.Dto;

/// <summary>
/// Параметры сортировки
/// </summary>
/// <param name="fieldName"></param>
/// <param name="Ascending"></param>
public record SortingParams(
    string SortField = "",
    bool Ascending = true
);

