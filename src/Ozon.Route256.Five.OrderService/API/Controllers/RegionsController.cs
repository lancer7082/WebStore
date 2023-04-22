using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ozon.Route256.Five.OrderService.Domain;

namespace Ozon.Route256.Five.OrderService.API.Controllers
{
    [Route("api/regions")]
    [ApiController]
    public class RegionsController : ControllerBase
    {
        private readonly IRegionsService _regionsService;

        public RegionsController(IRegionsService regionsService)
        {
            _regionsService = regionsService;
        }

        /// <summary>
        /// 2.4 Ручка возврата списка регионов
        /// </summary>
        /// <returns>
        ///     Возвращает все регионы системы или пустой список.
        /// </returns>
        [HttpGet]
        public async Task<ActionResult<string[]>> GetRegionsAsync(CancellationToken token)
        {
            var regions = await _regionsService.GetAllAsync(token);
            return Ok(regions);
        }
    }
}
