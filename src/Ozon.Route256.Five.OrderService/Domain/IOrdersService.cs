using Microsoft.AspNetCore.Mvc;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.Domain;

public interface IOrdersService
{
    /// <summary>
    /// Получить заказ по ид.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    public Task<OrderDto> GetOrderAsync(long orderId, CancellationToken token);

    /// <summary>
    /// Найти заказ по ид.
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    public Task<OrderDto?> FindOrderAsync(long orderId, CancellationToken token);

    /// <summary>
    /// Отмена заказа
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    public Task CancelOrderAsync(long orderId, CancellationToken token);

    /// <summary>
    /// Получить статус заказа
    /// </summary>
    /// <param name="orderId"></param>
    /// <returns></returns>
    public Task<OrderState> GetOrderStateAsync(long orderId, CancellationToken token);

    /// <summary>
    /// Получить список заказов по регионам
    /// </summary>
    /// <returns></returns>
    public Task<OrderDto[]> GetOrdersByRegionsAsync(string[]? regions, OrderSource? orderSource, PagingParams? paging, SortingParams? sorting, CancellationToken token);

    /// <summary>
    /// Получить список всех заказов клиента
    /// </summary>
    /// <param name="customerId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<OrderDto[]> GetOrdersByCustomerAsync(long customerId, DateTime dateBegin, PagingParams? paging, CancellationToken token);

    /// <summary>
    /// Получить статистику по заказам по регионам
    /// </summary>
    /// <param name="dateBegin"></param>
    /// <param name="regions"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<RegionSummaryDto[]> GetSummaryByRegionsAsync(string[] regions, DateTime dateBegin, CancellationToken token);

    /// <summary>
    /// Валидация заказа и отправка в топик new_orders
    /// </summary>
    /// <returns></returns>
    public Task<bool> CheckAndPublishNewOrderAsync(long orderId, CancellationToken token);

    /// <summary>
    /// Сохранение заказа (если еще нет в репозитории)
    /// </summary>
    /// <param name="orderDto"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> SaveIfNotExists(OrderDto orderDto, CancellationToken token);

    /// <summary>
    /// Обновление статуса заказа
    /// </summary>
    /// <param name="orderId"></param>
    /// <param name="newState"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<bool> UpdateOrderStateAsync(long orderId, OrderState newState, CancellationToken token);
}
