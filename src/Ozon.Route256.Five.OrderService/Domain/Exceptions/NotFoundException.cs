using System.Net;

namespace Ozon.Route256.Five.OrderService.Domain.Exceptions;

public class NotFoundException : CustomException
{
    public NotFoundException(string message) : base(message, null, HttpStatusCode.NotFound)
    {
    }
}
