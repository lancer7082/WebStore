namespace Ozon.Route256.Five.OrderService.Domain.Dto;

/// <summary>
/// Параметры пагинации
/// </summary>
/// <param name="pageNumber"></param>
/// <param name="pageSize"></param>
public record PagingParams(
    int PageNumber,
    int PageSize
);