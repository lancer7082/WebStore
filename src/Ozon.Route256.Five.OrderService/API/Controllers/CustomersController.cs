using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ozon.Route256.Five.OrderService.Domain;
using Ozon.Route256.Five.OrderService.Domain.Dto;

namespace Ozon.Route256.Five.OrderService.API.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomersService _customersService;

        public CustomersController(ICustomersService customersService)
        {
            _customersService = customersService;
        }

        /// <summary>
        /// 2.3 Ручка возврата списка клиентов
        /// </summary>
        /// <returns>
        ///     Возвращает всех клиентов системы или пустой список.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<CustomerDto[]>> GetCustomersAsync(CancellationToken token)
        {
            var customers = await _customersService.GetAllAsync(token);
            return Ok(customers);
        }
    }
}
