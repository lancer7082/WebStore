using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ozon.Route256.Five.OrderService.API.Controllers;

public class HomeController : ControllerBase
{
    // GET: /<controller>/
    public IActionResult Index()
    {
        return new RedirectResult("~/swagger");
    }
}
