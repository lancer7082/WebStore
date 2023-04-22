using System.Net;

namespace Ozon.Route256.Five.OrderService.Domain.Exceptions;

public class InvalidArgumentException : CustomException
{
    public InvalidArgumentException(string message) : base(message, null, HttpStatusCode.BadRequest)
    {
    }
}
