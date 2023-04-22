using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;
using Ozon.Route256.Five.OrderService.Domain.Dto.Filters;
using Ozon.Route256.Five.OrderService.Domain.Model;

namespace Ozon.Route256.Five.OrderService.API.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        /// <summary>
        /// 2.1 Ручка отмены заказа
        ///     Принимает один аргумент - номер заказа(long)
        /// </summary>
        /// <returns>
        ///     Если заказ отменен успешно - возвращает статус 200, если заказ отменить нельзя, возвращает ошибку 400 c описание причины, по которой заказ отменить нельзя.
        ///     Если такой заказ не найден - возвращает 404 также с описание ошибки.
        /// </returns>
        [HttpPut("{orderId}/cancel")]
        public async Task<ActionResult> CancelOrderAsync(long orderId, CancellationToken token)
        {
            await _ordersService.CancelOrderAsync(orderId, token);
            return Ok();
        }

        /// <summary>
        /// 2.2 Ручка возврата статуса заказа.
        ///     Принимает один аргумент - номер заказа.
        ///     Если такой заказ не найден - возвращает 404 также с описание ошибки.
        /// </summary>
        /// <returns>
        ///     При успешном выполнении возвращает статус заказа в логистике.
        /// </returns>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<OrderState>> GetOrderStateAsync(long orderId, CancellationToken token)
        {
            var orderState = await _ordersService.GetOrderStateAsync(orderId, token);
            return Ok(orderState);
        }

        /// <summary>
        /// 2.5 Ручка возврата списка заказов
        ///     Принимает в себя класс с одним параметром для фильтрации(регионы), 
        ///     а также параметры пагинации и признак сортировки по полю фильтрации.
        ///     Еще фильтрация по типу заказа.
        /// </summary>
        /// <returns>
        ///     Возвращает отфильтрованный список заказов или пустой список
        ///     Возвращает 400, если какой-то из регионов не найден.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<OrderDto[]>> GetOrdersAsync([FromQuery] OrdersFilterByRegions filter, [FromQuery] PagingParams? paging, [FromQuery] SortingParams? sorting, CancellationToken token)
        {
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var orders = await _ordersService.GetOrdersByRegionsAsync(filter.Regions, filter.OrderSource, paging, sorting, token);
            return Ok(orders);
        }

        /// <summary>
        /// 2.6 Ручка аггрегации заказов по региону
        ///     Принимает в себя параметр - дату/время, от которого нужно считать аггрегацию, 
        ///     а также список регионов по которым нужно будет осуществлять аггрегацию.
        /// </summary>
        /// <returns>
        ///     Если список регионов пустой - возвращаются все регионы.
        ///     Возвращает список регионов и количество заказов в нем, начиная с указанной даты.
        ///     Возвращает 400, если какой-то из регионов не найден.
        /// </returns>
        [HttpGet("summary")]
        public async Task<ActionResult<RegionSummaryDto[]>> GetSummaryByRegionsAsync([FromQuery] string[] regions, [FromQuery] DateTime dateBegin, CancellationToken token)
        {
            var summary = await _ordersService.GetSummaryByRegionsAsync(regions, dateBegin, token);
            return Ok(summary);
        }

        /// <summary>
        /// 2.7 Ручка получения всех заказов клиента
        ///     Принимает в себя параметр id клиента, дату/время с которого нужно получить данные, 
        ///     а также параметры пагинации. Сортировка не требуется.
        /// </summary>
        /// <returns>
        ///     Возвращает список заказов или пустой список, если таких заказов нет
        //      Возвращает 404, если такого клиента не существует.
        /// <returns>
        [HttpGet("getByCustomer/{customerId}")]
        public async Task<ActionResult<OrderDto[]>> GetOrdersByCustomerAsync(
            long customerId,
            [FromQuery] DateTime dateBegin,
            [FromQuery] PagingParams? paging,
            CancellationToken token
        )
        {
            var orders = await _ordersService.GetOrdersByCustomerAsync(customerId, dateBegin, paging, token);
            return Ok(orders);
        }
    }
}
