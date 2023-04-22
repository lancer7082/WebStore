using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Ozon.Route256.Five.OrderService.Domain.Exceptions;
using System;
using System.Net;

namespace Ozon.Route256.Five.OrderService.API.Infrastructure;

public class CustomExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public CustomExceptionHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var response = context.Response;
            response.ContentType = "application/json";

            // get the response code and message
            var (status, message) = GetResponse(exception);
            response.StatusCode = (int)status;
            await response.WriteAsync(message);
        }
    }

    public (HttpStatusCode code, string message) GetResponse(Exception exception)
    {
        var code = exception switch
        {
            NotFoundException => HttpStatusCode.NotFound,
            InvalidArgumentException or ArgumentNullException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError,
        };
        return (code, JsonConvert.SerializeObject(new ErrorResult { Exception = exception.Message, StatusCode = (int)code }));
    }
}
